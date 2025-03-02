using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class VehicleOwnerLoginDto
    {
        [Required]
        public string NatId { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
