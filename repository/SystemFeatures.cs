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

 
    }
}
