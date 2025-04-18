namespace GateHub.Dtos
{
    public class LostVehicleAlertDto
    {
        public string PlateNumber { get; set; }
        public string Gate { get; set; }
        public DateTime DetectedTime { get; set; }
    }

}
