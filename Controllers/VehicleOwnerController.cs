using GateHub.Dtos;
using GateHub.Models;
using GateHub.repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GateHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleOwnerController : ControllerBase
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;
        private readonly GateHubContext context;
        private readonly IGenerateTokenService generateTokenService;
        private readonly IVehicleOwnerRepo vehicleOwnerRepo;

        public VehicleOwnerController(SignInManager<AppUser> signInManager,UserManager<AppUser> userManager,GateHubContext context,IGenerateTokenService generateTokenService,IVehicleOwnerRepo vehicleOwnerRepo)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.context = context;
            this.generateTokenService = generateTokenService;
            this.vehicleOwnerRepo = vehicleOwnerRepo;
        }


        [HttpPost("register-vehicle-owner")]
        public async Task<IActionResult> RegisterVehicleOwner([FromBody] VehicleOwnerRegistrationDto dto)
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
            var roleResult = await userManager.AddToRoleAsync(user, "VehicleOwner");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            var vehicleOwner = new VehicleOwner
            {
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                License = dto.License,
                Balance = dto.Balance,
                AppUserId = user.Id,
                appUser = user
            };

            context.VehicleOwners.Add(vehicleOwner);
            await context.SaveChangesAsync();

            return Ok(vehicleOwner);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] VehicleOwnerLoginDto dto)
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


            // Verify that the user is a vehicle owner by checking if a VehicleOwner record exists for this user.
            var vehicleOwner = await context.VehicleOwners.FirstOrDefaultAsync(vo => vo.AppUserId == user.Id);
            if (vehicleOwner == null)
                return Unauthorized("User is not registered as a vehicle owner.");


            var tokenString = generateTokenService.GenerateJwtTokenAsync(user);

            return Ok(new { user, tokenString });

        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok("logout successful");
        }


        [HttpGet("VOProfile")]
        public async Task<IActionResult> GetVehicleOwnerProfile()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return Unauthorized();

            var userId = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

            //var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }
            var owner = await context.VehicleOwners
                .Include(vo => vo.Vehicles)
                .Include(vo => vo.Transactions)
                .FirstOrDefaultAsync(vo => vo.AppUserId == userId);

            if (owner == null)
            {
                return NotFound("Vehicle owner profile not found.");
            }

            return Ok(owner);
        }
    }
}
