using GateHub.Dtos;
using GateHub.Hubs;
using GateHub.Models;
using GateHub.repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;
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
        private readonly FirebaseNotificationService firebaseNotificationService;
        private readonly ITokenBlacklistService blacklistService;
        private readonly IEmailSender _emailSender;


        public VehicleOwnerController(IEmailSender emailSender, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, GateHubContext context, IGenerateTokenService generateTokenService, IVehicleOwnerRepo vehicleOwnerRepo, PaymobService paymobService, IHubContext<NotificationHub> hubContext,FirebaseNotificationService firebaseNotificationService,ITokenBlacklistService blacklistService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.context = context;
            this.generateTokenService = generateTokenService;
            this.vehicleOwnerRepo = vehicleOwnerRepo;
            this.paymobService = paymobService;
            this.hubContext = hubContext;
            this.firebaseNotificationService = firebaseNotificationService;
            this.blacklistService = blacklistService;
            this._emailSender = emailSender;
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
                Balance = 0,
                AppUserId = user.Id,
                appUser = user
            };

            await vehicleOwnerRepo.AddVehicleOwner(vehicleOwner);
            if (!string.IsNullOrEmpty(dto.DeviceToken))
            {
                user.DeviceToken = dto.DeviceToken;
                await userManager.UpdateAsync(user);
            }

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


            var tokenString = await generateTokenService.GenerateJwtTokenAsync(user);
            if (!string.IsNullOrEmpty(dto.DeviceToken))
            {
                user.DeviceToken = dto.DeviceToken;
                await userManager.UpdateAsync(user);
            }

            return Ok(new { user, tokenString });

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

            var owner = await vehicleOwnerRepo.GetVehicleOwner(userId);
            
            if (owner == null)
                return NotFound("Vehicle owner not found.");

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
            var vehicleEntry = await vehicleOwnerRepo.CheckVehicleEntry(dto.VehicleEntryId, owner);

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

            //notify
            var user = await userManager.FindByIdAsync(userId);
            var deviceToken = user?.DeviceToken;

            await firebaseNotificationService.StoreNotification(userId, "Objection Submitted",
                "Your objection has been submitted and is under review");

            //if (!string.IsNullOrEmpty(deviceToken))
            //{
            await firebaseNotificationService.SendNotificationAsync(
                "Objection Submitted",
                "Your objection has been submitted and is under review",
                deviceToken
            );
            //}

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

            var vehicleEntry = await vehicleOwnerRepo.CheckVehicleEntry(vehicleEntryId, owner);

            if (vehicleEntry == null)
                return NotFound("Vehicle entry not found or does not belong to the owner.");

            if (vehicleEntry.IsPaid)
                return BadRequest("This entry has already been paid.");

            string paymentUrl = await paymobService.InitiatePayment(owner, vehicleEntry.FeeValue + (vehicleEntry.FineValue ?? 0), vehicleEntry.Id.ToString(), "Vehicle Entry Payment");

            return Ok(new { message = "Redirect to this URL for payment", paymentUrl });

        }

        [HttpPost("pay-multiple-vehicle-entries")]
        public async Task<IActionResult> PayMultipleVehicleEntries([FromBody] List<int> vehicleEntryIds)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return Unauthorized();

            var userId = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var owner = await vehicleOwnerRepo.GetVehicleOwner(userId);

            if (owner == null)
                return NotFound("Vehicle owner not found.");

            var vehicleEntries = await vehicleOwnerRepo.GetVehicleEntriesByIds(vehicleEntryIds);

            if (!vehicleEntries.Any())
                return NotFound("No valid vehicle entries found.");

            foreach (var vehicleEntry in vehicleEntries)
            {
                if (vehicleEntry.IsPaid)
                    return BadRequest("This entry has already been paid.");
            }

            // Calculate total amount (fees + fines)
            decimal totalAmount = vehicleEntries.Sum(ve => ve.FeeValue + (ve.FineValue ?? 0));

            // Convert list of IDs to comma-separated string
            string vehicleEntryIdsString = string.Join(",", vehicleEntryIds);

            // Initiate payment for multiple entries
            string paymentUrl = await paymobService.InitiatePayment(owner, totalAmount, vehicleEntryIdsString, "Multiple Vehicle Entries Payment");

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

            string paymentUrl = await paymobService.InitiatePayment(owner, dto.Amount, "Balance Recharge", "Balance Recharge");
            return Ok(new { message = "Redirect to this URL for payment", paymentUrl });
        }

        [HttpPost("pay-from-balance")]
        public async Task<IActionResult> PayFromBalance([FromBody] List<int> vehicleEntryIds)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return Unauthorized();

            var userId = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var owner = await vehicleOwnerRepo.GetVehicleOwner(userId);

            if (owner == null)
                return NotFound("Vehicle owner not found.");

            var vehicleEntries = await vehicleOwnerRepo.GetVehicleEntriesByIds(vehicleEntryIds);

            if (!vehicleEntries.Any())
                return NotFound("No valid vehicle entries found.");

            foreach (var vehicleEntry in vehicleEntries)
            {
                if (vehicleEntry.IsPaid)
                    return BadRequest("This entry has already been paid.");
            }

            // **Calculate total amount**
            decimal totalAmount = vehicleEntries.Sum(ve => ve.FeeValue + (ve.FineValue ?? 0));

            if (owner.Balance < totalAmount)
                return BadRequest("Insufficient balance to complete the payment.");

            // **Deduct balance & mark entries as paid**
            owner.Balance -= totalAmount;

            foreach (var vehicleEntry in vehicleEntries)
            {
                vehicleEntry.IsPaid = true;
            }

            // **Save changes**
            await vehicleOwnerRepo.SaveChangesAsync();

            // **Store transaction**
            var transaction = new Transaction
            {
                Amount = totalAmount,
                PaymentType = "Balance Payment",
                TransactionDate = DateTime.UtcNow,
                Status = "Success",
                VehicleOwnerId = owner.Id
            };

            await vehicleOwnerRepo.AddTransaction(transaction);

            // **Send real-time notification**
            await hubContext.Clients.User(owner.AppUserId)
                .SendAsync("ReceiveNotification", new
                {
                    Title = "Payment Successful",
                    Message = $"Your payment of {totalAmount} EGP has been deducted from your balance.",
                    Date = DateTime.UtcNow
                });

            return Ok(new { message = "Payment successful", remainingBalance = owner.Balance });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> PaymentWebhook([FromBody] PaymobWebhookDto data)
        {
            try
            {
                if (data?.Obj == null || data.Obj.Order == null || data.Obj.PaymentKeyClaims?.BillingData == null)
                {
                    return BadRequest("Invalid webhook request.");
                }

                bool status = data.Obj.Success;
                decimal amountPaid = data.Obj.AmountCents / 100m;
                string vehicleEntryIdsString = data.Obj.PaymentKeyClaims.BillingData.ExtraDescription;

                if (!status)
                {
                    return BadRequest("Payment not successful.");
                }


                // **Check if the payment is for a balance recharge**
                if (vehicleEntryIdsString == "Balance Recharge")
                {
                    string natId = data.Obj.PaymentKeyClaims.BillingData.Nat_Id;
                    var owner = await vehicleOwnerRepo.GetVehicleOwnerByNatId(natId);

                    if (owner == null)
                    {
                        return NotFound("Vehicle owner not found.");
                    }

                    owner.Balance += amountPaid;
                    await vehicleOwnerRepo.SaveChangesAsync();

                    var rechargeTransaction = new Transaction
                    {
                        Amount = amountPaid,
                        PaymentType = "Balance Recharge",
                        TransactionDate = DateTime.UtcNow,
                        Status = "Success",
                        VehicleOwnerId = owner.Id
                    };

                    await vehicleOwnerRepo.AddTransaction(rechargeTransaction);

                    // Send real-time notification
                    await hubContext.Clients.User(owner.AppUserId)
                        .SendAsync("ReceiveNotification", new
                        {
                            Title = "Balance Recharged",
                            Message = $"Your balance has been successfully recharged with {amountPaid} EGP.",
                            Date = DateTime.UtcNow
                        });

                    return Ok();
                }

                var vehicleEntryIds = vehicleEntryIdsString.Split(',').Select(int.Parse).ToList();


                var vehicleEntries = await vehicleOwnerRepo.GetVehicleEntriesByIds(vehicleEntryIds);

                if (!vehicleEntries.Any())
                {
                    return NotFound("No matching vehicle entries found.");
                }

                decimal totalAmount = 0;
                int vehicleOwnerId = 0;
                string appUserId = "";

                foreach (var vehicleEntry in vehicleEntries)
                {
                    vehicleEntry.IsPaid = true;
                    totalAmount += vehicleEntry.FeeValue + (vehicleEntry.FineValue ?? 0);
                    vehicleOwnerId = vehicleEntry.vehicle.VehicleOwnerId;
                    appUserId = vehicleEntry.vehicle.VehicleOwner.AppUserId;
                }


                // **Store the transaction**
                var transaction = new Transaction
                {
                    Amount = totalAmount,
                    PaymentType = "Vehicle Entry Payment",
                    TransactionDate = DateTime.UtcNow,
                    Status = "Success",
                    VehicleOwnerId = vehicleOwnerId
                };

                await vehicleOwnerRepo.AddTransaction(transaction);


                // **Send real-time notification via SignalR**
                await hubContext.Clients.User(appUserId)
                    .SendAsync("ReceiveNotification", new
                    {
                        Title = "Payment Successful",
                        Message = $"Your payment of {amountPaid} EGP has been received.",
                        Date = DateTime.UtcNow
                    });

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> changePassword([FromBody] ChangePasswordDto dto)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return Unauthorized();

            var userId = jwtToken.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await userManager.FindByIdAsync(userId);


            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("New password and confirmation do not match.");

            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Message = "Failed to change password", Errors = errors });
            }

            await signInManager.RefreshSignInAsync(user); // Optional for cookie-based login

            return Ok(new { Message = "Password changed successfully." });


        }

        [HttpGet("VehicleEntries")]
        public async Task<IActionResult> VehicleEntries(int vehicleId)
        {
            if (vehicleId != null || vehicleId != 0)
            {
                var entries = await vehicleOwnerRepo.VehicleEntries(vehicleId);
                if (entries == null)
                    return BadRequest("invalid vehicle ID");

                return Ok(entries);
            }
            return BadRequest("Un valid vehicle ID");
        }

        // OTPHandle

        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp([FromBody] ForgetPasswordRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var otp = new Random().Next(100000, 999999).ToString();
            user.ResetPasswordOTP = otp;
            user.ResetPasswordOTPExpiry = DateTime.UtcNow.AddMinutes(5);
            user.IsResetPasswordOTPUsed = false;
            await userManager.UpdateAsync(user);

            await _emailSender.SendEmailAsync(user.Email, "Password Reset OTP", $"Your OTP for password reset is:  <strong>{otp}</strong>. It is valid for 5 minutes.");

            return Ok("OTP has been sent to your email.");

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordOTPDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
                
            var user = await userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (user.ResetPasswordOTP != model.Otp)
            {
                return BadRequest("Invalid OTP.");
            }

            if (user.ResetPasswordOTPExpiry < DateTime.UtcNow)
            {
                return BadRequest("OTP has expired.");
            }


            if (user.IsResetPasswordOTPUsed == true)
            {
                return BadRequest("OTP has already been used.");
            }
            
            var token = await  userManager.GeneratePasswordResetTokenAsync(user);
            var result = await  userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Mark the OTP as used
            user.IsResetPasswordOTPUsed = true;
            await userManager.UpdateAsync(user);

            return Ok("Password has been reset successfully.");

        }
       
        
        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token)) return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            var userId = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            var notifications = await context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpGet("vehicle-entry/{id}")]
        public async Task<IActionResult> GetVehicleEntryById(int id)
        {
            var vehicleEntry = await vehicleOwnerRepo.FindVehicleEntry(id);

            if (vehicleEntry == null)
            {
                return NotFound($"Vehicle entry with ID {id} not found.");
            }

            return Ok(vehicleEntry);
        }


    }
}
