using GateHub.Models;

namespace GateHub.repository
{
    public interface IVehicleOwnerRepo
    {
        public Task AddVehicleOwner(VehicleOwner vehicleOwner);
        public Task<VehicleOwner> VOProfile(string userId);
        public Task<VehicleOwner> GetVehicleOwner(string userId);
        public Task<List<VehicleEntry>> GetVehicleOwnerEntries(VehicleOwner owner);
        public Task<VehicleEntry> CheckVehicleEntry(int VehicleEntryId, VehicleOwner owner);
        public Task AddObjection(Objection objection);
        public Task<VehicleEntry> FindVehicleEntry(int vehicleEntryId);
        public Task<List<VehicleEntry>> GetVehicleEntriesByIds(List<int> vehicleEntryIds);
        public Task AddTransaction(Transaction transaction);
        public Task<VehicleOwner> GetVehicleOwnerByNatId(string natId);
        public Task SaveChangesAsync();
    }
}
