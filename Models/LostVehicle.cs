namespace GateHub.Models
{
    public class LostVehicle
    {
        public int Id { get; set; }
        public DateTime ReportedDate { get; set; }
        public bool IsFound { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

    }

}
