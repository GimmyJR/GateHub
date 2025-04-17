namespace GateHub.Dtos
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; }
        public DateTime LicenseStart { get; set; }
        public DateTime LicenseEnd { get; set; }
        public string ModelDescription { get; set; }
        public string ModelCompany { get; set; }
        public string Color { get; set; }
        public string Type { get; set; }
        public string RFID { get; set; }
    }


}
