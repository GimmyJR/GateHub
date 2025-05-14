namespace GateHub.Models
{
    public class Gate
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string AddressName { get; set; }
        public string AddressCity { get; set; }
        public string AddressGovernment { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public ICollection<GateStaff> GateStaffs { get; set; }
        public ICollection<VehicleEntry> VehicleEntries { get; set; }
        public ICollection<GateFee> gateFees { get; set; }

    }


}
