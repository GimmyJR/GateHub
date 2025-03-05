using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class ObjectionDto
    {
        [Required]
        public int VehicleEntryId { get; set; }  

        [Required]
        [StringLength(500, ErrorMessage = "Description must be at most 500 characters.")]
        public string Description { get; set; }
    }
}
