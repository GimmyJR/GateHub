using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GateHub.Models
{
    public class GateStaff
    {
        public int Id { get; set; }
        [Required, Phone]
        public string PhoneNumber { get; set; }
        public int GateId { get; set; }
        [JsonIgnore]
        public Gate Gate { get; set; }
        public AppUser appUser { get; set; }
        public string AppUserId { get; set; }
    }


}
