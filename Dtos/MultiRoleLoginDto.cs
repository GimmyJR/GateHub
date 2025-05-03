using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class MultiRoleLoginDto
    {
        [Required]
        public string Role { get; set; }         //  "Admin" or "GateStaff"

        [Required, StringLength(14, MinimumLength = 14)]
        public string NatId { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
        public string? DeviceToken { get; set; }
    }
}
