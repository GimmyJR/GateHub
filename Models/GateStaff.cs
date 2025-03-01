using System.ComponentModel.DataAnnotations;

namespace GateHub.Models
{
    public class GateStaff
    {
        public int Id { get; set; }
        [Required, Phone]
        public string PhoneNumber { get; set; }
        public int GateId { get; set; }
        public Gate Gate { get; set; }
        public AppUser appUser { get; set; }
        public string AppUserId { get; set; }
    }


}
