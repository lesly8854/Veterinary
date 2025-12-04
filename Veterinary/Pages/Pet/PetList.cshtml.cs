using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Veterinary.Model;
using Veterinary_Business;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Veterinary.Pages.Pet
{
    [Authorize(Roles = "Owner")]
    public class PetListModel : PageModel
    {
        public List<PetView> Pets { get; set; } = new List<PetView>();

        public void OnGet()
        {
            // Get the current logged-in owner's ID from claims
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (ownerIdClaim == null)
            {
                // Not logged in – you can redirect if you want, but for now just show empty list
                Pets = new List<PetView>();
                return;
            }

            int ownerId = int.Parse(ownerIdClaim.Value);

            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                string cmdText = @"SELECT PetID, OwnerID, Name, Species, Breed, Gender, DateOfBirth, Weight 
                           FROM Pet
                           WHERE OwnerID = @OwnerID";      // ?? filter by current owner

                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@OwnerID", ownerId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Pets.Add(new PetView
                    {
                        PetID = reader.GetInt32(0),
                        OwnerID = reader.GetInt32(1),
                        Name = reader.GetString(2),
                        Species = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Breed = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Gender = reader.IsDBNull(5) ? null : reader.GetString(5),
                        DateOfBirth = reader.IsDBNull(6) ? (System.DateTime?)null : reader.GetDateTime(6),
                        Weight = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7)
                    });
                }
            }
        }

    }
}
