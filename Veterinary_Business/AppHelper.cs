using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veterinary_Business
{
    public class AppHelper
    {
        public static string GetDBConnectionString()
        {
            return "Server=(localdb)\\MSSQLLocalDB;Database=Veterinary;Trusted_Connection=True;";
        }

        // Hash password so we dont store it as plain text in database.
        public static string GeneratePasswordHash(string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password);
            return passwordHash;
        }

        // Verify user logging in put the right password
        public static bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
        }
    }
}