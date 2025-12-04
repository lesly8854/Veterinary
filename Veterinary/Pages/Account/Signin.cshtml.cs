using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Veterinary.Model;
using Veterinary_Business;   // for AppHelper
using System.Collections.Generic;

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

            // Use same connection string helper as Register
            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                // -----------------------------
                // 1) Try logging in as PetOwner
                // -----------------------------
                string ownerCmdText = @"
                    SELECT OwnerID, Username, Password
                    FROM PetOwner
                    WHERE Username = @username";

                using (SqlCommand ownerCmd = new SqlCommand(ownerCmdText, conn))
                {
                    ownerCmd.Parameters.AddWithValue("@username", LoginUser.Username);

                    using (SqlDataReader reader = ownerCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int ownerId = reader.GetInt32(0);
                            string dbUsername = reader.GetString(1);
                            string dbPasswordHash = reader.GetString(2);

                            if (!AppHelper.VerifyPassword(LoginUser.Password, dbPasswordHash))
                            {
                                ModelState.AddModelError("SignInError", "Invalid username or password.");
                                return Page();
                            }

                            // Build claims for OWNER
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, ownerId.ToString()),
                                new Claim(ClaimTypes.Name, dbUsername),
                                new Claim(ClaimTypes.Role, "Owner")
                            };

                            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var principal = new ClaimsPrincipal(identity);

                            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                            return RedirectToPage("/Account/Profile");
                        }
                    }
                }

                // -----------------------------------
                // 2) If not an Owner, try Vet account
                // -----------------------------------
                string vetCmdText = @"
                    SELECT VetID, AdminUsername, AdminPassword
                    FROM VeterinarianAdmin
                    WHERE AdminUsername = @username";

                using (SqlCommand vetCmd = new SqlCommand(vetCmdText, conn))
                {
                    vetCmd.Parameters.AddWithValue("@username", LoginUser.Username);

                    using (SqlDataReader vetReader = vetCmd.ExecuteReader())
                    {
                        if (vetReader.Read())
                        {
                            int vetId = vetReader.GetInt32(0);
                            string dbUsername = vetReader.GetString(1);
                            string dbPasswordHash = vetReader.GetString(2);

                            if (!AppHelper.VerifyPassword(LoginUser.Password, dbPasswordHash))
                            {
                                ModelState.AddModelError("SignInError", "Invalid username or password.");
                                return Page();
                            }

                            // Build claims for VET
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, vetId.ToString()),
                                new Claim(ClaimTypes.Name, dbUsername),
                                new Claim(ClaimTypes.Role, "Vet")
                            };

                            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var principal = new ClaimsPrincipal(identity);

                            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                            return RedirectToPage("/Account/Profile");
                        }
                    }
                }

                // Not found in either table
                ModelState.AddModelError("SignInError", "Invalid username or password.");
                return Page();
            }
        }
    }
}
