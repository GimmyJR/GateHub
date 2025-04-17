namespace GateHub.Dtos
{
    public class VehicleOwnerWithVehiclesDto
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string License { get; set; }
        public decimal Balance { get; set; }
        public string AppUserId { get; set; }
        public List<VehicleDto> Vehicles { get; set; }
    }


}
