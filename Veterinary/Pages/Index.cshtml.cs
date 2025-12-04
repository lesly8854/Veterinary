using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Veterinary_Business;
using Veterinary.Model;

namespace Veterinary.Pages;

// This PageModel handles the logic for the homepage (Index page)
public class IndexModel : PageModel
{
    // List to store the pets that will be displayed on the homepage
    public List<PetDisplay> PetDisplays { get; set; } = new List<PetDisplay>();

    // On GET request, populate the list of pets
    public void OnGet()
    {
        PopulatePetDisplays();
    }

    // Method to retrieve pet data from the database
    private void PopulatePetDisplays()
    {
        // Establish a SQL connection using the connection string
        using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
        {
            // SQL command to select pet data
            string cmdText = "SELECT p.PetID, p.OwnerID, p.Name, p.Species, p.Breed, p.Weight, p.Gender, p.DateOfBirth " +
                             "FROM Pet p";

            SqlCommand cmd = new SqlCommand(cmdText, conn);
            conn.Open(); // Open the connection
            SqlDataReader reader = cmd.ExecuteReader(); // Execute the query

            // If the query returns rows, read through each row
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    // Create a PetDisplay object for each pet
                    PetDisplay petDisplay = new PetDisplay
                    {
                        PetID = reader.GetInt32(0),      // Primary key
                        OwnerID = reader.GetInt32(1),    // Foreign key
                        PetName = reader.GetString(2),   // Pet name
                        Species = reader.GetString(3),   // Species
                        Breed = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Weight = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                        Gender = reader.IsDBNull(6) ? null : reader.GetString(6),
                        DateOfBirth = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
                    };

                    // Add the pet to the list for display
                    PetDisplays.Add(petDisplay);
                }
            }
        }
    }
}
