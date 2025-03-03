using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class LostVehicleCreationDto
    {
        [Required, DataType(DataType.Date)]
        public DateTime ReportedDate { get; set; }

        public bool IsFound { get; set; } = false;

        [Required]
        public string PlateNumber { get; set; }
    }
}
