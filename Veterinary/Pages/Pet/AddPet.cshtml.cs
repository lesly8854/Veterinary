using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Veterinary.Model;
using Veterinary_Business;
using System;
using System.Security.Claims;

namespace Veterinary.Pages.Pet
{
    public class AddPetModel : PageModel
    {
        [BindProperty]
        public PetView NewPet { get; set; } = new PetView();

        public void OnGet()
        {
            // Get OwnerID from logged-in user (claim)
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
            {
                // not logged in -> you could redirect, but for now just leave it blank
                return;
            }

            NewPet.OwnerID = int.Parse(ownerIdClaim.Value);

            // Look up the owner's FIRST + LAST name from PetOwner table
            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                string cmdText = @"SELECT Firstname, Lastname
                                   FROM PetOwner
                                   WHERE OwnerID = @OwnerID";

                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@OwnerID", NewPet.OwnerID);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    string first = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    string last = reader.IsDBNull(1) ? "" : reader.GetString(1);

                    // Show "Firstname Lastname" in the OwnerName field
                    NewPet.OwnerName = (first + " " + last).Trim();
                }
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // Always use OwnerID from claims, not from the posted form
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
            {
                return RedirectToPage("/Account/Signin");
            }

            int ownerId = int.Parse(ownerIdClaim.Value);

            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                string cmdText = @"INSERT INTO Pet
                                   (OwnerID, Name, Species, Breed, Gender, DateOfBirth, Weight)
                                   VALUES (@OwnerID, @Name, @Species, @Breed, @Gender, @DateOfBirth, @Weight)";

                SqlCommand cmd = new SqlCommand(cmdText, conn);

                cmd.Parameters.AddWithValue("@OwnerID", ownerId);
                cmd.Parameters.AddWithValue("@Name", NewPet.Name);
                cmd.Parameters.AddWithValue("@Species", (object?)NewPet.Species ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Breed", (object?)NewPet.Breed ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", (object?)NewPet.Gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateOfBirth",
                    NewPet.DateOfBirth.HasValue ? NewPet.DateOfBirth.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Weight",
                    NewPet.Weight.HasValue ? NewPet.Weight.Value : (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToPage("/Pet/PetList");
        }
    }
}
