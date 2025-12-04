using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Veterinary_Business;

namespace Veterinary.Pages.MedicalRecords
{
    [Authorize(Roles = "Vet")]
    public class AddMedicalRecordModel : PageModel
    {
        // Dropdown of this vet's appointments (only those WITHOUT a medical record yet)
        public List<SelectListItem> AppointmentOptions { get; set; } = new();

        [BindProperty]
        [Required(ErrorMessage = "Please select an appointment.")]
        public int SelectedAppointmentID { get; set; }

        // Medical record fields
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

        public IActionResult OnGet()
        {
            var vetIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (vetIdClaim == null)
            {
                return RedirectToPage("/Account/Signin");
            }

            int vetId = int.Parse(vetIdClaim.Value);
            LoadAppointmentOptions(vetId);

            return Page();
        }

        public IActionResult OnPost()
        {
            var vetIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (vetIdClaim == null)
            {
                return RedirectToPage("/Account/Signin");
            }

            int vetId = int.Parse(vetIdClaim.Value);

            if (!ModelState.IsValid)
            {
                LoadAppointmentOptions(vetId);
                return Page();
            }

            int petId;

            using (var conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                // 1) Verify this appointment belongs to this vet and get the PetID
                string getApptSql = @"
                    SELECT PetID
                    FROM Appointment
                    WHERE AppointmentID = @AppointmentID
                      AND VetID = @VetID;";

                using (var getApptCmd = new SqlCommand(getApptSql, conn))
                {
                    getApptCmd.Parameters.AddWithValue("@AppointmentID", SelectedAppointmentID);
                    getApptCmd.Parameters.AddWithValue("@VetID", vetId);

                    var result = getApptCmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        ModelState.AddModelError(string.Empty, "Invalid appointment selection.");
                        LoadAppointmentOptions(vetId);
                        return Page();
                    }

                    petId = Convert.ToInt32(result);
                }

                // 2) Make sure THIS APPOINTMENT doesn't already have a medical record
                string checkExistingSql = @"
                    SELECT COUNT(*)
                    FROM MedicalRecord
                    WHERE AppointmentID = @AppointmentID;";

                using (var checkCmd = new SqlCommand(checkExistingSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@AppointmentID", SelectedAppointmentID);

                    int existingCount = (int)checkCmd.ExecuteScalar();
                    if (existingCount > 0)
                    {
                        ModelState.AddModelError(string.Empty,
                            "A medical record for this appointment already exists.");
                        LoadAppointmentOptions(vetId);
                        return Page();
                    }
                }

                // 3) Insert the medical record
                string insertSql = @"
                    INSERT INTO MedicalRecord
                        (PetID, VetID, AppointmentID, Diagnosis, Treatment, Prescription, Vaccination, Notes, DateRecorded)
                    VALUES
                        (@PetID, @VetID, @AppointmentID, @Diagnosis, @Treatment, @Prescription, @Vaccination, @Notes, @DateRecorded);";

                using (var insertCmd = new SqlCommand(insertSql, conn))
                {
                    insertCmd.Parameters.AddWithValue("@PetID", petId);
                    insertCmd.Parameters.AddWithValue("@VetID", vetId);
                    insertCmd.Parameters.AddWithValue("@AppointmentID", SelectedAppointmentID);
                    insertCmd.Parameters.AddWithValue("@Diagnosis", Diagnosis ?? string.Empty);
                    insertCmd.Parameters.AddWithValue("@Treatment", Treatment ?? string.Empty);
                    insertCmd.Parameters.AddWithValue("@Prescription", (object?)Prescription ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@Vaccination", (object?)Vaccination ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@Notes", (object?)Notes ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@DateRecorded", DateTime.Now);

                    insertCmd.ExecuteNonQuery();
                }
            }

            return RedirectToPage("/MedicalRecords/MedicalRecordList");
        }

        private void LoadAppointmentOptions(int vetId)
        {
            AppointmentOptions = new List<SelectListItem>();

            using (var conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                // IMPORTANT:
                //  - Join on AppointmentID
                //  - No DISTINCT or GROUP BY
                //  - Filter out ONLY appointments that already have a record for that AppointmentID
                string sql = @"
                    SELECT 
                        a.AppointmentID,
                        p.Name      AS PetName,
                        a.Date      AS ApptDate
                    FROM Appointment a
                    INNER JOIN Pet p ON a.PetID = p.PetID
                    LEFT JOIN MedicalRecord mr
                           ON mr.AppointmentID = a.AppointmentID
                    WHERE a.VetID = @VetID
                      AND mr.AppointmentID IS NULL   -- no record yet for this appointment
                    ORDER BY a.Date DESC;";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@VetID", vetId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int appointmentId = reader.GetInt32(0);
                            string petName = reader.GetString(1);
                            DateTime date = reader.GetDateTime(2);

                            AppointmentOptions.Add(new SelectListItem
                            {
                                Value = appointmentId.ToString(),
                                Text = $"{petName} - {date:g}"
                            });
                        }
                    }
                }
            }
        }
    }
}
