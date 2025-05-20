using GateHub.Dtos;
using GateHub.Models;
using GateHub.repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GateHub.Controllers
{
    
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
        private readonly ITokenBlacklistService blacklistService;

        public AdminController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration, IAdminRepo adminRepo, IGenerateTokenService generateTokenService, GateHubContext context,ITokenBlacklistService blacklistService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.adminRepo = adminRepo;
            this.generateTokenService = generateTokenService;
            this.context = context;
            this.blacklistService = blacklistService;
        }
        [Authorize(Roles ="Admin")]
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
            if (!string.IsNullOrEmpty(dto.DeviceToken))
            {
                user.DeviceToken = dto.DeviceToken;
                await userManager.UpdateAsync(user);
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
            if (!string.IsNullOrEmpty(dto.DeviceToken))
            {
                user.DeviceToken = dto.DeviceToken;
                await userManager.UpdateAsync(user);
            }


            return Ok(new { user, tokenString, role });

        }

        [Authorize(Roles ="Admin")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var rawToken = HttpContext.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(rawToken);

            await blacklistService.BlacklistToken(rawToken, jwtToken.ValidTo);
            return Ok("Logged out successfully");
        }

        [Authorize(Roles ="Admin")]
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

        [Authorize(Roles ="Admin")]
        [HttpGet("GetGateById/{id}")]
        public async Task<IActionResult> GetGateById(int id)
        {
            var gate = await adminRepo.GetGateById(id);

            if (gate == null)
                return NotFound();

            return Ok(gate);
        }

        [Authorize(Roles ="Admin")]
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

        [Authorize(Roles ="Admin")]
        [HttpGet("GetVehicleById/{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            var vehicle = await adminRepo.GetVehicleById(id);
            if (vehicle == null)
                return NotFound();
            return Ok(vehicle);
        }

        [Authorize(Roles ="Admin")]
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

        [Authorize(Roles ="Admin")]
        [HttpGet("getallLostVehicles")]
        public async Task<IActionResult> GetAllLostVehicles()
        {
            var lostVehicles = await adminRepo.GetAllLostVehicles();

            return Ok(lostVehicles);
        }

        [Authorize(Roles ="Admin")]
        [HttpPost("RecoverVehicle")]
        public async Task<IActionResult> RecoverVehicle (RecoverVehicleDTO dto)
        {
            if (! ModelState.IsValid)
            {
                return BadRequest(ModelState);  
            }
            var res = await adminRepo.RecoverVehicle(dto);
            return Ok(res);
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
        [Authorize(Roles ="Admin")]
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

        [Authorize(Roles ="Admin")]
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


        [Authorize(Roles ="Admin")]
        [HttpGet("LostVehicleCount")]
        public async Task<IActionResult> GetLostVehicleCount ()
        {
            var count = await adminRepo.GetLostVehicleCount();
            return Ok(count);
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("DailyReport")]
        public async Task<IActionResult> GetDailyReport()
        {
            
            var hourlyData = await adminRepo.GetDailyReport();

            return Ok(hourlyData);
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("RecentCars")]
        public async Task<IActionResult> GetRecentCars()
        {
            var recent = await adminRepo.GetRecentCars();

            return Ok(recent);
        }
        
        [Authorize(Roles ="Admin")]
        [HttpGet("TopGatesToday")]
        public async Task<IActionResult> GetTopGatesToday()
        {
            var topGates = await adminRepo.GetTopGatesToday();

            return Ok(topGates);
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("GetRecentLostVehicleAlerts")]
        public async Task<IActionResult> GetRecentLostVehicleAlerts()
        {
            var alerts = await adminRepo.GetRecentLostVehicleAlerts();

            var formattedAlerts = alerts.Select(a => new
            {
                TimeAgo = adminRepo.GetTimeAgo(a.DetectedTime),
                Message = $"Lost vehicle <strong>{a.PlateNumber}</strong> detected at {a.Gate}."
            });

            return Ok(formattedAlerts);
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("GetVehicleByPlateNumberWithOwner")]
        public async Task<IActionResult> GetVehicleByPlateNumberWithOwner(string plateNum)
        {
            var vehicle = await adminRepo.GetVehicleByPlateNumberWithOwner(plateNum);
            if (vehicle == null)
                return NotFound("Vehicle not found with the given Plate Number");

            return Ok(vehicle);
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("GetOwnerWithVehicles")]
        public async Task<IActionResult> GetOwnerWithVehicles(string natId)
        {
            var result = await adminRepo.GetOwnerWithVehiclesByNatId(natId);
            if (result == null)
                return NotFound("Vehicle owner not found with the given National ID");

            return Ok(result);
        }

        [Authorize(Roles ="Admin")]
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

        [Authorize(Roles ="Admin")]
        [HttpPost("AcceptObjection/{id}")]
        public async Task<IActionResult> AcceptObjection(int id)
        {
            var objection = await adminRepo.AcceptObjection(id);
            if (objection == null)
                return NotFound("Objection not found.");

            return Ok(new { message = "Objection accepted successfully" });
        }

        [Authorize(Roles ="Admin")]
        [HttpPost("RejectObjection/{id}")]
        public async Task<IActionResult> RejectObjection(int id)
        {
            var objection = await adminRepo.RejectObjection(id);
            if (objection == null)
                return NotFound("Objection not found");

            return Ok(new { message = "Objection rejected and value increased by 10%" });
        }

        [Authorize(Roles ="Admin")]
        [HttpPatch("update-vehicle-owner")]
        public async Task<IActionResult> UpdateVehicleOwner(int OwnerId,[FromBody] VehicleOwnerUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var owner = await adminRepo.UpdateVehicleOwner(OwnerId,dto);
            if (owner == null)
                return NotFound("Vehicle owner not found.");

            return Ok(new { message = "Vehicle owner updated successfully.", owner });
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("Objections")]
        public async Task<IActionResult> GetAllObjections ()
        {
            var objections = await adminRepo.GetAllObjection();
            if (objections == null)
                return NotFound();
           
            return Ok(objections);

        }

        [Authorize(Roles ="Admin")]
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

        [Authorize(Roles ="Admin")]
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

        [Authorize(Roles ="Admin")]
        [HttpGet("Profile")]
        public async Task<IActionResult> AdminProfile()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return Unauthorized("Token is missing.");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            var userId = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;


            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User not found.");

            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin"))
                return Forbid("Access denied. Admins only.");

            return Ok(new
            {
                user.Name,
                user.Id,
                user.UserName,
                user.Email,
                user.NatId
            });
        }

    }


    
}
