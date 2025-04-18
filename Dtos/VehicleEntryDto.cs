namespace GateHub.Dtos
{
    public class VehicleEntryDto
    {
        public int Id { get; set; }
        public decimal FeeValue { get; set; }
        public decimal FineValue { get; set; }
        public string FineType { get; set; }
        public DateTime Date { get; set; }
        public bool IsPaid { get; set; }
        public int VehicleId { get; set; }
        public int GateId { get; set; }
        public string GateName { get; set; } 
    }

}
