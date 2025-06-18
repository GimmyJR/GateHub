using GateHub.Dtos;
using GateHub.Hubs;
using GateHub.Models;
using GateHub.repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GateHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly ISystemFeatures systemFeatures;
        private readonly FirebaseNotificationService firebaseNotificationService;

        public SystemController(ISystemFeatures systemFeatures,FirebaseNotificationService firebaseNotificationService)
        {
            this.systemFeatures = systemFeatures;
            this.firebaseNotificationService = firebaseNotificationService;
        }


        [HttpPost("validate-vehicle")]
        public async Task<IActionResult> ValidateVehicle([FromBody] VehicleValidationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vehicle = await systemFeatures.FindVehicleByPlateNumber(dto.PlateNumber);

            var result = await systemFeatures.ValidateVehicle(dto);
            var owner = vehicle.VehicleOwner;
            var user = owner?.appUser;
            var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<VehicleHub>>();
            await hubContext.Clients.All.SendAsync("ReceiveVehicleUpdate",
                vehicle.PlateNumber,
                vehicle.ModelDescription,
                vehicle.VehicleOwner.appUser.Name,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                dto.GateId,
                "Active");

            if ( result.IsLost )
            {
                await hubContext.Clients.All.SendAsync("ReceiveAlert",
                "تم اكتشاف مركبة مفقودة",
                vehicle.PlateNumber,
                vehicle.ModelDescription,
                owner?.appUser?.Name ?? "غير معروف",
                currentTime);


                // Firebase notification to owner
                if (user != null)
                {
                    await firebaseNotificationService.StoreNotification(
                        user.Id,
                        "مركبة مفقودة",
                        $"تم اكتشاف مركبتك المسجلة كمفقودة ({vehicle.PlateNumber})");

                    if (!string.IsNullOrEmpty(user.DeviceToken))
                    {
                        await firebaseNotificationService.SendNotificationAsync(
                            "مركبة مفقودة",
                            $"تم اكتشاف مركبتك المسجلة كمفقودة ({vehicle.PlateNumber})",
                            user.DeviceToken);
                    }
                }

            }

            if (result.IsLicenseExpired)
            {
                // Real-time alert in Arabic
                await hubContext.Clients.All.SendAsync("ReceiveAlert",
                    "رخصة المركبة منتهية الصلاحية",
                    vehicle.PlateNumber,
                    vehicle.ModelDescription,
                    owner?.appUser?.Name ?? "غير معروف",
                    currentTime);

                // Firebase notification to owner
                if (user != null)
                {
                    await firebaseNotificationService.StoreNotification(
                        user.Id,
                        "رخصة منتهية",
                        $"رخصة مركبتك ({vehicle.PlateNumber}) منتهية الصلاحية. يرجى التجديد");

                    if (!string.IsNullOrEmpty(user.DeviceToken))
                    {
                        await firebaseNotificationService.SendNotificationAsync(
                            "رخصة منتهية",
                            $"رخصة مركبتك ({vehicle.PlateNumber}) منتهية الصلاحية. يرجى التجديد",
                            user.DeviceToken);
                    }
                }
            }

            if (!result.IsMatched)
            {
                // Real-time alert in Arabic
                await hubContext.Clients.All.SendAsync("ReceiveAlert",
                    "عدم تطابق بين RFID ولوحة المركبة",
                    vehicle.PlateNumber,
                    vehicle.ModelDescription,
                    owner?.appUser?.Name ?? "غير معروف",
                    currentTime);

                // Firebase notification to owner
                if (user != null)
                {
                    await firebaseNotificationService.StoreNotification(
                        user.Id,
                        "عدم تطابق",
                        $"عدم تطابق بين بطاقة RFID ولوحة مركبتك ({vehicle.PlateNumber})");

                    if (!string.IsNullOrEmpty(user.DeviceToken))
                    {
                        await firebaseNotificationService.SendNotificationAsync(
                            "عدم تطابق",
                            $"عدم تطابق بين بطاقة RFID ولوحة مركبتك ({vehicle.PlateNumber})",
                            user.DeviceToken);
                    }
                }
            }

            if(result.IsSpeeding)
            {
                // Real-time alert in Arabic
                await hubContext.Clients.All.SendAsync("ReceiveAlert",
                    "تجاوز السرعة المسموح بها",
                    vehicle.PlateNumber,
                    vehicle.ModelDescription,
                    owner?.appUser?.Name ?? "غير معروف",
                    currentTime);

                // Firebase notification to owner
                if (user != null)
                {
                    await firebaseNotificationService.StoreNotification(
                        user.Id,
                        "تجاوز السرعة",
                        $"تم رصد تجاوز سرعة مركبتك ({vehicle.PlateNumber})");

                    if (!string.IsNullOrEmpty(user.DeviceToken))
                    {
                        await firebaseNotificationService.SendNotificationAsync(
                            "تجاوز السرعة",
                            $"تم رصد تجاوز سرعة مركبتك ({vehicle.PlateNumber})",
                            user.DeviceToken);
                    }
                }
            }

            return Ok(result);
        }

    }
}
