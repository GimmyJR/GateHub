using GateHub.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

        public async Task<VehicleOwner> GetVehicleOwner(string userId)
        {
            var owner = await context.VehicleOwners
                .Include(vo => vo.Vehicles)
                .Include(ve => ve.appUser)
                .FirstOrDefaultAsync(vo => vo.AppUserId == userId);

            return owner;
        }

        public async Task<List<VehicleEntry>> GetVehicleOwnerEntries(VehicleOwner owner)
        {
            if (owner == null || owner.Vehicles == null || !owner.Vehicles.Any())
                return new List<VehicleEntry>(); 

            var vehicleIds = owner.Vehicles.Select(v => v.Id).ToList();

            var vehicleEntries = await context.VehicleEntries
                .Include(ve => ve.vehicle)
                .Include(ve => ve.gate)
                .Where(ve => vehicleIds.Contains(ve.VehicleId)) 
                .Where(ve => ve.IsPaid == false)
                .ToListAsync();

            return vehicleEntries;
        }

        public async Task<VehicleEntry> CheckVehicleEntry(int VehicleEntryId,VehicleOwner owner)
        {
            var vehicleEntry = await context.VehicleEntries
                .Include(ve => ve.vehicle)
                .FirstOrDefaultAsync(ve => ve.Id == VehicleEntryId && ve.vehicle.VehicleOwnerId == owner.Id);

            return vehicleEntry;
        }

        public async Task AddObjection(Objection objection)
        {
            context.Objections.Add(objection);
            await context.SaveChangesAsync();
        }

        public async Task<VehicleEntry> FindVehicleEntry(int vehicleEntryId)
        {
            var vehicleEntry = await context.VehicleEntries
                .Include(ve => ve.vehicle)  // Include Vehicle
                .ThenInclude(v => v.VehicleOwner)  // Include VehicleOwner inside Vehicle
                .FirstOrDefaultAsync(ve => ve.Id == vehicleEntryId);  // Find by ID

            return vehicleEntry;
        }

        public async Task<List<VehicleEntry>> GetVehicleEntriesByIds(List<int> vehicleEntryIds)
        {
            return await context.VehicleEntries
                .Where(ve => vehicleEntryIds.Contains(ve.Id))
                .Include(ve => ve.vehicle)
                .ThenInclude(v => v.VehicleOwner)
                .ToListAsync();
        }

        public async Task AddTransaction(Transaction transaction)
        {
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();
        }

        public async Task<VehicleOwner> GetVehicleOwnerByNatId(string natId)
        {
            var owner = await context.VehicleOwners
                .FirstOrDefaultAsync(vo => vo.appUser.NatId == natId);

            return owner;
        }
       
        public async Task<List<VehicleEntry>> VehicleEntries(int vehicleId)
        {
            List<VehicleEntry> entries = new List<VehicleEntry>();

            entries = await context.VehicleEntries.Where(vi => vi.VehicleId == vehicleId).ToListAsync();
            return entries;
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
