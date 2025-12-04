using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Veterinary.Model;
using System;

namespace Veterinary.Pages.Pet
{
    public class BillingModel : PageModel
    {
        [BindProperty]
        public BillingView NewBilling { get; set; } = new BillingView();

        public void OnGet()
        {
            // loads page, nothing else
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            using (SqlConnection conn = new SqlConnection(
                "Server=(localdb)\\MSSQLLocalDB;Database=Veterinary;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                string cmdText = @"INSERT INTO Billing 
                                   (AppointmentID, OwnerID, PaymentStatus, DateIssued)
                                   VALUES (@AppointmentID, @OwnerID, @Status, @DateIssued)";

                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@AppointmentID", NewBilling.AppointmentID);
                cmd.Parameters.AddWithValue("@OwnerID", NewBilling.OwnerID);
                cmd.Parameters.AddWithValue("@Status", NewBilling.PaymentStatus ?? "Pending");
                cmd.Parameters.AddWithValue("@DateIssued", DateTime.Now);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToPage("/Pet/BillingList");
        }
    }
}
