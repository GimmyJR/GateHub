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
               .Where(v =>v.IsFound == false)
                .ToListAsync();

            return lostVehicles;
        }

        public async Task RecoverVehicle (RecoverVehicleDTO dto)
        {
            var lostVehicle = await context.LostVehicles.FirstOrDefaultAsync(v => v.PlateNumber == dto.PlateNum);
            if (lostVehicle == null)
            {
                return;
            }
            else
            {
                lostVehicle.IsFound = true;
            }

            await context.SaveChangesAsync();
        }
        public async Task<DailyVehicleEntryCountDto> VehicleCount ()
        {
            var today = DateTime.Today;

            var count = await context.VehicleEntries
                .CountAsync(ve => ve.Date.Date == today);

            return new DailyVehicleEntryCountDto
            {
                Date = today,
                Count = count
            };
        }
        public async Task<MonthlyPaymentSummaryDto> TotalRevenue()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);

            var entries = await context.VehicleEntries.Where(ve => ve.IsPaid == true &&
                                                        ve.Date >= startOfMonth &&
                                                        ve.Date < startOfNextMonth).ToListAsync();
            var summary = new MonthlyPaymentSummaryDto
            {
                TotalFees = entries.Sum(e => e.FeeValue),
                TotalFines = entries.Sum(e => e.FineValue ?? 0)
            };

            return summary;
        }
        public async Task<int> GetLostVehicleCount()
        {
            var lostVehicles = await context.LostVehicles
               .Where(v => v.IsFound == false).CountAsync();

            return lostVehicles;
        }
        public async Task<List<DailyReportDto>> GetDailyReport()
        {
            var today = DateTime.Today;

            var hourlyData = await context.VehicleEntries
                .Where(v => v.Date.Date == today)
                .GroupBy(v => v.Date.Hour)
                .Select(g => new DailyReportDto
                {
                    Hour = g.Key,
                    Cars = g.Count(),
                    Revenue = g.Where(x => x.IsPaid).Sum(x => x.FeeValue),
                })
                .ToListAsync();

            var lostVehicles = await context.LostVehicles
                .Where(lv => lv.ReportedDate.Date == today)
                .GroupBy(lv => lv.ReportedDate.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            foreach (var report in hourlyData)
            {
                var lost = lostVehicles.FirstOrDefault(l => l.Hour == report.Hour);
                report.LostVehicles = lost?.Count ?? 0;
            }

            return hourlyData;
        }

        public async Task<List<RecentCarDto>> GetRecentCars()
        {
            var recent = await context.VehicleEntries
               .OrderByDescending(v => v.Date)
               .Take(10)
               .Select(v => new RecentCarDto
               {
                   VehicleLicensePlate = v.vehicle.PlateNumber,
                   GateName = v.gate.AddressName,
                   DateOfPassing = v.Date,
               })
               .ToListAsync();

            return recent;
        }

        public async Task<List<GateTrafficDto>> GetTopGatesToday()
        {
            var today = DateTime.Today;

            var topGates = await context.VehicleEntries
                .Where(v => v.Date.Date == today)
                .GroupBy(v => v.gate.AddressName)
                .Select(g => new GateTrafficDto
                {
                    GateName = g.Key,
                    VehicleCount = g.Count()
                })
                .OrderByDescending(g => g.VehicleCount)
                .Take(5)
                .ToListAsync();

            return topGates;
        }


    }
}
