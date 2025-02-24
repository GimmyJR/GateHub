namespace GateHub.Models
{
    public class Objection
    {
        public int Id { get; set; }
        public string Statue { get; set; }
        public DateTime date { get; set; }
        public string Description { get; set; }
        public int VehicleOwnerId { get; set; }
        public VehicleOwner vehicleOwner { get; set; }
        public int VehicleEntryId { get; set;}
        public VehicleEntry vehicleEntry { get; set; }
    }


}
