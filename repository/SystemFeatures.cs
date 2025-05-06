using GateHub.Dtos;
using GateHub.Models;
using Microsoft.EntityFrameworkCore;

namespace GateHub.repository
{
    public class SystemFeatures : ISystemFeatures
    {
        private readonly GateHubContext _context;
        public SystemFeatures(GateHubContext context)
        {
            _context = context;
        }

        public async Task<bool> VechicleIsLost(Vehicle vehicle)
        {
            return await _context.LostVehicles.AnyAsync(lv => lv.VehicleId == vehicle.Id && lv.IsFound == false);
        }
        public async Task<bool> VechicleLicenseIsExpired(Vehicle vehicle)
        {
            return await _context.Vehicles.AnyAsync(v => v.Id == vehicle.Id && v.LicenseEnd < DateTime.Now);
        }

        public async Task AddFine(VehicleEntry fineEntry)
        {
             _context.VehicleEntries.Add(fineEntry);
             await _context.SaveChangesAsync();
        }
        public async Task<GateFee> RetrieveTheFeeOfTheVehilceAndGate(int GateId, string VehicleType)
        {
            var gateFee = await _context.GateFees
                .FirstOrDefaultAsync(gf => gf.GateId == GateId
                    && gf.VehicleType.ToLower() == VehicleType.ToLower());

            return gateFee;
        }

        public async Task<Vehicle> FindVehicleByPlateNumber(string PlateNumber)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleOwner)
                .FirstOrDefaultAsync(v => v.PlateNumber.ToLower() == PlateNumber.ToLower());

            return vehicle;
        }

        public async Task<VehicleValidationResult> ValidateVehicle(VehicleValidationDto dto)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.PlateNumber == dto.PlateNumber && v.RFID == dto.RFID);

            if (vehicle == null)
            {
                return (new VehicleValidationResult
                {
                    IsMatched = false,
                    IsLost = false,
                    IsLicenseExpired = false
                });
            }

            var gateFee = await RetrieveTheFeeOfTheVehilceAndGate(dto.GateId, vehicle.Type);

            if (gateFee == null)
            {
                return (new VehicleValidationResult
                {
                    IsMatched = false,
                    IsLost = false,
                    IsLicenseExpired = false
                });
            }

            var fineEntry = new VehicleEntry
            {
                FeeValue = gateFee.Fee,
                FineValue = 0,
                FineType = "",
                Date = DateTime.Now,
                IsPaid = false,
                VehicleId = vehicle.Id,
                GateId = dto.GateId
            };
            await AddFine(fineEntry);


            bool isLost = await VechicleIsLost(vehicle);
            bool isExpired = await VechicleLicenseIsExpired(vehicle);

            return (new VehicleValidationResult
            {
                IsMatched = true,
                IsLost = isLost,
                IsLicenseExpired = isExpired
            });
        }



    }
}
