using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Veterinary.Model;
using Veterinary_Business;

namespace Veterinary.Pages.Appointments
{
    [Authorize(Roles = "Vet")] // ? only vets can edit
    public class EditAppointmentModel : PageModel
    {
        [BindProperty]
        public AppointmentView Appointment { get; set; } = new();

        // Split date + time for UI (minute precision)
        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [BindProperty]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }

        // Display-only fields
        public string PetName { get; set; } = "";
        public string OwnerUsername { get; set; } = "";

        public IActionResult OnGet(int id)
        {
            // Vet must be logged in
            var vetIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (vetIdClaim == null)
            {
                return RedirectToPage("/Account/Signin");
            }

            int vetId = int.Parse(vetIdClaim.Value);

            using (SqlConnection conn = new(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                string cmdText = @"
                    SELECT a.AppointmentID,
                           a.PetID,
                           p.Name       AS PetName,
                           a.Date,
                           a.Reason,
                           a.Status,
                           a.VetID,
                           o.Username   AS OwnerUsername
                    FROM Appointment a
                    INNER JOIN Pet      p ON a.PetID = p.PetID
                    INNER JOIN PetOwner o ON a.OwnerID = o.OwnerID
                    WHERE a.AppointmentID = @id";

                using var cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return NotFound();
                }

                int recordVetId = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);

                // Extra safety: vet can only edit their own appointments
                if (recordVetId != vetId)
                {
                    return Forbid();
                }

                Appointment.AppointmentID = reader.GetInt32(0);
                Appointment.PetID = reader.GetInt32(1);
                PetName = reader.GetString(2);

                var fullDateTime = reader.GetDateTime(3);
                AppointmentDate = fullDateTime.Date;
                AppointmentTime = new TimeSpan(fullDateTime.Hour, fullDateTime.Minute, 0);

                Appointment.Reason = reader.IsDBNull(4) ? null : reader.GetString(4);
                Appointment.Status = reader.IsDBNull(5) ? "Scheduled" : reader.GetString(5);
                Appointment.VetID = recordVetId;

                OwnerUsername = reader.GetString(7);
            }

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            var vetIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (vetIdClaim == null)
            {
                return RedirectToPage("/Account/Signin");
            }

            int vetId = int.Parse(vetIdClaim.Value);

            if (!ModelState.IsValid)
            {
                // We need PetName/OwnerUsername again for redisplay
                ReloadDisplayFields(id, vetId);
                return Page();
            }

            // Combine date + time
            var combinedDateTime = AppointmentDate.Date + AppointmentTime;
            Appointment.Date = combinedDateTime;

            using (SqlConnection conn = new(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                string cmdText = @"
                    UPDATE Appointment
                    SET Date   = @Date,
                        Reason = @Reason,
                        Status = @Status
                    WHERE AppointmentID = @Id
                      AND VetID = @VetID";  // safety: only their own appts

                using var cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@Date", Appointment.Date);
                cmd.Parameters.AddWithValue("@Reason", (object?)Appointment.Reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status",
                    string.IsNullOrWhiteSpace(Appointment.Status) ? "Scheduled" : Appointment.Status);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@VetID", vetId);

                cmd.ExecuteNonQuery();
            }

            return RedirectToPage("/Appointments/AppointmentList");
        }

        // When ModelState fails, we need PetName + OwnerUsername again
        private void ReloadDisplayFields(int id, int vetId)
        {
            using (SqlConnection conn = new(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                string cmdText = @"
                    SELECT p.Name, o.Username
                    FROM Appointment a
                    INNER JOIN Pet      p ON a.PetID = p.PetID
                    INNER JOIN PetOwner o ON a.OwnerID = o.OwnerID
                    WHERE a.AppointmentID = @id
                      AND a.VetID = @VetID";

                using var cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@VetID", vetId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    PetName = reader.GetString(0);
                    OwnerUsername = reader.GetString(1);
                }
            }
        }
    }
}
