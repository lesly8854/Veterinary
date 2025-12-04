using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Veterinary.Model;
using Veterinary_Business;

namespace Veterinary.Pages.Appointments
{
    [Authorize(Roles = "Owner")] // ? Only owners can create appointments
    public class AddAppointmentModel : PageModel
    {
        [BindProperty]
        public AppointmentView NewAppointment { get; set; } = new();

        // UI-only fields for date + time (minute precision, no seconds)
        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today;

        [BindProperty]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; } = new TimeSpan(9, 0, 0);

        // Dropdown lists
        public List<SelectListItem> PetOptions { get; set; } = new();
        public List<SelectListItem> VetOptions { get; set; } = new(); // ? For choosing a vet

        [BindProperty]
        public int SelectedVetID { get; set; } // ? Holds chosen vet

        public IActionResult OnGet()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
                return RedirectToPage("/Account/Signin");

            int ownerId = int.Parse(ownerIdClaim.Value);

            // Pre-fill Owner in appointment model
            NewAppointment.OwnerID = ownerId;
            NewAppointment.Status = "Scheduled";
            AppointmentTime = new TimeSpan(9, 0, 0);

            // -----------------------------
            // LOAD PET DROPDOWN (Owner Pets)
            // -----------------------------
            using (SqlConnection conn = new(AppHelper.GetDBConnectionString()))
            {
                conn.Open();
                string cmdText = @"SELECT PetID, Name FROM Pet WHERE OwnerID = @id";
                using var cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@id", ownerId);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    PetOptions.Add(new SelectListItem
                    {
                        Value = reader.GetInt32(0).ToString(),
                        Text = reader.GetString(1)
                    });
                }
            }

            // -----------------------------
            // LOAD VET DROPDOWN
            // -----------------------------
            using (SqlConnection conn = new(AppHelper.GetDBConnectionString()))
            {
                conn.Open();
                string cmdText = @"SELECT VetID, AdminName FROM VeterinarianAdmin";
                using var cmd = new SqlCommand(cmdText, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    VetOptions.Add(new SelectListItem
                    {
                        Value = reader.GetInt32(0).ToString(),  // VetID
                        Text = reader.GetString(1)            // AdminName
                    });
                }
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
                return RedirectToPage("/Account/Signin");

            int ownerId = int.Parse(ownerIdClaim.Value);

            if (!ModelState.IsValid)
            {
                OnGet(); // reload dropdowns
                return Page();
            }

            // Combine date + time into one DateTime (minute-level precision)
            NewAppointment.Date = AppointmentDate.Date + AppointmentTime;

            // INSERT APPOINTMENT WITH ASSIGNED VET
            using (SqlConnection conn = new(AppHelper.GetDBConnectionString()))
            {
                conn.Open();

                string cmdText = @"INSERT INTO Appointment
                                   (PetID, OwnerID, Date, Reason, Status, VetID)
                                   VALUES (@PetID, @OwnerID, @Date, @Reason, @Status, @VetID)";

                using var cmd = new SqlCommand(cmdText, conn);

                cmd.Parameters.AddWithValue("@PetID", NewAppointment.PetID);
                cmd.Parameters.AddWithValue("@OwnerID", ownerId);
                cmd.Parameters.AddWithValue("@Date", NewAppointment.Date);
                cmd.Parameters.AddWithValue("@Reason", (object?)NewAppointment.Reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", NewAppointment.Status);
                cmd.Parameters.AddWithValue("@VetID", SelectedVetID); // ? Assigned vet

                cmd.ExecuteNonQuery();
            }

            return RedirectToPage("/Appointments/AppointmentList");
        }
    }
}
