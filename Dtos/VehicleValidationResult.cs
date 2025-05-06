namespace GateHub.Dtos
{
    public class VehicleValidationResult
    {
        public bool IsMatched { get; set; }
        public bool IsLost { get; set; }
        public bool IsLicenseExpired { get; set; }
    }

}
