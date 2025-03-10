﻿using GateHub.Dtos;
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

    }
}
