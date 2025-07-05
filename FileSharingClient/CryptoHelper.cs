using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace FileSharingClient
{
    public static class CryptoHelper
    {
        private const int KEY_SIZE = 32; // 256 bits
        private const int IV_SIZE = 16; // 128 bits for CBC mode
        private const int SALT_SIZE = 16; // 128 bits
        private const int HMAC_SIZE = 32; // 256 bits for HMAC-SHA256
        private const int ITERATIONS = 10000; // PBKDF2 iterations

        /// <summary>
        /// Derive encryption key from user password using PBKDF2
        /// </summary>
        public static byte[] DeriveKeyFromPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS))
            {
                return pbkdf2.GetBytes(KEY_SIZE);
            }
        }

        /// <summary>
        /// Derive HMAC key from user password using PBKDF2
        /// </summary>
        public static byte[] DeriveHMACKeyFromPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS))
            {
                // Skip the first 32 bytes (used for encryption key) and get next 32 bytes
                pbkdf2.GetBytes(KEY_SIZE);
                return pbkdf2.GetBytes(HMAC_SIZE);
            }
        }

        /// <summary>
        /// Generate random salt for key derivation
        /// </summary>
        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SALT_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Generate random IV for AES-CBC
        /// </summary>
        public static byte[] GenerateIV()
        {
            byte[] iv = new byte[IV_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }

        /// <summary>
        /// Encrypt file using AES-256-CBC + HMAC-SHA256 (compatible with .NET Framework 4.8)
        /// </summary>
        public static byte[] EncryptFile(byte[] fileData, string userPassword)
        {
            try
            {
                // Generate salt and IV
                byte[] salt = GenerateSalt();
                byte[] iv = GenerateIV();
                
                // Derive keys from password
                byte[] encryptionKey = DeriveKeyFromPassword(userPassword, salt);
                byte[] hmacKey = DeriveHMACKeyFromPassword(userPassword, salt);
                
                // Encrypt using AES-256-CBC
                byte[] ciphertext;
                using (var aes = new AesCng())
                {
                    aes.Key = encryptionKey;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    
                    using (var encryptor = aes.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(fileData, 0, fileData.Length);
                        csEncrypt.FlushFinalBlock();
                        ciphertext = msEncrypt.ToArray();
                    }
                }

                // Calculate HMAC for authentication
                byte[] hmac;
                using (var hmacSha256 = new HMACSHA256(hmacKey))
                {
                    // HMAC over: IV + Ciphertext
                    byte[] dataToAuthenticate = new byte[IV_SIZE + ciphertext.Length];
                    Buffer.BlockCopy(iv, 0, dataToAuthenticate, 0, IV_SIZE);
                    Buffer.BlockCopy(ciphertext, 0, dataToAuthenticate, IV_SIZE, ciphertext.Length);
                    
                    hmac = hmacSha256.ComputeHash(dataToAuthenticate);
                }

                // Combine: [SALT(16)] + [IV(16)] + [HMAC(32)] + [CIPHERTEXT]
                byte[] result = new byte[SALT_SIZE + IV_SIZE + HMAC_SIZE + ciphertext.Length];
                
                Buffer.BlockCopy(salt, 0, result, 0, SALT_SIZE);
                Buffer.BlockCopy(iv, 0, result, SALT_SIZE, IV_SIZE);
                Buffer.BlockCopy(hmac, 0, result, SALT_SIZE + IV_SIZE, HMAC_SIZE);
                Buffer.BlockCopy(ciphertext, 0, result, SALT_SIZE + IV_SIZE + HMAC_SIZE, ciphertext.Length);

                return result;
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Encryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Decrypt file using AES-256-CBC + HMAC-SHA256 (compatible with .NET Framework 4.8)
        /// </summary>
        public static byte[] DecryptFile(byte[] encryptedData, string userPassword)
        {
            try
            {
                if (encryptedData.Length < SALT_SIZE + IV_SIZE + HMAC_SIZE)
                    throw new CryptographicException("Invalid encrypted data format");

                // Extract components
                byte[] salt = new byte[SALT_SIZE];
                byte[] iv = new byte[IV_SIZE];
                byte[] storedHmac = new byte[HMAC_SIZE];
                byte[] ciphertext = new byte[encryptedData.Length - SALT_SIZE - IV_SIZE - HMAC_SIZE];

                Buffer.BlockCopy(encryptedData, 0, salt, 0, SALT_SIZE);
                Buffer.BlockCopy(encryptedData, SALT_SIZE, iv, 0, IV_SIZE);
                Buffer.BlockCopy(encryptedData, SALT_SIZE + IV_SIZE, storedHmac, 0, HMAC_SIZE);
                Buffer.BlockCopy(encryptedData, SALT_SIZE + IV_SIZE + HMAC_SIZE, ciphertext, 0, ciphertext.Length);

                // Derive keys from password
                byte[] encryptionKey = DeriveKeyFromPassword(userPassword, salt);
                byte[] hmacKey = DeriveHMACKeyFromPassword(userPassword, salt);

                // Verify HMAC for authentication
                using (var hmacSha256 = new HMACSHA256(hmacKey))
                {
                    // HMAC over: IV + Ciphertext
                    byte[] dataToAuthenticate = new byte[IV_SIZE + ciphertext.Length];
                    Buffer.BlockCopy(iv, 0, dataToAuthenticate, 0, IV_SIZE);
                    Buffer.BlockCopy(ciphertext, 0, dataToAuthenticate, IV_SIZE, ciphertext.Length);
                    
                    byte[] computedHmac = hmacSha256.ComputeHash(dataToAuthenticate);
                    
                    // Constant-time comparison to prevent timing attacks
                    if (!ConstantTimeEquals(storedHmac, computedHmac))
                        throw new CryptographicException("HMAC verification failed - data may be corrupted or tampered with");
                }

                // Decrypt using AES-256-CBC
                byte[] plaintext;
                using (var aes = new AesCng())
                {
                    aes.Key = encryptionKey;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    
                    using (var decryptor = aes.CreateDecryptor())
                    using (var msDecrypt = new MemoryStream(ciphertext))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var msPlaintext = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msPlaintext);
                        plaintext = msPlaintext.ToArray();
                    }
                }

                return plaintext;
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Decryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Constant-time comparison to prevent timing attacks
        /// </summary>
        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            
            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }

        /// <summary>
        /// Encrypt file from disk and return encrypted bytes
        /// </summary>
        public static byte[] EncryptFileFromDisk(string filePath, string userPassword)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            return EncryptFile(fileData, userPassword);
        }

        /// <summary>
        /// Decrypt file and save to disk
        /// </summary>
        public static void DecryptFileToLocal(byte[] encryptedData, string userPassword, string savePath)
        {
            byte[] decryptedData = DecryptFile(encryptedData, userPassword);
            File.WriteAllBytes(savePath, decryptedData);
        }

        /// <summary>
        /// Calculate SHA256 hash of encrypted file for integrity check
        /// </summary>
        public static string CalculateHash(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
