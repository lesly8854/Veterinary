using System.ComponentModel.DataAnnotations;

namespace Veterinary.Model
{
    public class RegistrationView
    {
        [Required]
        [StringLength(40)]
        [Display(Name = "First Name:")]
        public string Firstname { get; set; }

        [Required]
        [StringLength(40)]
        [Display(Name = "Last Name:")]
        public string Lastname { get; set; }

        [Required]
        [StringLength(40)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        // "Owner" or "Vet" if you want a role flag
        [Required]
        [StringLength(20)]
        public string Role { get; set; }
    }
}
