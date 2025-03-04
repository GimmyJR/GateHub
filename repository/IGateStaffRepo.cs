using GateHub.Dtos;
using GateHub.Models;

namespace GateHub.repository
{
    public interface IGateStaffRepo
    {
        public Task AddGateStaff(GateStaff gateStaff);
        public Task<Vehicle> FindVehicleByPlateNumber(string PlateNumber);
        public Task<GateFee> RetrieveTheFeeOfTheVehilceAndGate(int GateId, string VehicleType);
        public Task SendNotification(FineCreationDto dto, Vehicle vehicle);
        public Task AddFine(VehicleEntry fineEntry);
    }
}
