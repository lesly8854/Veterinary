using System;
using System.ComponentModel.DataAnnotations;

namespace Veterinary.Model
{
    public class MedicalRecordView
    {
        public int RecordID { get; set; }  // PK

        [Required]
        public int PetID { get; set; }  // FK

        [Required]
        public int VetID { get; set; }  // FK

        [Required]
        public int AppointmentID { get; set; }  // FK

        [StringLength(200)]
        public string Diagnosis { get; set; }

        [StringLength(200)]
        public string Treatment { get; set; }

        [StringLength(200)]
        public string Prescription { get; set; }

        [StringLength(200)]
        public string Vaccination { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateRecorded { get; set; }

        // UI helpers
        public string PetName { get; set; }
        public string VetName { get; set; }
    }
}
