using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class UpdateVehicleDto
    {
        [Required]
        public DateTime LicenseStart { get; set; }

        [Required]
        public DateTime LicenseEnd { get; set; }

        public string Color { get; set; }

        public string RFID { get; set; }
    }
}
