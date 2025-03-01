using GateHub.Dtos;
using GateHub.Dtos.AdminDto;
using GateHub.Models;
using GateHub.repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        public AdminController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager,IConfiguration configuration,IAdminRepo adminRepo)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.adminRepo = adminRepo;
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
            var result = await userManager.CreateAsync(user,dto.Password);
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
            
            if(user == null)
            {
                return Unauthorized("Invalid Credentials");
            }    

            var passcheck = await signInManager.CheckPasswordSignInAsync(user,dto.Password,lockoutOnFailure:false);
            
            if (!passcheck.Succeeded) 
            {
                return Unauthorized("Invalid Credentials");
            }

            bool hasRole = await userManager.IsInRoleAsync(user, dto.Role);
            if (!hasRole)
            {
                return Unauthorized($"User does not have the role '{dto.Role}'.");
            }

            var tokenString = adminRepo.GenerateJwtTokenAsync(user);

            return Ok(new { user ,tokenString});

        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok("logout successful");
        }



    }
}
