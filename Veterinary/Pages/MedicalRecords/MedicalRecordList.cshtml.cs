using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Veterinary_Business;

namespace Veterinary.Pages.MedicalRecords
{
    [Authorize(Roles = "Vet")]
    public class MedicalRecordListModel : PageModel
    {
        public List<MedicalRecordView> Records { get; set; } = new();

        public void OnGet()
        {
            // Get current vet id from claims
            var vetIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (vetIdClaim == null)
            {
                Records = new List<MedicalRecordView>();
                return;
            }

            int vetId = int.Parse(vetIdClaim.Value);

            using (var conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                string cmdText = @"
                    SELECT 
                        mr.RecordID,
                        mr.PetID,
                        p.Name              AS PetName,
                        mr.AppointmentID,
                        a.Date              AS AppointmentDate,
                        mr.Diagnosis,
                        mr.Treatment,
                        mr.Prescription,
                        mr.Vaccination,
                        mr.Notes,
                        mr.DateRecorded
                    FROM MedicalRecord mr
                    LEFT JOIN Pet        p ON mr.PetID = p.PetID
                    LEFT JOIN Appointment a ON mr.AppointmentID = a.AppointmentID
                    WHERE mr.VetID = @VetID
                    ORDER BY mr.DateRecorded DESC";

                using (var cmd = new SqlCommand(cmdText, conn))
                {
                    cmd.Parameters.AddWithValue("@VetID", vetId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var rec = new MedicalRecordView
                            {
                                RecordID = reader.GetInt32(0),
                                PetID = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                PetName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                AppointmentID = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                                AppointmentDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                Diagnosis = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                Treatment = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                Prescription = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                                Vaccination = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                                Notes = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                                DateRecorded = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10)
                            };

                            Records.Add(rec);
                        }
                    }
                }
            }
        }

        public class MedicalRecordView
        {
            public int RecordID { get; set; }

            public int? PetID { get; set; }
            public string PetName { get; set; }

            public int? AppointmentID { get; set; }
            public DateTime? AppointmentDate { get; set; }

            public string Diagnosis { get; set; }
            public string Treatment { get; set; }
            public string Prescription { get; set; }
            public string Vaccination { get; set; }
            public string Notes { get; set; }

            public DateTime? DateRecorded { get; set; }
        }
    }
}
