namespace GateHub.Models
{
    public class VehicleOwner
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NatId { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string License { get; set; }
        public decimal Balance { get; set; }
        public ICollection<Vehicle>? Vehicles { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        ICollection<Notification>? Notifications { get; set; }
        ICollection<Objection>? Objections { get; set;}

    }


}
