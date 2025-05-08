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

        public SystemController(ISystemFeatures systemFeatures)
        {
            this.systemFeatures = systemFeatures;
        }


        [HttpPost("validate-vehicle")]
        public async Task<IActionResult> ValidateVehicle([FromBody] VehicleValidationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vehicle = await systemFeatures.FindVehicleByPlateNumber(dto.PlateNumber);

            var result = await systemFeatures.ValidateVehicle(dto);

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
                    "The Vehicle Is Flagged as Lost Vehicle",
                    vehicle.PlateNumber,
                vehicle.ModelDescription,
                vehicle.VehicleOwner.appUser.Name,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            if (result.IsLicenseExpired)
            {
                await hubContext.Clients.All.SendAsync("ReceiveAlert",
                    "The License of The Car Expired",
                    vehicle.PlateNumber,
                vehicle.ModelDescription,
                vehicle.VehicleOwner.appUser.Name,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            return Ok(result);
        }

    }
}
