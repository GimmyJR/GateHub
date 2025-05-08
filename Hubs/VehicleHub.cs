using Microsoft.AspNetCore.SignalR;

namespace GateHub.Hubs
{
    public class VehicleHub : Hub
    {
        public async Task SendVehicleUpdate(string licensePlate, string vehicleModel, string ownerName,
                                           string entryTime, string gateNumber, string status)
        {
            await Clients.All.SendAsync("ReceiveVehicleUpdate",
                licensePlate, vehicleModel, ownerName, entryTime, gateNumber, status);
        }

        public async Task SendAlert(string message, string licensePlate, string vehicleModel)
        {
            await Clients.All.SendAsync("ReceiveAlert", message, licensePlate, vehicleModel);
        }
    }
}
