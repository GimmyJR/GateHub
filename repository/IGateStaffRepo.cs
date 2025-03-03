using GateHub.Models;

namespace GateHub.repository
{
    public interface IGateStaffRepo
    {
        public Task AddGateStaff(GateStaff gateStaff);
    }
}
