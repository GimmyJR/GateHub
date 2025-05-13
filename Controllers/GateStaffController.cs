using GateHub.Dtos;
using GateHub.Models;
using GateHub.repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace GateHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GateStaffController : ControllerBase
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;
        private readonly IConfiguration configuration;
        private readonly IGateStaffRepo gateStaffRepo;
        private readonly IGenerateTokenService generateTokenService;
        private readonly GateHubContext context;
        private readonly ITokenBlacklistService blacklistService;

        public GateStaffController(SignInManager<AppUser> signInManager,UserManager<AppUser> userManager,IConfiguration configuration,IGateStaffRepo gateStaffRepo,IGenerateTokenService generateTokenService,GateHubContext context,ITokenBlacklistService blacklistService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.configuration = configuration;
            this.gateStaffRepo = gateStaffRepo;
            this.generateTokenService = generateTokenService;
            this.context = context;
            this.blacklistService = blacklistService;
        }

        [HttpPost("register-gatestaff")]
        public async Task<IActionResult> RegisterGateStaff([FromBody] GateStaffRegistrationDto dto)
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
            var roleResult = await userManager.AddToRoleAsync(user, "GateStaff");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            var gateStaff = new GateStaff
            {
                PhoneNumber = dto.PhoneNumber,
                GateId = dto.GateId,
                AppUserId = user.Id,
                appUser = user
            };

            await gateStaffRepo.AddGateStaff(gateStaff);
            if (!string.IsNullOrEmpty(dto.DeviceToken))
            {
                user.DeviceToken = dto.DeviceToken;
                await userManager.UpdateAsync(user);
            }

            return Ok(gateStaff);
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

            if (role != "GateStaff")
            {
                return Unauthorized("Invalid Credentials");
            }

            var tokenString = generateTokenService.GenerateJwtTokenAsync(user);
            if (!string.IsNullOrEmpty(dto.DeviceToken))
            {
                user.DeviceToken = dto.DeviceToken;
                await userManager.UpdateAsync(user);
            }

            return Ok(new { user, tokenString, role });

        }


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

        [HttpPost("AddFine")]
        public async Task<IActionResult> AddFine([FromBody] FineCreationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vehicle = await gateStaffRepo.FindVehicleByPlateNumber(dto.PlateNumber);
            
            if (vehicle == null)
                return NotFound("Vehicle not found.");
            

            var gateFee = await gateStaffRepo.RetrieveTheFeeOfTheVehilceAndGate(dto.GateId, vehicle.Type);

            if (gateFee == null)
                return BadRequest("No fee record found for this vehicle type at the specified gate.");

            // Create a new VehicleEntry record for the fine.
            var fineEntry = new VehicleEntry
            {
                FeeValue = gateFee.Fee, 
                FineValue = dto.FineValue,
                FineType = dto.FineType,
                Date = DateTime.Now,
                IsPaid = false,
                VehicleId = vehicle.Id,
                GateId = dto.GateId
            };
            await gateStaffRepo.AddFine(fineEntry);

            return Ok(new { message = $"Fine {fineEntry} added and notification sent successfully." });
        }


    }
}


