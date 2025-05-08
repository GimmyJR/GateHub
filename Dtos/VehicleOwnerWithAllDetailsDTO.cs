using GateHub.Models;
using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class VehicleOwnerWithAllDetailsDTO
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string License { get; set; }
        public decimal Balance { get; set; }
        public  string UserName { get; set; }
        public string FullName { get; set; }
        public string NatId { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public ICollection<Vehicle>? Vehicles { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
  
    }
}
