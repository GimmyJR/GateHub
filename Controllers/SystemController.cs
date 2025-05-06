using GateHub.Dtos;
using GateHub.Models;
using GateHub.repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

            var result = await systemFeatures.ValidateVehicle(dto);

            return Ok(result);
        }

    }
}
