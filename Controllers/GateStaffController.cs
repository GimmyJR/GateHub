﻿using GateHub.Dtos;
using GateHub.Models;
using GateHub.repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public GateStaffController(SignInManager<AppUser> signInManager,UserManager<AppUser> userManager,IConfiguration configuration,IGateStaffRepo gateStaffRepo,IGenerateTokenService generateTokenService,GateHubContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.configuration = configuration;
            this.gateStaffRepo = gateStaffRepo;
            this.generateTokenService = generateTokenService;
            this.context = context;
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

            var tokenString = generateTokenService.GenerateJwtTokenAsync(user);

            return Ok(new { user, tokenString });

        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok("logout successful");
        }
    }
}
