using System.ComponentModel.DataAnnotations;

namespace Veterinary.Model
{
    public class VeterinarianView
    {
        public int VetID { get; set; }  // PK

        [Required]
        [StringLength(60)]
        public string AdminName { get; set; }

        [Phone]
        [StringLength(20)]
        public string AdminPhoneNumber { get; set; }

        [Required]
        [StringLength(40)]
        public string AdminUsername { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100)]
        public string AdminPassword { get; set; }
    }
}
