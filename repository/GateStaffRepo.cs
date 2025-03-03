using GateHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GateHub.repository
{
    public class GateStaffRepo:IGateStaffRepo
    {
        private readonly GateHubContext context;

        public GateStaffRepo(GateHubContext context)
        {
            this.context = context;
        }

        public async Task AddGateStaff(GateStaff gateStaff)
        {
            context.GateStaff.Add(gateStaff);
            await context.SaveChangesAsync();
        }
    }
}
