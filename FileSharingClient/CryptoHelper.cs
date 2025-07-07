using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.IO.Compression;

namespace FileSharingClient
{
    public static class CryptoHelper
    {
        private const int KEY_SIZE = 32; // 256 bits
        private const int IV_SIZE = 16; // 128 bits for CBC mode
        private const int SALT_SIZE = 16; // 128 bits
        private const int HMAC_SIZE = 32; // 256 bits for HMAC-SHA256
        private const int ITERATIONS = 100000; // Enhanced PBKDF2 iterations
        private const int CHUNK_SIZE = 64 * 1024; // 64KB chunks for large files
        private const int SESSION_KEY_SIZE = 32; // 256 bits for session keys

        // Enhanced: Progress reporting delegate
        public delegate void ProgressReportDelegate(long processedBytes, long totalBytes, string operation);

        // Enhanced: Compression level enum
        public enum CompressionLevel
        {
            None = 0,
            Fastest = 1,
            Optimal = 2,
            SmallestSize = 3
        }

        /// <summary>
        /// Enhanced: Generate cryptographically secure session key
        /// </summary>
        public static byte[] GenerateSessionKey()
        {
            byte[] sessionKey = new byte[SESSION_KEY_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(sessionKey);
            }
            return sessionKey;
        }

        /// <summary>
        /// Enhanced: Derive multiple keys from password using PBKDF2 with enhanced security
        /// </summary>
        public static (byte[] encryptionKey, byte[] hmacKey, byte[] authKey) DeriveMultipleKeys(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS))
            {
                byte[] encryptionKey = pbkdf2.GetBytes(KEY_SIZE);
                byte[] hmacKey = pbkdf2.GetBytes(HMAC_SIZE);
                byte[] authKey = pbkdf2.GetBytes(KEY_SIZE);
                return (encryptionKey, hmacKey, authKey);
            }
        }

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

        // ======================== ENHANCED FEATURES ========================

        /// <summary>
        /// Enhanced: Compress data using GZIP
        /// </summary>
        public static byte[] CompressData(byte[] data, CompressionLevel level = CompressionLevel.Optimal)
        {
            if (level == CompressionLevel.None) return data;

            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, (System.IO.Compression.CompressionLevel)(int)level))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }

        /// <summary>
        /// Enhanced: Decompress GZIP data
        /// </summary>
        public static byte[] DecompressData(byte[] compressedData)
        {
            using (var input = new MemoryStream(compressedData))
            using (var output = new MemoryStream())
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }

        /// <summary>
        /// Enhanced: Encrypt file with compression and progress tracking
        /// </summary>
        public static byte[] EncryptFileEnhanced(byte[] fileData, string userPassword, 
            CompressionLevel compression = CompressionLevel.Optimal, 
            ProgressReportDelegate progressCallback = null)
        {
            try
            {
                progressCallback?.Invoke(0, fileData.Length, "Starting encryption");

                // Step 1: Compress if needed
                byte[] dataToEncrypt = fileData;
                bool isCompressed = compression != CompressionLevel.None;
                
                if (isCompressed)
                {
                    progressCallback?.Invoke(fileData.Length / 4, fileData.Length, "Compressing");
                    dataToEncrypt = CompressData(fileData, compression);
                }

                // Step 2: Generate salt and IV
                byte[] salt = GenerateSalt();
                byte[] iv = GenerateIV();
                
                // Step 3: Derive keys
                progressCallback?.Invoke(fileData.Length / 2, fileData.Length, "Deriving keys");
                var (encryptionKey, hmacKey, _) = DeriveMultipleKeys(userPassword, salt);
                
                // Step 4: Create metadata
                byte[] metadata = new byte[1];
                metadata[0] = (byte)(isCompressed ? 1 : 0);

                // Step 5: Encrypt using AES-256-CBC
                progressCallback?.Invoke(fileData.Length * 3 / 4, fileData.Length, "Encrypting");
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
                        // Write metadata first
                        csEncrypt.Write(metadata, 0, metadata.Length);
                        // Write compressed/original data
                        csEncrypt.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                        csEncrypt.FlushFinalBlock();
                        ciphertext = msEncrypt.ToArray();
                    }
                }

                // Step 6: Calculate HMAC for authentication
                progressCallback?.Invoke(fileData.Length * 9 / 10, fileData.Length, "Calculating HMAC");
                byte[] hmac;
                using (var hmacSha256 = new HMACSHA256(hmacKey))
                {
                    // HMAC over: IV + Ciphertext
                    byte[] dataToAuthenticate = new byte[IV_SIZE + ciphertext.Length];
                    Buffer.BlockCopy(iv, 0, dataToAuthenticate, 0, IV_SIZE);
                    Buffer.BlockCopy(ciphertext, 0, dataToAuthenticate, IV_SIZE, ciphertext.Length);
                    
                    hmac = hmacSha256.ComputeHash(dataToAuthenticate);
                }

                // Step 7: Combine final result
                // Format: [SALT(16)] + [IV(16)] + [HMAC(32)] + [CIPHERTEXT]
                byte[] result = new byte[SALT_SIZE + IV_SIZE + HMAC_SIZE + ciphertext.Length];
                
                Buffer.BlockCopy(salt, 0, result, 0, SALT_SIZE);
                Buffer.BlockCopy(iv, 0, result, SALT_SIZE, IV_SIZE);
                Buffer.BlockCopy(hmac, 0, result, SALT_SIZE + IV_SIZE, HMAC_SIZE);
                Buffer.BlockCopy(ciphertext, 0, result, SALT_SIZE + IV_SIZE + HMAC_SIZE, ciphertext.Length);

                progressCallback?.Invoke(fileData.Length, fileData.Length, "Encryption completed");
                return result;
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Enhanced encryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Enhanced: Decrypt file with decompression and progress tracking
        /// </summary>
        public static byte[] DecryptFileEnhanced(byte[] encryptedData, string userPassword, 
            ProgressReportDelegate progressCallback = null)
        {
            try
            {
                progressCallback?.Invoke(0, encryptedData.Length, "Starting decryption");

                if (encryptedData.Length < SALT_SIZE + IV_SIZE + HMAC_SIZE + 1)
                    throw new CryptographicException("Invalid encrypted data format");

                // Step 1: Extract components
                progressCallback?.Invoke(encryptedData.Length / 10, encryptedData.Length, "Extracting components");
                byte[] salt = new byte[SALT_SIZE];
                byte[] iv = new byte[IV_SIZE];
                byte[] storedHmac = new byte[HMAC_SIZE];
                byte[] ciphertext = new byte[encryptedData.Length - SALT_SIZE - IV_SIZE - HMAC_SIZE];

                Buffer.BlockCopy(encryptedData, 0, salt, 0, SALT_SIZE);
                Buffer.BlockCopy(encryptedData, SALT_SIZE, iv, 0, IV_SIZE);
                Buffer.BlockCopy(encryptedData, SALT_SIZE + IV_SIZE, storedHmac, 0, HMAC_SIZE);
                Buffer.BlockCopy(encryptedData, SALT_SIZE + IV_SIZE + HMAC_SIZE, ciphertext, 0, ciphertext.Length);

                // Step 2: Derive keys
                progressCallback?.Invoke(encryptedData.Length / 4, encryptedData.Length, "Deriving keys");
                var (encryptionKey, hmacKey, _) = DeriveMultipleKeys(userPassword, salt);

                // Step 3: Verify HMAC
                progressCallback?.Invoke(encryptedData.Length / 3, encryptedData.Length, "Verifying integrity");
                using (var hmacSha256 = new HMACSHA256(hmacKey))
                {
                    byte[] dataToAuthenticate = new byte[IV_SIZE + ciphertext.Length];
                    Buffer.BlockCopy(iv, 0, dataToAuthenticate, 0, IV_SIZE);
                    Buffer.BlockCopy(ciphertext, 0, dataToAuthenticate, IV_SIZE, ciphertext.Length);
                    
                    byte[] computedHmac = hmacSha256.ComputeHash(dataToAuthenticate);
                    
                    if (!ConstantTimeEquals(storedHmac, computedHmac))
                        throw new CryptographicException("HMAC verification failed - data may be corrupted or tampered with");
                }

                // Step 4: Decrypt
                progressCallback?.Invoke(encryptedData.Length * 2 / 3, encryptedData.Length, "Decrypting");
                byte[] decryptedData;
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
                        decryptedData = msPlaintext.ToArray();
                    }
                }

                // Step 5: Extract metadata and data
                if (decryptedData.Length < 1)
                    throw new CryptographicException("Decrypted data too small");

                bool isCompressed = decryptedData[0] == 1;
                byte[] fileData = new byte[decryptedData.Length - 1];
                Buffer.BlockCopy(decryptedData, 1, fileData, 0, fileData.Length);

                // Step 6: Decompress if needed
                if (isCompressed)
                {
                    progressCallback?.Invoke(encryptedData.Length * 9 / 10, encryptedData.Length, "Decompressing");
                    fileData = DecompressData(fileData);
                }

                progressCallback?.Invoke(encryptedData.Length, encryptedData.Length, "Decryption completed");
                return fileData;
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Enhanced decryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Enhanced: Encrypt with session key instead of password
        /// </summary>
        public static byte[] EncryptWithSessionKey(byte[] fileData, byte[] sessionKey, 
            CompressionLevel compression = CompressionLevel.Optimal)
        {
            try
            {
                // Compress if needed
                byte[] dataToEncrypt = fileData;
                bool isCompressed = compression != CompressionLevel.None;
                
                if (isCompressed)
                {
                    dataToEncrypt = CompressData(fileData, compression);
                }

                // Generate IV
                byte[] iv = GenerateIV();
                
                // Create metadata
                byte[] metadata = new byte[1];
                metadata[0] = (byte)(isCompressed ? 1 : 0);

                // Encrypt using session key directly
                byte[] ciphertext;
                using (var aes = new AesCng())
                {
                    aes.Key = sessionKey;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    
                    using (var encryptor = aes.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(metadata, 0, metadata.Length);
                        csEncrypt.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                        csEncrypt.FlushFinalBlock();
                        ciphertext = msEncrypt.ToArray();
                    }
                }

                // Calculate HMAC using session key
                byte[] hmac;
                using (var hmacSha256 = new HMACSHA256(sessionKey))
                {
                    byte[] dataToAuthenticate = new byte[IV_SIZE + ciphertext.Length];
                    Buffer.BlockCopy(iv, 0, dataToAuthenticate, 0, IV_SIZE);
                    Buffer.BlockCopy(ciphertext, 0, dataToAuthenticate, IV_SIZE, ciphertext.Length);
                    
                    hmac = hmacSha256.ComputeHash(dataToAuthenticate);
                }

                // Combine: [IV(16)] + [HMAC(32)] + [CIPHERTEXT]
                byte[] result = new byte[IV_SIZE + HMAC_SIZE + ciphertext.Length];
                
                Buffer.BlockCopy(iv, 0, result, 0, IV_SIZE);
                Buffer.BlockCopy(hmac, 0, result, IV_SIZE, HMAC_SIZE);
                Buffer.BlockCopy(ciphertext, 0, result, IV_SIZE + HMAC_SIZE, ciphertext.Length);

                return result;
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Session key encryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Enhanced: Decrypt with session key
        /// </summary>
        public static byte[] DecryptWithSessionKey(byte[] encryptedData, byte[] sessionKey)
        {
            try
            {
                if (encryptedData.Length < IV_SIZE + HMAC_SIZE + 1)
                    throw new CryptographicException("Invalid session encrypted data format");

                // Extract components
                byte[] iv = new byte[IV_SIZE];
                byte[] storedHmac = new byte[HMAC_SIZE];
                byte[] ciphertext = new byte[encryptedData.Length - IV_SIZE - HMAC_SIZE];

                Buffer.BlockCopy(encryptedData, 0, iv, 0, IV_SIZE);
                Buffer.BlockCopy(encryptedData, IV_SIZE, storedHmac, 0, HMAC_SIZE);
                Buffer.BlockCopy(encryptedData, IV_SIZE + HMAC_SIZE, ciphertext, 0, ciphertext.Length);

                // Verify HMAC
                using (var hmacSha256 = new HMACSHA256(sessionKey))
                {
                    byte[] dataToAuthenticate = new byte[IV_SIZE + ciphertext.Length];
                    Buffer.BlockCopy(iv, 0, dataToAuthenticate, 0, IV_SIZE);
                    Buffer.BlockCopy(ciphertext, 0, dataToAuthenticate, IV_SIZE, ciphertext.Length);
                    
                    byte[] computedHmac = hmacSha256.ComputeHash(dataToAuthenticate);
                    
                    if (!ConstantTimeEquals(storedHmac, computedHmac))
                        throw new CryptographicException("Session HMAC verification failed");
                }

                // Decrypt
                byte[] decryptedData;
                using (var aes = new AesCng())
                {
                    aes.Key = sessionKey;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    
                    using (var decryptor = aes.CreateDecryptor())
                    using (var msDecrypt = new MemoryStream(ciphertext))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var msPlaintext = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msPlaintext);
                        decryptedData = msPlaintext.ToArray();
                    }
                }

                // Extract metadata and decompress if needed
                if (decryptedData.Length < 1)
                    throw new CryptographicException("Session decrypted data too small");

                bool isCompressed = decryptedData[0] == 1;
                byte[] fileData = new byte[decryptedData.Length - 1];
                Buffer.BlockCopy(decryptedData, 1, fileData, 0, fileData.Length);

                if (isCompressed)
                {
                    fileData = DecompressData(fileData);
                }

                return fileData;
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Session key decryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Enhanced: Chunked encryption for large files with progress tracking
        /// </summary>
        public static async Task<byte[]> EncryptFileChunkedAsync(string filePath, string userPassword, 
            CompressionLevel compression = CompressionLevel.Optimal,
            ProgressReportDelegate progressCallback = null)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                long totalSize = fileInfo.Length;

                progressCallback?.Invoke(0, totalSize, "Starting chunked encryption");

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var memoryStream = new MemoryStream())
                {
                    long processedBytes = 0;
                    byte[] buffer = new byte[CHUNK_SIZE];
                    
                    while (processedBytes < totalSize)
                    {
                        int bytesRead = await fileStream.ReadAsync(buffer, 0, 
                            (int)Math.Min(CHUNK_SIZE, totalSize - processedBytes));
                        
                        if (bytesRead == 0) break;
                        
                        await memoryStream.WriteAsync(buffer, 0, bytesRead);
                        processedBytes += bytesRead;
                        
                        progressCallback?.Invoke(processedBytes, totalSize, "Reading file");
                    }
                    
                    byte[] fileData = memoryStream.ToArray();
                    return EncryptFileEnhanced(fileData, userPassword, compression, progressCallback);
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Chunked encryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Enhanced: Secure random password generation
        /// </summary>
        public static string GenerateSecurePassword(int length = 16, bool includeSpecialChars = true)
        {
            const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numberChars = "0123456789";
            const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            
            string charSet = lowerChars + upperChars + numberChars;
            if (includeSpecialChars) charSet += specialChars;
            
            var password = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);
                
                for (int i = 0; i < length; i++)
                {
                    int index = randomBytes[i] % charSet.Length;
                    password.Append(charSet[index]);
                }
            }
            
            return password.ToString();
        }

        /// <summary>
        /// Enhanced: Calculate file hash with progress tracking
        /// </summary>
        public static async Task<string> CalculateFileHashAsync(string filePath, 
            ProgressReportDelegate progressCallback = null)
        {
            using (var sha256 = SHA256.Create())
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long totalSize = fileStream.Length;
                long processedBytes = 0;
                byte[] buffer = new byte[CHUNK_SIZE];
                
                progressCallback?.Invoke(0, totalSize, "Calculating hash");
                
                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    sha256.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                    processedBytes += bytesRead;
                    progressCallback?.Invoke(processedBytes, totalSize, "Calculating hash");
                }
                
                sha256.TransformFinalBlock(new byte[0], 0, 0);
                byte[] hash = sha256.Hash;
                
                progressCallback?.Invoke(totalSize, totalSize, "Hash calculation completed");
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
