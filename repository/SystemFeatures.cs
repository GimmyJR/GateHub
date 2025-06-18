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
                .Include(v => v.VehicleOwner).ThenInclude(v => v.appUser)
                .FirstOrDefaultAsync(v => v.PlateNumber.ToLower() == PlateNumber.ToLower());

            return vehicle;
        }

        public async Task<VehicleValidationResult> ValidateVehicle(VehicleValidationDto dto)
        {
            const double SPEED_LIMIT = 120;

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.PlateNumber == dto.PlateNumber && v.RFID == dto.RFID);

            if (vehicle == null)
            {
                return (new VehicleValidationResult
                {
                    IsMatched = false,
                    IsLost = false,
                    IsSpeeding = false,
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
                    IsSpeeding = false,
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
            _context.VehicleEntries.Add(fineEntry);
            await _context.SaveChangesAsync();


            double? speed = await CalculateVehicleSpeed(vehicle.Id);
            bool isSpeeding = speed.HasValue && speed > SPEED_LIMIT;

            bool isLost = await VechicleIsLost(vehicle);

            bool isExpired = await VechicleLicenseIsExpired(vehicle);

            return (new VehicleValidationResult
            {
                IsMatched = true,
                IsLost = isLost,
                IsSpeeding = isSpeeding,
                IsLicenseExpired = isExpired
            });
        }

        private async Task<double?> CalculateVehicleSpeed(int vehicleId)
        {
            // Get the last two entries for this vehicle
            var lastTwoEntries = await _context.VehicleEntries
                .Where(ve => ve.VehicleId == vehicleId)
                .OrderByDescending(ve => ve.Date)
                .Take(2)
                .ToListAsync();

            if (lastTwoEntries.Count != 2)
            {
                return null;
            }

            var newerEntry = lastTwoEntries[0];
            var olderEntry = lastTwoEntries[1];

            var newerGate = await _context.Gates.FindAsync(newerEntry.GateId);
            var olderGate = await _context.Gates.FindAsync(olderEntry.GateId);

            if (newerGate == null || olderGate == null)
            {
                return null;
            }

            double distance = CalculateDistance(
                (double)olderGate.Latitude, (double)olderGate.Longitude,
                (double)newerGate.Latitude, (double)newerGate.Longitude);

            // Calculate time difference in hours
            TimeSpan timeDiff = newerEntry.Date - olderEntry.Date;
            double hours = timeDiff.TotalHours;

            if (hours <= 0)
            {
                return null; 
            }

            return distance / hours;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; 
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            lat1 = ToRadians(lat1);
            lat2 = ToRadians(lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }



    }
}
