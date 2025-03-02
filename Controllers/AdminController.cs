using GateHub.Dtos;
using GateHub.Models;
using GateHub.repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

            var tokenString = generateTokenService.GenerateJwtTokenAsync(user);

            return Ok(new { user, tokenString });

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

            var gate = new Gate
            {
                Type = dto.Type,
                AddressName = dto.AddressName,
                AddressCity = dto.AddressCity,
                AddressGovernment = dto.AddressGovernment,
                GateStaffs = new List<GateStaff>(),
                VehicleEntries = new List<VehicleEntry>()
            };

            context.Gates.Add(gate);
            await context.SaveChangesAsync();

            return Ok(gate);
        }

        [HttpGet("GetGateById/{id}")]
        public async Task<IActionResult> GetGateById(int id)
        {
            var gate = await context.Gates
                .Include(G => G.GateStaffs)
                .Include(G => G.VehicleEntries)
                .FirstOrDefaultAsync(G => G.Id == id);
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

            var vehicle = new Vehicle
            {
                PlateNumber = dto.PlateNumber,
                LicenseStart = dto.LicenseStart,
                LicenseEnd = dto.LicenseEnd,
                ModelDescription = dto.ModelDescription,
                ModelCompany = dto.ModelCompany,
                Color = dto.Color,
                Type = dto.Type,
                RFID = dto.RFID,
                VehicleOwnerId = dto.VehicleOwnerId,
            };


            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();

            return Ok(vehicle);
        }
        [HttpGet("GetVehicleById/{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            var vehicle = await context.Vehicles.Include(v => v.VehicleEntries)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
                return NotFound();
            return Ok(vehicle);
        }

    }
}
