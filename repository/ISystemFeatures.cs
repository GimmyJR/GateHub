using GateHub.Dtos;
using GateHub.Models;
using Microsoft.EntityFrameworkCore;

namespace GateHub.repository
{
    public interface ISystemFeatures
    {

        Task<bool> VechicleIsLost(Vehicle vehicle);
        Task<bool> VechicleLicenseIsExpired(Vehicle vehicle);
        public  Task AddFine(VehicleEntry fineEntry);
        public  Task<GateFee> RetrieveTheFeeOfTheVehilceAndGate(int GateId, string VehicleType);
        public  Task<Vehicle> FindVehicleByPlateNumber(string PlateNumber);
        public Task<VehicleValidationResult> ValidateVehicle(VehicleValidationDto dto);
    }


}
