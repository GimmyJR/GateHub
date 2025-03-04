using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GateHub.Models
{
    public class GateFee
    {
        // Composite key: GateId and VehicleType
        [Key, Column(Order = 0)]
        public int GateId { get; set; }

        [Key, Column(Order = 1)]
        [Required]
        public string VehicleType { get; set; }

        [Required]
        public decimal Fee { get; set; }

        // Navigation property: many-to-one relationship with Gate
        [JsonIgnore]
        public Gate Gate { get; set; }
    }
}
