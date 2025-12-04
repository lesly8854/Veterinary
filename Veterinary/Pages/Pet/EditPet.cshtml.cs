using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Veterinary.Model;
using Veterinary_Business;
using System;

namespace Veterinary.Pages.Pet
{
    public class EditPetModel : PageModel
    {
        [BindProperty]
        public PetView EditPet { get; set; } = new PetView();

        public IActionResult OnGet(int id)
        {
            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                // 1) Load pet info
                string cmdText = @"SELECT PetID, OwnerID, Name, Species, Breed, Gender, DateOfBirth, Weight 
                                   FROM Pet 
                                   WHERE PetID = @PetID";
                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@PetID", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    return RedirectToPage("/Pet/PetList");
                }

                reader.Read();

                EditPet.PetID = reader.GetInt32(0);
                EditPet.OwnerID = reader.GetInt32(1);
                EditPet.Name = reader.GetString(2);
                EditPet.Species = reader.IsDBNull(3) ? null : reader.GetString(3);
                EditPet.Breed = reader.IsDBNull(4) ? null : reader.GetString(4);
                EditPet.Gender = reader.IsDBNull(5) ? null : reader.GetString(5);
                EditPet.DateOfBirth = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6);
                EditPet.Weight = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7);

                reader.Close();

                // 2) Load owner name for display (optional nice touch)
                string ownerCmdText = @"SELECT Firstname, Lastname 
                                        FROM PetOwner 
                                        WHERE OwnerID = @OwnerID";
                SqlCommand ownerCmd = new SqlCommand(ownerCmdText, conn);
                ownerCmd.Parameters.AddWithValue("@OwnerID", EditPet.OwnerID);

                SqlDataReader ownerReader = ownerCmd.ExecuteReader();
                if (ownerReader.HasRows)
                {
                    ownerReader.Read();
                    string first = ownerReader.IsDBNull(0) ? "" : ownerReader.GetString(0);
                    string last = ownerReader.IsDBNull(1) ? "" : ownerReader.GetString(1);
                    EditPet.OwnerName = (first + " " + last).Trim();
                }
                ownerReader.Close();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                // ?? Notice: OwnerID is NOT updated here
                string cmdText = @"UPDATE Pet
                                   SET Name = @Name,
                                       Species = @Species,
                                       Breed = @Breed,
                                       Gender = @Gender,
                                       DateOfBirth = @DateOfBirth,
                                       Weight = @Weight
                                   WHERE PetID = @PetID";

                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@PetID", EditPet.PetID);
                cmd.Parameters.AddWithValue("@Name", EditPet.Name);
                cmd.Parameters.AddWithValue("@Species", (object?)EditPet.Species ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Breed", (object?)EditPet.Breed ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", (object?)EditPet.Gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateOfBirth",
                    EditPet.DateOfBirth.HasValue ? EditPet.DateOfBirth.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Weight",
                    EditPet.Weight.HasValue ? EditPet.Weight.Value : (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToPage("/Pet/PetList");
        }
    }
}
