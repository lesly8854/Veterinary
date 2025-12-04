using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Veterinary_Business;

namespace Veterinary.Pages.MedicalRecords
{
    [Authorize(Roles = "Vet")]
    public class EditMedicalRecordModel : PageModel
    {
        // Editable fields
        [BindProperty]
        public int RecordID { get; set; }

        [BindProperty]
        [Required]
        [StringLength(100)]
        public string Diagnosis { get; set; }

        [BindProperty]
        [Required]
        [StringLength(100)]
        public string Treatment { get; set; }

        [BindProperty]
        [StringLength(100)]
        public string Prescription { get; set; }

        [BindProperty]
        [StringLength(100)]
        public string Vaccination { get; set; }

        [BindProperty]
        [StringLength(500)]
        public string Notes { get; set; }

        // Read-only display info
        public int? AppointmentID { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string PetName { get; set; } = "";
        public DateTime? DateRecorded { get; set; }

        public IActionResult OnGet(int id)
        {
            var vetIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (vetIdClaim == null)
            {
                return RedirectToPage("/Account/Signin");
            }

            int vetId = int.Parse(vetIdClaim.Value);

            using (var conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                string sql = @"
                    SELECT 
                        mr.RecordID,
                        mr.AppointmentID,
                        mr.Diagnosis,
                        mr.Treatment,
                        mr.Prescription,
                        mr.Vaccination,
                        mr.Notes,
                        mr.DateRecorded,
                        p.Name      AS PetName,
                        a.Date      AS AppointmentDate,
                        mr.VetID
                    FROM MedicalRecord mr
                    LEFT JOIN Appointment a ON mr.AppointmentID = a.AppointmentID
                    LEFT JOIN Pet        p ON mr.PetID        = p.PetID
                    WHERE mr.RecordID = @RecordID";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecordID", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return NotFound();
                        }

                        int recordVetId = reader.IsDBNull(10) ? 0 : reader.GetInt32(10);
                        if (recordVetId != vetId)
                        {
                            // This record doesn't belong to this vet
                            return Forbid();
                        }

                        RecordID = reader.GetInt32(0);
                        AppointmentID = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1);
                        Diagnosis = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        Treatment = reader.IsDBNull(3) ? "" : reader.GetString(3);
                        Prescription = reader.IsDBNull(4) ? "" : reader.GetString(4);
                        Vaccination = reader.IsDBNull(5) ? "" : reader.GetString(5);
                        Notes = reader.IsDBNull(6) ? "" : reader.GetString(6);
                        DateRecorded = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7);
                        PetName = reader.IsDBNull(8) ? "" : reader.GetString(8);
                        AppointmentDate = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9);
                    }
                }
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
                // reload read-only labels
                ReloadDisplayFields(id, vetId);
                return Page();
            }

            using (var conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                string sql = @"
                    UPDATE MedicalRecord
                    SET Diagnosis   = @Diagnosis,
                        Treatment   = @Treatment,
                        Prescription= @Prescription,
                        Vaccination = @Vaccination,
                        Notes       = @Notes
                    WHERE RecordID = @RecordID
                      AND VetID    = @VetID;";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Diagnosis", Diagnosis ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Treatment", Treatment ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Prescription", (object?)Prescription ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Vaccination", (object?)Vaccination ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", (object?)Notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RecordID", id);
                    cmd.Parameters.AddWithValue("@VetID", vetId);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToPage("/MedicalRecords/MedicalRecordList");
        }

        private void ReloadDisplayFields(int recordId, int vetId)
        {
            using (var conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                string sql = @"
                    SELECT 
                        mr.AppointmentID,
                        mr.DateRecorded,
                        p.Name      AS PetName,
                        a.Date      AS AppointmentDate
                    FROM MedicalRecord mr
                    LEFT JOIN Appointment a ON mr.AppointmentID = a.AppointmentID
                    LEFT JOIN Pet        p ON mr.PetID        = p.PetID
                    WHERE mr.RecordID = @RecordID
                      AND mr.VetID    = @VetID";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RecordID", recordId);
                    cmd.Parameters.AddWithValue("@VetID", vetId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            AppointmentID = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0);
                            DateRecorded = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);
                            PetName = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            AppointmentDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3);
                        }
                    }
                }
            }
        }
    }
}
