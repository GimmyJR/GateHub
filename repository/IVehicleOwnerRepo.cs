using GateHub.Models;

namespace GateHub.repository
{
    public interface IVehicleOwnerRepo
    {
        public Task AddVehicleOwner(VehicleOwner vehicleOwner);
        public Task<VehicleOwner> VOProfile(string userId);
    }
}
