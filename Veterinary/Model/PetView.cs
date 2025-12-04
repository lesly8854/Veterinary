using System;
using System.ComponentModel.DataAnnotations;

namespace Veterinary.Model
{
    public class PetView
    {
        public int PetID { get; set; }  // PK

        [Required]
        public int OwnerID { get; set; }  // FK to PetOwner

        // UI helper
        [Display(Name = "Owner")]
        public string OwnerName { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(30)]
        public string Species { get; set; }

        [StringLength(30)]
        public string Breed { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }  // "Male", "Female", etc.

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public decimal? Weight { get; set; }
    }
}
