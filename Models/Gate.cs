namespace GateHub.Models
{
    public class Gate
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string AddressName { get; set; }
        public string AddressCity { get; set; }
        public string AddressGovernment { get; set; }
        public ICollection<VehicleEntry> VehicleEntries { get; set; }

    }


}
