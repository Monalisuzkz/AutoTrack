using System;
using System.Security.Cryptography;
using System.Text;

namespace AutoTrack.Helpers
{
    public static class PasswordHelper
    {
        // Hash a password using SHA256 (you can upgrade to BCrypt later)
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Verify a password against its hash
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            string hashOfEntered = HashPassword(enteredPassword);
            return hashOfEntered == storedHash;
        }
    }
}