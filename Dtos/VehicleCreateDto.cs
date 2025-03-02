using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class VehicleCreateDto
    {
        [Required]
        public string PlateNumber { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime LicenseStart { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime LicenseEnd { get; set; }

        public string ModelDescription { get; set; }

        public string ModelCompany { get; set; }

        public string Color { get; set; }

        public string Type { get; set; }

        public string RFID { get; set; }

        [Required]
        public int VehicleOwnerId { get; set; }
    }
}
