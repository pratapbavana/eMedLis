using System;
using System.Security.Cryptography;
using System.Text;

namespace eMedLis.Models
{
    public class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 20; // 160 bits
        private const int Iterations = 10000; // PBKDF2 iterations

        /// <summary>
        /// Hashes a password and returns both hash and salt as base64 strings
        /// </summary>
        public static (string hash, string salt) HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty");

            // Generate random salt
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] saltBytes = new byte[SaltSize];
                rng.GetBytes(saltBytes);

                // Hash password with salt using PBKDF2
                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    password: password,
                    salt: saltBytes,
                    iterations: Iterations,
                    hashAlgorithm: HashAlgorithmName.SHA256))
                {
                    byte[] hashBytes = pbkdf2.GetBytes(HashSize);

                    // Return as base64 strings
                    string hashBase64 = Convert.ToBase64String(hashBytes);
                    string saltBase64 = Convert.ToBase64String(saltBytes);

                    return (hashBase64, saltBase64);
                }
            }
        }

        /// <summary>
        /// Verifies a password against its stored hash
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
                return false;

            try
            {
                // Decode salt from base64
                byte[] saltBytes = Convert.FromBase64String(storedSalt);

                // Hash input password with stored salt
                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    password: password,
                    salt: saltBytes,
                    iterations: Iterations,
                    hashAlgorithm: HashAlgorithmName.SHA256))
                {
                    byte[] hashBytes = pbkdf2.GetBytes(HashSize);
                    string computedHash = Convert.ToBase64String(hashBytes);

                    // Compare with stored hash
                    return computedHash == storedHash;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
