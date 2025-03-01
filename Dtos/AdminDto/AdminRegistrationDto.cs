namespace GateHub.Dtos.AdminDto
{
    public class AdminRegistrationDto
    {
        public string FullName { get; set; }
        public string NatId { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string Password { get; set; }
    }
}
