using System.ComponentModel.DataAnnotations;

namespace GateHub.Models
{
    public class VehicleOwner
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Required, StringLength(14, MinimumLength = 14)]
        public string NatId { get; set; }
        [Required, Phone]
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }
        public string License { get; set; }
        public decimal Balance { get; set; }
        public ICollection<Vehicle>? Vehicles { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        ICollection<Notification>? Notifications { get; set; }
        ICollection<Objection>? Objections { get; set;}
        public AppUser appUser { get; set; }
        public string AppUserId { get; set; } 
    }


}
