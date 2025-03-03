using GateHub.Models;
using Microsoft.EntityFrameworkCore;

namespace GateHub.repository
{
    public class VehicleOwnerRepo : IVehicleOwnerRepo
    {
        private readonly GateHubContext context;

        public VehicleOwnerRepo(GateHubContext context)
        {
            this.context = context;
        }
        
        public async Task AddVehicleOwner(VehicleOwner vehicleOwner)
        {
            context.VehicleOwners.Add(vehicleOwner);
            await context.SaveChangesAsync();
        }

        public async Task<VehicleOwner> VOProfile(string userId)
        {
            var owner = await context.VehicleOwners
                .Include(vo => vo.Vehicles)
                .Include(vo => vo.Transactions)
                .FirstOrDefaultAsync(vo => vo.AppUserId == userId);

            return owner;
        }
    }
}
