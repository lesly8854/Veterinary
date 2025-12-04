using System.ComponentModel.DataAnnotations;

namespace Veterinary.Model
{
    public class PetOwnerView
    {
        public int OwnerID { get; set; }  // PK

        [Required]
        [StringLength(40)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(40)]
        public string LastName { get; set; }

        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(40)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100)]
        public string Password { get; set; }

        // UI helper
        public string FullName => $"{FirstName} {LastName}";
    }
}
