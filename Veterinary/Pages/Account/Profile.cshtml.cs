using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
//using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Veterinary.Model;
using Veterinary_Business;

namespace Veterinary.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public PetOwnerView OwnerProfile { get; set; } = new PetOwnerView();

        public void OnGet()
        {
            // honestly just grabbing the logged-in username from claims
            string username = User.FindFirst(ClaimTypes.Name)?.Value;

            // loading the profile from DB
            LoadOwnerProfile(username);
        }

        private void LoadOwnerProfile(string? username)
        {
            if (username == null)
            {
                // lol something went wrong, just dip
                return;
            }

            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                // selecting owner info, matches columns I actually have
                string query = "SELECT OwnerID, FirstName, LastName, PhoneNumber, Username, Password " +
                               "FROM PetOwner WHERE Username = @username";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();

                    // mapping DB -> model properties
                    OwnerProfile.OwnerID = reader.GetInt32(0);
                    OwnerProfile.FirstName = reader.GetString(1);
                    OwnerProfile.LastName = reader.GetString(2);
                    OwnerProfile.PhoneNumber = reader.IsDBNull(3) ? null : reader.GetString(3);
                    OwnerProfile.Username = reader.GetString(4);
                    OwnerProfile.Password = reader.GetString(5);
                }
            }
        }
    }
}
