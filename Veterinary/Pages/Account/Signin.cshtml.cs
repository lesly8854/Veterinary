using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Veterinary.Model;
using Veterinary_Business;   // <-- for AppHelper

namespace Veterinary.Pages.Account
{
    public class SigninModel : PageModel
    {
        [BindProperty]
        public LoginView LoginUser { get; set; } = new LoginView();

        public void OnGet()
        {
            // just loads the page
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            using (SqlConnection conn = new SqlConnection(
                "Server=(localdb)\\MSSQLLocalDB;Database=Veterinary;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                string cmdText = @"SELECT OwnerID, Username, Password
                                   FROM PetOwner
                                   WHERE Username = @username";

                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@username", LoginUser.Username);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    // no such user
                    ModelState.AddModelError("SignInError", "Invalid username or password.");
                    return Page();
                }

                reader.Read();

                int ownerId = reader.GetInt32(0);
                string dbUsername = reader.GetString(1);
                string dbPasswordHash = reader.GetString(2);

                // ?? compare entered password with hashed password from DB
                if (!AppHelper.VerifyPassword(LoginUser.Password, dbPasswordHash))
                {
                    ModelState.AddModelError("SignInError", "Invalid username or password.");
                    return Page();
                }

                // build claims (logged-in user info)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, ownerId.ToString()),
                    new Claim(ClaimTypes.Name, dbUsername),
                    new Claim(ClaimTypes.Role, "Owner")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // sign in user with cookie auth
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // redirect after login
                return RedirectToPage("/Account/Profile");
            }
        }
    }
}
