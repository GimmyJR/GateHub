using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GateHub.Models
{
    public class VehicleEntry
    {
        public int Id { get; set; }
        public decimal FeeValue { get; set; }
        public decimal? FineValue { get; set; }
        public string? FineType { get; set; }
        [Required, DataType(DataType.DateTime)]
        public DateTime Date { get; set; }
        public bool IsPaid { get; set; }
        public int VehicleId { get; set; }
        [JsonIgnore]
        public Vehicle vehicle { get; set; }
        public int GateId { get; set; }
        [JsonIgnore]
        public Gate gate{ get; set; }
    }


}
