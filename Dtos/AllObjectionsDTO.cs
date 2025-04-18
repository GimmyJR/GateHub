using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class AllObjectionsDTO
    {
        public string Statue { get; set; }
        public DateTime date { get; set; }
        public string Description { get; set; }
        public string VehicleOwnerName { get; set; }
        public string PlateNum { get; set; }


    }
}
