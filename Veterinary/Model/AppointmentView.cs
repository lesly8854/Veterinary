using System;
using System.ComponentModel.DataAnnotations;

namespace Veterinary.Model
{
    public class AppointmentView
    {
        public int AppointmentID { get; set; }  // PK

        [Required]
        public int PetID { get; set; }  // FK

        [Required]
        public int OwnerID { get; set; }  // FK

        public int? VetID { get; set; }  // FK

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(100)]
        public string Reason { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }  // e.g. "Scheduled", "Completed"

        // UI helpers
        public string? PetName { get; set; }
        public string? OwnerName { get; set; }
        public string? VetName { get; set; }

    }
}
