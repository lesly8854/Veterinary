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

namespace Veterinary.Pages.Pet
{
    public class AddAppointmentModel : PageModel
    {
        [BindProperty]
        public AppointmentView NewAppointment { get; set; } = new AppointmentView();

        // UI-only fields for separate date + time (no seconds)
        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today;

        [BindProperty]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; } = new TimeSpan(9, 0, 0); // 9:00 AM default

        // for dropdown of THIS owner’s pets
        public List<SelectListItem> PetOptions { get; set; } = new List<SelectListItem>();

        public IActionResult OnGet()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
            {
                return RedirectToPage("/Account/Signin");
            }

            int ownerId = int.Parse(ownerIdClaim.Value);

            // Pre-fill owner in the model
            NewAppointment.OwnerID = ownerId;

            // set sensible defaults for date/time
            AppointmentDate = DateTime.Today;
            AppointmentTime = new TimeSpan(9, 0, 0); // 9:00 AM

            // Load this owner’s pets for dropdown
            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                string cmdText = @"SELECT PetID, Name 
                                   FROM Pet 
                                   WHERE OwnerID = @OwnerID";

                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@OwnerID", ownerId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    PetOptions.Add(new SelectListItem
                    {
                        Value = reader.GetInt32(0).ToString(),   // PetID
                        Text = reader.GetString(1)              // Pet Name
                    });
                }
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
            {
                return RedirectToPage("/Account/Signin");
            }

            int ownerId = int.Parse(ownerIdClaim.Value);

            if (!ModelState.IsValid)
            {
                // reload pet dropdown if validation fails
                ReloadPetOptions(ownerId);
                return Page();
            }

            // Combine date + time into one DateTime (minute precision)
            var combinedDateTime = AppointmentDate.Date + AppointmentTime;
            NewAppointment.Date = combinedDateTime;

            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                string cmdText = @"INSERT INTO Appointment
                                   (PetID, OwnerID, Date, Reason, Status)
                                   VALUES (@PetID, @OwnerID, @Date, @Reason, @Status)";

                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@PetID", NewAppointment.PetID);
                cmd.Parameters.AddWithValue("@OwnerID", ownerId);
                cmd.Parameters.AddWithValue("@Date", NewAppointment.Date);
                cmd.Parameters.AddWithValue("@Reason", (object?)NewAppointment.Reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status",
                    string.IsNullOrWhiteSpace(NewAppointment.Status) ? "Scheduled" : NewAppointment.Status);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToPage("/Pet/AppointmentList");
        }

        // helper to repopulate dropdown on validation error
        private void ReloadPetOptions(int ownerId)
        {
            PetOptions = new List<SelectListItem>();

            using (SqlConnection conn = new SqlConnection(AppHelper.GetDBConnectionString()))
            {
                string cmdText = @"SELECT PetID, Name 
                                   FROM Pet 
                                   WHERE OwnerID = @OwnerID";

                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@OwnerID", ownerId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    PetOptions.Add(new SelectListItem
                    {
                        Value = reader.GetInt32(0).ToString(),
                        Text = reader.GetString(1)
                    });
                }
            }
        }
    }
}
