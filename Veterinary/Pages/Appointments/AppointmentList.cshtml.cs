using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Veterinary.Model;
using Veterinary_Business;

namespace Veterinary.Pages.Appointments
{
    public class AppointmentListModel : PageModel
    {
        public List<AppointmentView> Appointments { get; set; } = new();

        public void OnGet()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(idClaim))
            {
                Appointments = new List<AppointmentView>();
                return;
            }

            using (var conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                // ---------------- OWNER VIEW ----------------
                if (role == "Owner")
                {
                    int ownerId = int.Parse(idClaim);

                    string sql = @"
                        SELECT 
                            a.AppointmentID,
                            a.PetID,
                            p.Name              AS PetName,
                            a.Date,
                            a.Reason,
                            a.Status,
                            a.VetID,
                            v.AdminName         AS VetName
                        FROM Appointment a
                        INNER JOIN Pet              p ON a.PetID = p.PetID
                        LEFT JOIN  VeterinarianAdmin v ON a.VetID = v.VetID
                        WHERE a.OwnerID = @OwnerID
                        ORDER BY a.Date";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@OwnerID", ownerId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Appointments.Add(new AppointmentView
                                {
                                    AppointmentID = reader.GetInt32(0),
                                    PetID = reader.GetInt32(1),
                                    PetName = reader.GetString(2),
                                    Date = reader.GetDateTime(3),
                                    Reason = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    Status = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    VetID = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                    VetName = reader.IsDBNull(7) ? "" : reader.GetString(7)
                                });
                            }
                        }
                    }
                }
                // ---------------- VET VIEW ----------------
                else if (role == "Vet")
                {
                    int vetId = int.Parse(idClaim);

                    string sql = @"
                        SELECT 
                            a.AppointmentID,
                            a.PetID,
                            p.Name          AS PetName,
                            a.Date,
                            a.Reason,
                            a.Status,
                            o.Username      AS OwnerName
                        FROM Appointment a
                        INNER JOIN Pet      p ON a.PetID   = p.PetID
                        INNER JOIN PetOwner o ON a.OwnerID = o.OwnerID
                        WHERE a.VetID = @VetID
                        ORDER BY a.Date";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@VetID", vetId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Appointments.Add(new AppointmentView
                                {
                                    AppointmentID = reader.GetInt32(0),
                                    PetID = reader.GetInt32(1),
                                    PetName = reader.GetString(2),
                                    Date = reader.GetDateTime(3),
                                    Reason = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    Status = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    OwnerName = reader.GetString(6)
                                });
                            }
                        }
                    }
                }
            }
        }

        // ---------------- CANCEL / UNASSIGN (Owner only) ----------------
        public IActionResult OnPostCancel(int id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (role != "Owner" || string.IsNullOrEmpty(idClaim))
            {
                return Forbid();
            }

            int ownerId = int.Parse(idClaim);

            using (var conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                // Soft-cancel: mark as Cancelled and unassign the vet
                string sql = @"
                    UPDATE Appointment
                    SET Status = 'Cancelled',
                        VetID  = NULL
                    WHERE AppointmentID = @AppointmentID
                      AND OwnerID       = @OwnerID;";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AppointmentID", id);
                    cmd.Parameters.AddWithValue("@OwnerID", ownerId);
                    cmd.ExecuteNonQuery();
                }
            }

            // reload list
            return RedirectToPage();
        }
    }
}
