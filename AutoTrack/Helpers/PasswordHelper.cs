using System;
using System.Security.Cryptography;

namespace AutoTrack.Helpers
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;
        private const string AlgorithmTag = "PBKDF2";

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.", nameof(password));

            using (var rng = RandomNumberGenerator.Create())
            {
                var salt = new byte[SaltSize];
                rng.GetBytes(salt);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
                {
                    byte[] key = pbkdf2.GetBytes(KeySize);
                    return $"{AlgorithmTag}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
                }
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword) || enteredPassword == null)
                return false;

            if (!IsPasswordHashed(storedPassword))
                return string.Equals(storedPassword, enteredPassword, StringComparison.Ordinal);

            string[] parts = storedPassword.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out int iterations))
                return false;

            byte[] salt;
            byte[] expectedKey;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedKey = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, iterations))
            {
                byte[] actualKey = pbkdf2.GetBytes(expectedKey.Length);
                return SlowEquals(actualKey, expectedKey);
            }
        }

        public static bool IsPasswordHashed(string storedPassword)
        {
            return !string.IsNullOrEmpty(storedPassword) && storedPassword.StartsWith(AlgorithmTag + "$", StringComparison.Ordinal);
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
