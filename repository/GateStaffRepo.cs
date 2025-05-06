using GateHub.Dtos;
using GateHub.Hubs;
using GateHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GateHub.repository
{
    public class GateStaffRepo:IGateStaffRepo
    {
        private readonly GateHubContext context;
        private readonly IHubContext<NotificationHub> hubContext;

        public GateStaffRepo(GateHubContext context,IHubContext<NotificationHub> hubContext)
        {
            this.context = context;
            this.hubContext = hubContext;
        }

        public async Task AddGateStaff(GateStaff gateStaff)
        {
            context.GateStaff.Add(gateStaff);
            await context.SaveChangesAsync();
        }
        public async Task<Vehicle> FindVehicleByPlateNumber(string PlateNumber)
        {
            var vehicle = await context.Vehicles
                .Include(v => v.VehicleOwner)
                .FirstOrDefaultAsync(v => v.PlateNumber.ToLower() == PlateNumber.ToLower());

            return vehicle;
        }
        public async Task<GateFee> RetrieveTheFeeOfTheVehilceAndGate(int GateId,string VehicleType)
        {
            var gateFee = await context.GateFees
                .FirstOrDefaultAsync(gf => gf.GateId == GateId
                    && gf.VehicleType.ToLower() == VehicleType.ToLower());

            return gateFee;
        }
        public async Task AddFine(VehicleEntry fineEntry)
        {
            context.VehicleEntries.Add(fineEntry);
            await context.SaveChangesAsync();
        }
    }
}
