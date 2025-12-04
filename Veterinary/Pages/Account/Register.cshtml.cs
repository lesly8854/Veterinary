using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Veterinary.Model;
using Veterinary_Business;
using System;

namespace Veterinary.Pages.Account
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public RegistrationView NewUser { get; set; } = new RegistrationView();

        public void OnGet()
        {
            // page loads, no setup needed right now
        }

        public IActionResult OnPost()
        {
            // if the form is bad, just reload it with validation messages
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // optional extra safety
            if (string.IsNullOrWhiteSpace(NewUser.Password))
            {
                ModelState.AddModelError("NewUser.Password", "Password cannot be empty.");
                return Page();
            }

            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                string cmdText = "INSERT INTO PetOwner (Firstname, Lastname, Username, Password, PhoneNumber) " +
                                 "VALUES (@Firstname, @Lastname, @Username, @Password, @PhoneNumber)";

                SqlCommand cmd = new SqlCommand(cmdText, conn);

                cmd.Parameters.AddWithValue("@Firstname", NewUser.Firstname);
                cmd.Parameters.AddWithValue("@Lastname", NewUser.Lastname);

                cmd.Parameters.AddWithValue("@Username", NewUser.Username);
                cmd.Parameters.AddWithValue("@Password", AppHelper.GeneratePasswordHash(NewUser.Password));
                cmd.Parameters.AddWithValue("@PhoneNumber", "N/A"); // adjust later if you add this to the form

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToPage("/Account/Signin");
        }
    }
}
