using System.ComponentModel.DataAnnotations;

namespace Veterinary.Model
{
    public class LoginView
    {
        [Required]
        [StringLength(40)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
