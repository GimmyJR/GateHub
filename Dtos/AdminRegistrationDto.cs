using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class AdminRegistrationDto
    {
        public string FullName { get; set; }
        [Required, StringLength(14, MinimumLength = 14)]
        public string NatId { get; set; }
        [Required, Phone]
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string Password { get; set; }
    }

}
