using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GateHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetVehicles()
        {
            List<string> architectureList = new List<string>() { "2", "9" };
            return Ok(architectureList);
        }
    }
}
