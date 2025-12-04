using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Veterinary_Business;

namespace Veterinary.Pages.Account
{
    public class ProfileModel : PageModel
    {
        public UserProfileView Profile { get; set; }
        public string Role { get; set; }
        public string IdLabel { get; set; }

        public void OnGet()
        {
            Role = User.FindFirstValue(ClaimTypes.Role);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Profile = new UserProfileView();

            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                // -----------------------------
                // OWNER
                // -----------------------------
                if (Role == "Owner")
                {
                    IdLabel = "Owner ID";

                    string cmdText = @"SELECT OwnerID, Firstname, Lastname, PhoneNumber, Username
                                       FROM PetOwner
                                       WHERE OwnerID = @id";

                    using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Profile.Id = reader.GetInt32(0);
                                Profile.FirstName = reader.GetString(1);
                                Profile.LastName = reader.GetString(2);
                                Profile.PhoneNumber = reader.GetString(3);
                                Profile.Username = reader.GetString(4);
                            }
                        }
                    }
                }
                // -----------------------------
                // VET
                // -----------------------------
                else if (Role == "Vet")
                {
                    IdLabel = "Vet ID";

                    string cmdText = @"SELECT VetID, AdminName, AdminPhoneNumber, AdminUsername
                                       FROM VeterinarianAdmin
                                       WHERE VetID = @id";

                    using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Profile.Id = reader.GetInt32(0);

                                string fullName = reader.GetString(1);
                                // optional split
                                var parts = fullName.Split(' ');
                                Profile.FirstName = parts.Length > 0 ? parts[0] : "";
                                Profile.LastName = parts.Length > 1 ? parts[1] : "";

                                Profile.PhoneNumber = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                Profile.Username = reader.GetString(3);
                            }
                        }
                    }
                }
            }
        }

        public class UserProfileView
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PhoneNumber { get; set; }
            public string Username { get; set; }
        }
    }
}
