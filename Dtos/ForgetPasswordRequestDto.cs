using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class ForgetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }   
    }
}
