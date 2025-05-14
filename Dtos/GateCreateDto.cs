using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class GateCreateDto
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public string AddressName { get; set; }

        [Required]
        public string AddressCity { get; set; }

        [Required]
        public string AddressGovernment { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
