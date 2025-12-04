using System;
using System.ComponentModel.DataAnnotations;

namespace Veterinary.Model
{
    public class BillingView
    {
        public int BillingID { get; set; }  // PK

        [Required]
        public int AppointmentID { get; set; }  // FK

        [Required]
        public int OwnerID { get; set; }  // FK

        [StringLength(20)]
        public string PaymentStatus { get; set; }  // "Paid", "Unpaid", etc.

        [DataType(DataType.Date)]
        public DateTime DateIssued { get; set; }

        // Optional UI helpers
        public string OwnerName { get; set; }
        public string PetName { get; set; }
    }
}
