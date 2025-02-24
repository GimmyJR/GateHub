using System.ComponentModel.DataAnnotations;

namespace GateHub.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }
        [Required, DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }

        public int VehicleOwnerId { get; set; }
        public VehicleOwner VehicleOwner { get; set; }
    }




}
