using GateHub.Models;

namespace GateHub.repository
{
    public interface ISystemFeatures
    {

        Task<bool> VechicleIsLost(Vehicle vehicle);
        Task<bool> VechicleLicenseIsExpired(Vehicle vehicle);


    }


}
