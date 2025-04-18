using GateHub.Dtos;
using GateHub.Models;
using GateHub.repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GateHub.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly IAdminRepo adminRepo;
        private readonly IGenerateTokenService generateTokenService;
        private readonly GateHubContext context;

        public AdminController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration, IAdminRepo adminRepo, IGenerateTokenService generateTokenService, GateHubContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.adminRepo = adminRepo;
            this.generateTokenService = generateTokenService;
            this.context = context;
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegistrationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = new AppUser
            {
                UserName = dto.NatId,
                Name = dto.FullName,
                NatId = dto.NatId,
                PhoneNumber = dto.PhoneNumber,
                BirthDate = dto.BirthDate,
                Gender = dto.Gender,
            };
            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            var roleResult = await userManager.AddToRoleAsync(user, "Admin");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            return Ok(user);
        }

        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] MultiRoleLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await userManager.FindByNameAsync(dto.NatId);

            if (user == null)
            {
                return Unauthorized("Invalid Credentials");
            }

            var passcheck = await signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);

            if (!passcheck.Succeeded)
            {
                return Unauthorized("Invalid Credentials");
            }

            bool hasRole = await userManager.IsInRoleAsync(user, dto.Role);
            if (!hasRole)
            {
                return Unauthorized($"User does not have the role '{dto.Role}'.");
            }

            var role = (await userManager.GetRolesAsync(user)).FirstOrDefault();

            if (role != "Admin")
            {
                return Unauthorized("User is not assigned to the 'Admin' role.");
            }

            var tokenString = generateTokenService.GenerateJwtTokenAsync(user);

            return Ok(new { user, tokenString, role });

        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok("logout successful");
        }

        
        [HttpPost("add-gate")]
        public async Task<IActionResult> AddGate([FromBody] GateCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await adminRepo.AddGate(dto);

            return Ok(dto);
        }

        
        [HttpGet("GetGateById/{id}")]
        public async Task<IActionResult> GetGateById(int id)
        {
            var gate = await adminRepo.GetGateById(id);

            if (gate == null)
                return NotFound();

            return Ok(gate);
        }


        [HttpPost("Add-vehicle")]
        public async Task<IActionResult> AddVehicle(VehicleCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await adminRepo.AddVehicle(dto);

            return Ok(dto);
        }
        
        
        [HttpGet("GetVehicleById/{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            var vehicle = await adminRepo.GetVehicleById(id);
            if (vehicle == null)
                return NotFound();
            return Ok(vehicle);
        }

        
        [HttpPost("AddLostVehicle")]
        public async Task<IActionResult> AddLostVehicle([FromBody] LostVehicleCreationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find the vehicle by plate number 
            var vehicle = await adminRepo.FindVehicleByPlateNumber(dto.PlateNumber);

            if (vehicle == null)
                return NotFound("Vehicle with the provided plate number was not found.");

            await adminRepo.AddLostVehicle(dto);

            return Ok(dto);
        }

        
        [HttpGet("getallLostVehicles")]
        public async Task<IActionResult> GetAllLostVehicles()
        {
            var lostVehicles = await adminRepo.GetAllLostVehicles();

            return Ok(lostVehicles);
        }

        
        [HttpPost("RecoverVehicle")]
        public async Task<IActionResult> RecoverVehicle (RecoverVehicleDTO dto)
        {
            if (! ModelState.IsValid)
            {
                return BadRequest(ModelState);  
            }

            await   adminRepo.RecoverVehicle(dto);
            return Ok(dto);

        }

        
        [HttpPost("AddFee")]
        public async Task<IActionResult> AddGateFee([FromBody] GateFeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify that the gate exists
            var gate = await context.Gates.FindAsync(dto.GateId);
            if (gate == null)
                return NotFound("Gate not found.");

            // Check if a fee for this gate and vehicle type already exists
            var existingFee = await context.GateFees
                .FirstOrDefaultAsync(gf => gf.GateId == dto.GateId && gf.VehicleType.ToLower() == dto.VehicleType.ToLower());
            if (existingFee != null)
                return BadRequest("A fee for this vehicle type at the specified gate already exists.");

            // Create the new GateFee record
            var gateFee = new GateFee
            {
                GateId = dto.GateId,
                VehicleType = dto.VehicleType,
                Fee = dto.Fee
            };

            context.GateFees.Add(gateFee);
            await context.SaveChangesAsync();

            return Ok(gateFee);
        }



        // dashboard actions

        [HttpGet("VehicleCount")]
        public async Task<IActionResult> TotalVehicleEnteredToDay ()
        {
            var count = await adminRepo.VehicleCount();
            if (count != null)
            {
                return Ok(count);
            }
            else
                return BadRequest();

        }

        [HttpGet ("TotalRevenue")]
        public async Task<IActionResult> TotalRevenueMonthly ()
        {
            var revenue = await adminRepo.TotalRevenue();
            if (revenue != null) 
            { 
                return Ok(revenue);
            }
            else 
            { 
                return BadRequest(); 
            }

        }

        [HttpGet("LostVehicleCount")]
        public async Task<IActionResult> GetLostVehicleCount ()
        {
            var count = await adminRepo.GetLostVehicleCount();
            return Ok(count);
        }

        [HttpGet("DailyReport")]
        public async Task<IActionResult> GetDailyReport()
        {
            
            var hourlyData = await adminRepo.GetDailyReport();

            return Ok(hourlyData);
        }


        [HttpGet("RecentCars")]
        public async Task<IActionResult> GetRecentCars()
        {
            var recent = await adminRepo.GetRecentCars();

            return Ok(recent);
        }

        [HttpGet("TopGatesToday")]
        public async Task<IActionResult> GetTopGatesToday()
        {
            var topGates = await adminRepo.GetTopGatesToday();

            return Ok(topGates);
        }

        [HttpGet("GetVehicleByPlateNumberWithOwner")]
        public async Task<IActionResult> GetVehicleByPlateNumberWithOwner(string plateNum)
        {
            var vehicle = await adminRepo.GetVehicleByPlateNumberWithOwner(plateNum);
            if (vehicle == null)
                return NotFound("Vehicle not found with the given Plate Number");

            return Ok(vehicle);
        }

        [HttpGet("GetOwnerWithVehicles")]
        public async Task<IActionResult> GetOwnerWithVehicles(string natId)
        {
            var result = await adminRepo.GetOwnerWithVehiclesByNatId(natId);
            if (result == null)
                return NotFound("Vehicle owner not found with the given National ID");

            return Ok(result);
        }

        [HttpGet("GetLostVehicleByPlate")]
        public async Task<IActionResult> GetLostVehicleByPlate(string plateNumber)
        {
            var lostVehicle = await adminRepo.GetLostVehicleByPlate(plateNumber);
            if (lostVehicle == null)
            {
                return NotFound("No lost vehicle found with this plate number.");
            }

            return Ok(new
            {
                lostVehicle.Id,
                lostVehicle.PlateNumber,
                lostVehicle.ReportedDate,
                lostVehicle.IsFound,
                Vehicle = new
                {
                    lostVehicle.Vehicle.Id,
                    lostVehicle.Vehicle.ModelCompany,
                    lostVehicle.Vehicle.ModelDescription,
                    lostVehicle.Vehicle.Color,
                    lostVehicle.Vehicle.Type,
                    lostVehicle.Vehicle.PlateNumber,
                    Owner = new
                    {
                        lostVehicle.Vehicle.VehicleOwner?.PhoneNumber,
                        lostVehicle.Vehicle.VehicleOwner?.Address
                    }
                }
            });
        }


        [HttpGet("Objections")]
        public async Task<IActionResult> GetAllObjections ()
        {
            var objections = await adminRepo.GetAllObjection();
            if (objections == null)
                return NotFound();
           
            return Ok(objections);

        }

        
        [HttpGet("ObjectionDetails")]
        public async Task<IActionResult> GetObjectionDetails(int  objectionId)
        {
            var details = await adminRepo.GetObjectionDetialsByID(objectionId); 
            if (details != null)
            {
                return Ok (details);
            }
            return BadRequest("InValid Objection ID");

        }


        [HttpPatch("EditVehicle")]
        public async Task<IActionResult> EditVehicle(int vehicleId , [FromBody] UpdateVehicleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vehicleUpdated = await adminRepo.updateVehicle(vehicleId, dto);   
            
            if (vehicleUpdated != null) 
            {
                return Ok(vehicleUpdated);
            }
            return BadRequest("vehicle Not Found"); 
        }

    }
}
