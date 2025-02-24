namespace GateHub.Models
{
    public class GateStaff
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NatId { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }

        public int GateId { get; set; }
        public Gate Gate { get; set; }
    }


}
