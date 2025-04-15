using GateHub.Dtos;
using GateHub.Models;

namespace GateHub.repository
{
    public interface IAdminRepo
    {
        public Task AddGate(GateCreateDto dto);
        public Task<Gate> GetGateById(int id);
        public Task AddVehicle(VehicleCreateDto dto);
        public Task<Vehicle> GetVehicleById(int id);
        public Task AddLostVehicle(LostVehicleCreationDto dto);
        public Task<Vehicle> FindVehicleByPlateNumber(string PlateNumber);
        public Task<List<LostVehicle>> GetAllLostVehicles();
        public Task RecoverVehicle(RecoverVehicleDTO dto);
        public  Task<DailyVehicleEntryCountDto> VehicleCount();
        public Task<MonthlyPaymentSummaryDto> TotalRevenue();
        public Task<int> GetLostVehicleCount();


    }
}
