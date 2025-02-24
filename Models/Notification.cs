namespace GateHub.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Statue { get; set; }
        public string Description { get; set; }
        public int VehicleOwnerId { get; set; }
        public VehicleOwner vehicleOwner { get; set; }  
    }


}
