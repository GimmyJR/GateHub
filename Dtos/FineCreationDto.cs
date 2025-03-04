using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class FineCreationDto
    {
        [Required]
        public string PlateNumber { get; set; }

        [Required]
        public decimal FineValue { get; set; }

        public string FineType { get; set; }

        [Required]
        public int GateId { get; set; }
    }
}
