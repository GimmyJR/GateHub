using System.ComponentModel.DataAnnotations;

namespace GateHub.Models
{
    public class Objection
    {
        public int Id { get; set; }
        public string Statue { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime date { get; set; }
        [Required, StringLength(500)]
        public string Description { get; set; }
        public int VehicleOwnerId { get; set; }
        public VehicleOwner vehicleOwner { get; set; }
        public int VehicleEntryId { get; set;}
        public VehicleEntry vehicleEntry { get; set; }
    }


}
