using System.ComponentModel.DataAnnotations;

namespace GateHub.Models
{
    public class GateStaff
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }
        [Required, StringLength(14, MinimumLength = 14)]
        public string NatId { get; set; }
        [Required, Phone]
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        public int GateId { get; set; }
        public Gate Gate { get; set; }
        public AppUser appUser { get; set; }
        public string AppUserId { get; set; }
    }


}
