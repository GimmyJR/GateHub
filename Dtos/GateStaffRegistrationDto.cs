using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class GateStaffRegistrationDto
    {
        public string FullName { get; set; }

        [Required, StringLength(14, MinimumLength = 14)]
        public string NatId { get; set; }

        public DateTime BirthDate { get; set; }

        public string Gender { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }

        public int GateId { get; set; }

        public string Password { get; set; }
        public string? DeviceToken { get; set; }
    }
}
