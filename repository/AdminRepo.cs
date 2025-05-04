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
        public async Task<VehicleWithOwnerDto> GetVehicleByPlateNumberWithOwner(string plateNumber)
        {
            var vehicle = await context.Vehicles
                .Include(v => v.VehicleOwner)
                .FirstOrDefaultAsync(v => v.PlateNumber.ToLower() == plateNumber.ToLower());

            if (vehicle == null) return null;

            return new VehicleWithOwnerDto
            {
                Id = vehicle.Id,
                PlateNumber = vehicle.PlateNumber,
                LicenseStart = vehicle.LicenseStart,
                LicenseEnd = vehicle.LicenseEnd,
                ModelDescription = vehicle.ModelDescription,
                ModelCompany = vehicle.ModelCompany,
                Color = vehicle.Color,
                Type = vehicle.Type,
                RFID = vehicle.RFID,
                VehicleOwner = new VehicleOwnerDto
                {
                    Id = vehicle.VehicleOwner.Id,
                    PhoneNumber = vehicle.VehicleOwner.PhoneNumber,
                    Address = vehicle.VehicleOwner.Address,
                    License = vehicle.VehicleOwner.License,
                    Balance = vehicle.VehicleOwner.Balance,
                    AppUserId = vehicle.VehicleOwner.AppUserId
                }
            };
        }
        public async Task<VehicleOwnerWithVehiclesDto> GetOwnerWithVehiclesByNatId(string natId)
        {
            var owner = await context.VehicleOwners
                .Include(vo => vo.Vehicles)
                .FirstOrDefaultAsync(vo => vo.appUser.NatId == natId);

            if (owner == null) return null;

            return new VehicleOwnerWithVehiclesDto
            {
                Id = owner.Id,
                PhoneNumber = owner.PhoneNumber,
                Address = owner.Address,
                License = owner.License,
                Balance = owner.Balance,
                AppUserId = owner.AppUserId,
                Vehicles = owner.Vehicles?.Select(v => new VehicleDto
                {
                    Id = v.Id,
                    PlateNumber = v.PlateNumber,
                    LicenseStart = v.LicenseStart,
                    LicenseEnd = v.LicenseEnd,
                    ModelDescription = v.ModelDescription,
                    ModelCompany = v.ModelCompany,
                    Color = v.Color,
                    Type = v.Type,
                    RFID = v.RFID
                }).ToList()
            };
        }
        public async Task<LostVehicle> GetLostVehicleByPlate(string plateNumber)
        {
            return await context.LostVehicles
                .Include(lv => lv.Vehicle)
                .ThenInclude(v => v.VehicleOwner)
                .FirstOrDefaultAsync(lv => lv.PlateNumber.ToLower() == plateNumber.ToLower() && !lv.IsFound);
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
        public async Task<Objection> AcceptObjection(int id)
        {
            var objection = await context.Objections.FindAsync(id);
            if (objection == null)
                return null;

            objection.Statue = "Accepted";
            await context.SaveChangesAsync();
            return objection;
        }

        public async Task<Objection> RejectObjection(int id)
        {
            var objection = await context.Objections.FindAsync(id);
            if (objection == null)
                return null;

            objection.Statue = "Rejected";

            var vehicleEntry = await context.VehicleEntries.FindAsync(objection.VehicleEntryId);
            if (vehicleEntry == null)
                return null;


            if (vehicleEntry.FineValue == 0)
            {
                vehicleEntry.FeeValue += vehicleEntry.FeeValue * 0.1m;
            }
            vehicleEntry.FineValue += vehicleEntry.FineValue * 0.1m;

            await context.SaveChangesAsync();

            return objection;
        }

        public async Task<List<LostVehicleAlertDto>> GetRecentLostVehicleAlerts()
        {
            var alerts = await (from entry in context.VehicleEntries
                                join lost in context.LostVehicles on entry.VehicleId equals lost.VehicleId
                                where !lost.IsFound
                                orderby entry.Date descending
                                select new LostVehicleAlertDto
                                {
                                    PlateNumber = entry.vehicle.PlateNumber,
                                    Gate = entry.gate.AddressName,
                                    DetectedTime = entry.Date
                                })
                               .Take(10)
                               .ToListAsync();

            return alerts;
        }

        public string GetTimeAgo(DateTime time)
        {
            var span = DateTime.Now - time;
            if (span.TotalMinutes < 1)
                return "Just now";
            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} min";
            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours} hr";
            return $"{(int)span.TotalDays} day";
        }

        public async Task<VehicleOwner> UpdateVehicleOwner(int OwnerId, [FromBody] VehicleOwnerUpdateDto dto)
        {
            var owner = await context.VehicleOwners.FindAsync(OwnerId);
            if (owner == null)
                return null;

            // Update fields
            owner.PhoneNumber = dto.PhoneNumber;
            owner.Address = dto.Address;
            owner.License = dto.License;

            await context.SaveChangesAsync();
            return owner;
        }

        public async Task<List<AllObjectionsDTO>> GetAllObjection()
        {
            List<AllObjectionsDTO> objections = new List<AllObjectionsDTO>();

            objections = await context.Objections.Include(o => o.vehicleOwner)
                .Include(ve => ve.vehicleEntry)
                   .ThenInclude(v => v.vehicle)
                .Select(o => new AllObjectionsDTO
                {
                    id = o.Id,
                    date = o.date,
                    Statue = o.Statue,
                    Description = o.Description,
                    VehicleOwnerName = o.vehicleOwner.appUser.Name,
                    PlateNum = o.vehicleEntry.vehicle.PlateNumber

                })
                .ToListAsync();
            return objections;
        }
        public async Task<ObjectionDetailsDTO> GetObjectionDetialsByID(int id)
        {
            var details = await context.Objections.Where(o => o.Id == id)
                .Select(o => new ObjectionDetailsDTO
                {
                    // Objection details
                    ObjectionId = o.Id,
                    ObjectionStatue = o.Statue,
                    ObjectionDate = o.date,
                    ObjectionDescription = o.Description,

                    // Entry details
                    EntrieFeeValue = o.vehicleEntry.FeeValue,
                    EntrieFineValue = o.vehicleEntry.FineValue,
                    EntrieFineType = o.vehicleEntry.FineType,
                    EntrieDate = o.vehicleEntry.Date,
                    EntrieIsPaid = o.vehicleEntry.IsPaid,
                    GateType = o.vehicleEntry.gate.Type,
                    GateAddressName = o.vehicleEntry.gate.AddressName,

                    // Vehicle owner details
                    VehivleOwnerPhoneNumb = o.vehicleOwner.PhoneNumber,
                    VehivleOwnerName = o.vehicleOwner.appUser.Name,

                    // Vehicle details
                    vehiclePlateNumber = o.vehicleEntry.vehicle.PlateNumber,
                    vehicleLicenseStart = o.vehicleEntry.vehicle.LicenseStart,
                    vehicleLicenseEnd = o.vehicleEntry.vehicle.LicenseEnd,
                    vehicleModelDescription = o.vehicleEntry.vehicle.ModelDescription,
                    vehicleModelCompany = o.vehicleEntry.vehicle.ModelCompany,
                    vehicleColor = o.vehicleEntry.vehicle.Color,
                    vehicleType = o.vehicleEntry.vehicle.Type
                })
                .FirstOrDefaultAsync();

      
            return details;

        }
        public async Task<Vehicle> updateVehicle( int vehicleId , UpdateVehicleDto dto)
        {
            var vehicleformDB = await GetVehicleById(vehicleId);

            if (vehicleformDB == null)
                return null;

            vehicleformDB.LicenseStart = dto.LicenseStart;
            vehicleformDB.LicenseEnd = dto.LicenseEnd;
            vehicleformDB.Color = dto.Color;
            vehicleformDB.RFID = dto.RFID;

             await context.SaveChangesAsync();
            return vehicleformDB;
        }
    }
}
