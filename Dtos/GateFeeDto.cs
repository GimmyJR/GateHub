using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class GateFeeDto
    {
        [Required]
        public int GateId { get; set; }

        [Required]
        public string VehicleType { get; set; }

        [Required]
        public decimal Fee { get; set; }
    }
}
