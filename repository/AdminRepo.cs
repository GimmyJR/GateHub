using GateHub.Dtos;
using GateHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Unicode;

namespace GateHub.repository
{
    public class AdminRepo : IAdminRepo
    {
        private readonly GateHubContext context;

        public AdminRepo(GateHubContext context)
        {
            this.context = context;
        }

        public async Task AddGate(GateCreateDto dto)
        {
            var gate = new Gate
            {
                Type = dto.Type,
                AddressName = dto.AddressName,
                AddressCity = dto.AddressCity,
                AddressGovernment = dto.AddressGovernment,
                GateStaffs = new List<GateStaff>(),
                VehicleEntries = new List<VehicleEntry>()
            };

            context.Gates.Add(gate);
            await context.SaveChangesAsync();
        }
        public async Task<Gate> GetGateById(int id)
        {
            var gate = await context.Gates
                .Include(G => G.GateStaffs)
                .Include(G => G.VehicleEntries)
                .Include(G => G.gateFees)
                .FirstOrDefaultAsync(G => G.Id == id);

            return gate;
        }
        public async Task AddVehicle(VehicleCreateDto dto)
        {
            var vehicle = new Vehicle
            {
                PlateNumber = dto.PlateNumber,
                LicenseStart = dto.LicenseStart,
                LicenseEnd = dto.LicenseEnd,
                ModelDescription = dto.ModelDescription,
                ModelCompany = dto.ModelCompany,
                Color = dto.Color,
                Type = dto.Type,
                RFID = dto.RFID,
                VehicleOwnerId = dto.VehicleOwnerId,
            };


            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
        }
        public async Task<Vehicle> GetVehicleById(int id)
        {
            var vehicle = await context.Vehicles.Include(v => v.VehicleEntries)
                .FirstOrDefaultAsync(v => v.Id == id);

            return vehicle;
        }
        public async Task<Vehicle> FindVehicleByPlateNumber(string PlateNumber)
        {
            var vehicle = await context.Vehicles
                .FirstOrDefaultAsync(v => v.PlateNumber.ToLower() == PlateNumber.ToLower());

            return vehicle;
        }
        public async Task AddLostVehicle(LostVehicleCreationDto dto)
        {
            // Find the vehicle by plate number (case-insensitive)
            var vehicle = await FindVehicleByPlateNumber(dto.PlateNumber);

            // Create the LostVehicle record
            var lostVehicle = new LostVehicle
            {
                ReportedDate = dto.ReportedDate,
                IsFound = dto.IsFound,
                PlateNumber = dto.PlateNumber,
                VehicleId = vehicle.Id,
                Vehicle = vehicle
            };

            context.LostVehicles.Add(lostVehicle);
            await context.SaveChangesAsync();
        }
        public async Task<List<LostVehicle>> GetAllLostVehicles()
        {
            var lostVehicles = await context.LostVehicles
                .Include(lv => lv.Vehicle)
                .ToListAsync();

            return lostVehicles;
        }
    }
}
