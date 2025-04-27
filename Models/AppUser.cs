using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GateHub.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        [Required, StringLength(14, MinimumLength = 14)]
        public string NatId { get; set; }
        public string Gender { get; set; }
        
        [Required, DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        public string? ResetPasswordOTP { get; set; }
        public DateTime? ResetPasswordOTPExpiry { get; set; }
        public bool? IsResetPasswordOTPUsed { get; set; }

    }
}
