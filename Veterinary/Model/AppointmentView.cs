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

        // now we REQUIRE choosing a vet
        [Required]
        public int VetID { get; set; }   // FK to VeterinarianAdmin

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(100)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Scheduled";

        // UI helpers – optional
        public string? PetName { get; set; }
        public string? OwnerName { get; set; }
        public string? VetName { get; set; }
    }
}
