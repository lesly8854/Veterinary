using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Veterinary.Model;
using System;

namespace Veterinary.Pages.Billing
{
    public class EditBillingModel : PageModel
    {
        [BindProperty]
        public BillingView EditBilling { get; set; } = new BillingView();

        public IActionResult OnGet(int id)
        {
            using (SqlConnection conn = new SqlConnection(
                "Server=(localdb)\\MSSQLLocalDB;Database=Veterinary;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                string cmdText = "SELECT BillingID, AppointmentID, OwnerID, PaymentStatus, DateIssued FROM Billing WHERE BillingID = @BillingID";
                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@BillingID", id);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    return RedirectToPage("/Pet/BillingList");
                }

                reader.Read();

                EditBilling.BillingID = reader.GetInt32(0);
                EditBilling.AppointmentID = reader.GetInt32(1);
                EditBilling.OwnerID = reader.GetInt32(2);
                EditBilling.PaymentStatus = reader.IsDBNull(3) ? null : reader.GetString(3);
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            using (SqlConnection conn = new SqlConnection(
                "Server=(localdb)\\MSSQLLocalDB;Database=Veterinary;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                string cmdText = @"UPDATE Billing 
                                   SET PaymentStatus = @Status
                                   WHERE BillingID = @BillingID";

                SqlCommand cmd = new SqlCommand(cmdText, conn);
                cmd.Parameters.AddWithValue("@BillingID", EditBilling.BillingID);
                cmd.Parameters.AddWithValue("@Status", (object?)EditBilling.PaymentStatus ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToPage("/Pet/BillingList");
        }
    }
}
