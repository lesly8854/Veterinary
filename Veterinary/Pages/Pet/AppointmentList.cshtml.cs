using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Veterinary.Model;
using Veterinary_Business;

namespace Veterinary.Pages.Pet
{
    public class AppointmentListModel : PageModel
    {
        public List<AppointmentView> Appointments { get; set; } = new List<AppointmentView>();

        public void OnGet()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
            {
                Appointments = new List<AppointmentView>();
                return;
            }

            int ownerId = int.Parse(ownerIdClaim.Value);

            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                string cmdText = @"
                    SELECT a.AppointmentID,
                           a.PetID,
                           p.Name AS PetName,
                           a.Date,
                           a.Reason,
                           a.Status
                    FROM Appointment a
                    INNER JOIN Pet p ON a.PetID = p.PetID
                    WHERE a.OwnerID = @OwnerID
                    ORDER BY a.Date";

                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@OwnerID", ownerId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Appointments.Add(new AppointmentView
                    {
                        AppointmentID = reader.GetInt32(0),
                        PetID = reader.GetInt32(1),
                        PetName = reader.GetString(2),
                        Date = reader.GetDateTime(3),
                        Reason = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Status = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }
        }
    }
}
