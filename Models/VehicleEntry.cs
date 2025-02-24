namespace GateHub.Models
{
    public class VehicleEntry
    {
        public int Id { get; set; }
        public decimal FeeValue { get; set; }
        public decimal? FineValue { get; set; }
        public string? FineType { get; set; }
        DateTime Date { get; set; }
        public bool IsPaid { get; set; }

        public int VehicleId { get; set; }
        public Vehicle vehicle { get; set; }
        public int GateId { get; set; }
        public Gate gate{ get; set; }
    }


}
