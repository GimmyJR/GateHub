using GateHub.Dtos;
using GateHub.Hubs;
using GateHub.Models;
using GateHub.repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

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
        private readonly PaymobService paymobService;
        private readonly IHubContext<NotificationHub> hubContext;

        public VehicleOwnerController(SignInManager<AppUser> signInManager,UserManager<AppUser> userManager,GateHubContext context,IGenerateTokenService generateTokenService,IVehicleOwnerRepo vehicleOwnerRepo,PaymobService paymobService,IHubContext<NotificationHub> hubContext)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.context = context;
            this.generateTokenService = generateTokenService;
            this.vehicleOwnerRepo = vehicleOwnerRepo;
            this.paymobService = paymobService;
            this.hubContext = hubContext;
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

            await vehicleOwnerRepo.AddVehicleOwner(vehicleOwner);

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

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var owner = await vehicleOwnerRepo.VOProfile(userId);

            if (owner == null)
            {
                return NotFound("Vehicle owner profile not found.");
            }

            return Ok(owner);
        }

        [HttpGet("VOEntries")]
        public async Task<IActionResult> GetMyVehicleEntries()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return Unauthorized();

            var userId = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            // Find the vehicle owner based on the AppUserId
            var owner = await vehicleOwnerRepo.GetVehicleOwner(userId);

            if (owner == null)
                return NotFound("Vehicle owner not found.");

            // Get all vehicle entries associated with the owner's vehicles
            var vehicleEntries = await vehicleOwnerRepo.GetVehicleOwnerEntries(owner);

            return Ok(vehicleEntries);
        }

        [HttpPost("SubmitObjection")]
        public async Task<IActionResult> SubmitObjection([FromBody] ObjectionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return Unauthorized();

            var userId = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            // Find the vehicle owner associated with the logged-in user
            var owner = await vehicleOwnerRepo.GetVehicleOwner(userId);

            if (owner == null)
                return NotFound("Vehicle owner not found.");


            // Verify that the VehicleEntry exists and belongs to the owner
            var vehicleEntry = await vehicleOwnerRepo.CheckVehicleEntry(dto.VehicleEntryId,owner);

            if (vehicleEntry == null)
                return NotFound("Vehicle entry not found or does not belong to this owner.");

            var objection = new Objection
            {
                VehicleEntryId = dto.VehicleEntryId,
                VehicleOwnerId = owner.Id,
                Description = dto.Description,
                date = DateTime.UtcNow,
                Statue = "Pending"  // Default status
            };

            await vehicleOwnerRepo.AddObjection(objection);

            return Ok(new { message = "Objection submitted successfully.", objection });
        }

        [HttpPost("pay-vehicle-entry/{vehicleEntryId}")]
        public async Task<IActionResult> PayVehicleEntry(int vehicleEntryId)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return Unauthorized();

            var userId = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }


            var owner = await vehicleOwnerRepo.GetVehicleOwner(userId);

            if (owner == null)
                return NotFound("Vehicle owner not found.");

            var vehicleEntry = await vehicleOwnerRepo.CheckVehicleEntry(vehicleEntryId,owner);

            if (vehicleEntry == null)
                return NotFound("Vehicle entry not found or does not belong to the owner.");

            if (vehicleEntry.IsPaid)
                return BadRequest("This entry has already been paid.");

            string paymentUrl = await paymobService.InitiatePayment(owner, vehicleEntry.FeeValue +(vehicleEntry.FineValue ?? 0) , vehicleEntry.Id, "Vehicle Entry Payment");
            
            return Ok(new { message = "Redirect to this URL for payment", paymentUrl });

        }

        [HttpPost("recharge-balance")]
        public async Task<IActionResult> RechargeBalance([FromBody] BalanceRechargeDto dto)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return Unauthorized();

            var userId = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }


            var owner = await vehicleOwnerRepo.GetVehicleOwner(userId);

            if (owner == null)
                return NotFound("Vehicle owner not found.");

            string paymentUrl = await paymobService.InitiatePayment(owner, dto.Amount, 0, "Balance Recharge");
            return Ok(new { message = "Redirect to this URL for payment", paymentUrl });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> PaymentWebhook([FromBody] PaymobWebhookDto data)
        {
            try
            {
                Console.WriteLine("Webhook Received: " + JsonSerializer.Serialize(data));

                if (data?.Obj == null || data.Obj.Order == null)
                {
                    Console.WriteLine("Invalid webhook data.");
                    return BadRequest("Invalid webhook request.");
                }

                // Extract data safely
                string status = data.Obj.Success;
                string orderId = data.Obj.Order.MerchantOrderId;
                decimal amountPaid = data.Obj.AmountCents / 100m; // Convert cents to EGP

                if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(orderId))
                {
                    Console.WriteLine("Missing required fields in webhook data.");
                    return BadRequest("Invalid webhook request.");
                }

                if (status != "true")
                {
                    Console.WriteLine("Payment was not successful.");
                    return BadRequest("Payment not successful.");
                }

                // Find vehicle entry by orderId
                var vehicleEntry = await vehicleOwnerRepo.FindVehicleEntry(int.Parse(orderId));

                if (vehicleEntry != null)
                {
                    vehicleEntry.IsPaid = true;
                    string paymentType = vehicleEntry.FineValue > 0 ? "Fee and Fine" : "Fee";

                    var transaction = new Transaction
                    {
                        Amount = vehicleEntry.FeeValue + (vehicleEntry.FineValue ?? 0),
                        PaymentType = paymentType,
                        TransactionDate = DateTime.UtcNow,
                        Status = "Success",
                        VehicleOwnerId = vehicleEntry.vehicle.VehicleOwnerId
                    };

                    await vehicleOwnerRepo.AddTransaction(transaction);

                    Console.WriteLine($"Vehicle Entry {vehicleEntry.Id} marked as paid.");

                    // Send real-time notification via SignalR
                    await hubContext.Clients.User(vehicleEntry.vehicle.VehicleOwner.AppUserId)
                        .SendAsync("ReceiveNotification", new
                        {
                            Title = "Payment Successful",
                            Message = $"Your payment of {amountPaid} EGP has been received.",
                            Date = DateTime.UtcNow
                        });
                }
                else
                {
                    Console.WriteLine($"No matching vehicle entry found for Order ID: {orderId}");
                    return NotFound("No matching order found.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Webhook: " + ex.Message);
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }



    }
}
