using System;
using System.Security.Cryptography;

namespace AutoTrack.Helpers
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 310000;
        private const string CurrentAlgorithmTag = "PBKDF2-SHA256";
        private const string LegacyAlgorithmTag = "PBKDF2";

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.", nameof(password));

            using (var rng = RandomNumberGenerator.Create())
            {
                var salt = new byte[SaltSize];
                rng.GetBytes(salt);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] key = pbkdf2.GetBytes(KeySize);
                    return $"{CurrentAlgorithmTag}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
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

            if (string.Equals(parts[0], CurrentAlgorithmTag, StringComparison.Ordinal))
                return VerifyDerivedKey(enteredPassword, salt, iterations, expectedKey, HashAlgorithmName.SHA256);

            if (string.Equals(parts[0], LegacyAlgorithmTag, StringComparison.Ordinal))
                return VerifyDerivedKey(enteredPassword, salt, iterations, expectedKey, HashAlgorithmName.SHA1);

            return false;
        }

        public static bool IsPasswordHashed(string storedPassword)
        {
            return !string.IsNullOrEmpty(storedPassword) &&
                   (storedPassword.StartsWith(CurrentAlgorithmTag + "$", StringComparison.Ordinal) ||
                    storedPassword.StartsWith(LegacyAlgorithmTag + "$", StringComparison.Ordinal));
        }

        public static bool NeedsRehash(string storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword))
                return false;

            return !storedPassword.StartsWith(CurrentAlgorithmTag + "$", StringComparison.Ordinal);
        }

        private static bool VerifyDerivedKey(string enteredPassword, byte[] salt, int iterations, byte[] expectedKey, HashAlgorithmName algorithm)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, iterations, algorithm))
            {
                byte[] actualKey = pbkdf2.GetBytes(expectedKey.Length);
                return SlowEquals(actualKey, expectedKey);
            }
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
