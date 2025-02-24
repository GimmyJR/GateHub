using System.ComponentModel.DataAnnotations;

namespace GateHub.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
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
        public ICollection<VehicleEntry> VehicleEntries { get; set; }
        public int VehicleOwnerId { get; set; }
        public VehicleOwner VehicleOwner { get; set; }
    }




}
