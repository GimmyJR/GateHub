using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class VehicleOwnerRegistrationDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string NatId { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string License { get; set; }

        public decimal Balance { get; set; } = 0;

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
