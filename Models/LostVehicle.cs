using System.ComponentModel.DataAnnotations;

namespace GateHub.Models
{
    public class LostVehicle
    {
        public int Id { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime ReportedDate { get; set; }
        public bool IsFound { get; set; }
        public string PlateNumber { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

    }

}
