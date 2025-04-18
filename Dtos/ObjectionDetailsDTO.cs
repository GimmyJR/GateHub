using System.ComponentModel.DataAnnotations;

namespace GateHub.Dtos
{
    public class ObjectionDetailsDTO
    {
        //objection details
        public int ObjectionId { get; set; }
        public string ObjectionStatue { get; set; }
        
        public DateTime ObjectionDate { get; set; }
        
        public string ObjectionDescription { get; set; }

        //---------------------------- entrie details
        public decimal EntrieFeeValue { get; set; }
        public decimal? EntrieFineValue { get; set; }
        public string? EntrieFineType { get; set; }
        
        public DateTime EntrieDate { get; set; }
        public bool EntrieIsPaid { get; set; }
        public string GateType { get; set; } // gate
        public string GateAddressName { get; set; }  // gate

        //----------------------------------  VehivleOwner details
        public string VehivleOwnerPhoneNumb { get; set; }
        public string VehivleOwnerName { get; set; }

        // ------------------------------------ vehicle details
        public string vehiclePlateNumber { get; set; }
        public DateTime vehicleLicenseStart { get; set; }
        public DateTime vehicleLicenseEnd { get; set; }
        public string vehicleModelDescription { get; set; }
        public string vehicleModelCompany { get; set; }
        public string vehicleColor { get; set; }
        public string vehicleType { get; set; }



    }
}
