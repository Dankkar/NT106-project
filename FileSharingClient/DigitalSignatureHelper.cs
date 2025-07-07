using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FileSharingClient
{
    public static class DigitalSignatureHelper
    {
        private const int RSA_KEY_SIZE = 2048; // RSA key size in bits
        private const string SIGNATURE_ALGORITHM = "SHA256withRSA";
        private const string CLIENT_KEYS_DIR = "ClientKeys";
        private const string USER_PRIVATE_KEY_FILE = "user_private.key";
        private const string USER_PUBLIC_KEY_FILE = "user_public.key";
        private const string SERVER_PUBLIC_KEY_FILE = "server_public.key";

        /// <summary>
        /// Digital signature information
        /// </summary>
        public class SignatureInfo
        {
            public string Algorithm { get; set; }
            public string Signature { get; set; }
            public string PublicKeyFingerprint { get; set; }
            public DateTime SignedAt { get; set; }
            public string SignerId { get; set; }
            public Dictionary<string, object> Metadata { get; set; }

            public SignatureInfo()
            {
                Algorithm = SIGNATURE_ALGORITHM;
                SignedAt = DateTime.UtcNow;
                Metadata = new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Key pair information
        /// </summary>
        public class KeyPairInfo
        {
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }
            public string Fingerprint { get; set; }
            public DateTime CreatedAt { get; set; }
            public string KeyId { get; set; }
            public int KeySize { get; set; }

            public KeyPairInfo()
            {
                CreatedAt = DateTime.UtcNow;
                KeySize = RSA_KEY_SIZE;
            }
        }

        /// <summary>
        /// File signature package for upload
        /// </summary>
        public class SignedFilePackage
        {
            public string FileName { get; set; }
            public byte[] FileData { get; set; }
            public SignatureInfo Signature { get; set; }
            public string FileHash { get; set; }
            public DateTime CreatedAt { get; set; }
            public int FileSize { get; set; }

            public SignedFilePackage()
            {
                CreatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Initialize client keys for current user
        /// </summary>
        public static async Task<bool> InitializeClientKeysAsync(string username, string userPassword)
        {
            try
            {
                return await Task.Run(() =>
                {
                    string keysDir = GetClientKeysDirectory();
                    if (!Directory.Exists(keysDir))
                    {
                        Directory.CreateDirectory(keysDir);
                    }

                    string privateKeyPath = Path.Combine(keysDir, USER_PRIVATE_KEY_FILE);
                    string publicKeyPath = Path.Combine(keysDir, USER_PUBLIC_KEY_FILE);

                    // Check if keys already exist
                    if (File.Exists(privateKeyPath) && File.Exists(publicKeyPath))
                    {
                        // Verify existing keys are valid
                        var existingKeys = LoadUserKeys(userPassword);
                        if (existingKeys != null && VerifyKeyIntegrity(existingKeys))
                        {
                            Console.WriteLine("[CLIENT SIGNATURE] User keys already exist and are valid");
                            return true;
                        }
                    }

                    // Generate new user keys
                    string keyId = $"CLIENT_{username}_{DateTime.UtcNow:yyyyMMdd}";
                    var keyPair = GenerateKeyPair(keyId);
                    
                    // Save keys to files (encrypted with user password)
                    SaveUserKeys(keyPair, userPassword);
                    
                    Console.WriteLine($"[CLIENT SIGNATURE] User keys generated for {username}");
                    return true;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to initialize client keys: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generate RSA key pair (.NET Framework 4.8 compatible)
        /// </summary>
        public static KeyPairInfo GenerateKeyPair(string keyId)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider(RSA_KEY_SIZE))
                {
                    // Export parameters
                    RSAParameters privateParams = rsa.ExportParameters(true);
                    RSAParameters publicParams = rsa.ExportParameters(false);
                    
                    // Convert to XML format (compatible with .NET Framework)
                    string privateKey = rsa.ToXmlString(true);
                    string publicKey = rsa.ToXmlString(false);
                    
                    // Calculate fingerprint
                    string fingerprint = CalculatePublicKeyFingerprint(publicKey);

                    return new KeyPairInfo
                    {
                        PublicKey = publicKey,
                        PrivateKey = privateKey,
                        Fingerprint = fingerprint,
                        KeyId = keyId
                    };
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to generate key pair: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sign file before upload
        /// </summary>
        public static async Task<SignedFilePackage> SignFileForUploadAsync(string filePath, string userPassword)
        {
            try
            {
                return await Task.Run(() =>
                {
                    // Load user keys
                    var userKeys = LoadUserKeys(userPassword);
                    if (userKeys == null)
                    {
                        throw new InvalidOperationException("User keys not available. Please initialize keys first.");
                    }

                    // Read file data
                    byte[] fileData = File.ReadAllBytes(filePath);
                    string fileName = Path.GetFileName(filePath);

                    // Sign file data
                    var signature = SignData(fileData, userKeys.PrivateKey, Session.LoggedInUser ?? "UNKNOWN");

                    // Calculate file hash
                    string fileHash = CryptoHelper.CalculateHash(fileData);

                    return new SignedFilePackage
                    {
                        FileName = fileName,
                        FileData = fileData,
                        Signature = signature,
                        FileHash = fileHash,
                        FileSize = fileData.Length
                    };
                });
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to sign file for upload: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sign data with RSA private key (.NET Framework 4.8 compatible)
        /// </summary>
        public static SignatureInfo SignData(byte[] data, string privateKeyXml, string signerId)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider(RSA_KEY_SIZE))
                {
                    rsa.FromXmlString(privateKeyXml);
                    
                    // Sign data using SHA256 with RSA
                    byte[] signature = rsa.SignData(data, new SHA256CryptoServiceProvider());
                    
                    // Get public key for fingerprint
                    string publicKeyXml = rsa.ToXmlString(false);
                    string fingerprint = CalculatePublicKeyFingerprint(publicKeyXml);

                    return new SignatureInfo
                    {
                        Signature = Convert.ToBase64String(signature),
                        PublicKeyFingerprint = fingerprint,
                        SignerId = signerId
                    };
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to sign data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verify signature with RSA public key (.NET Framework 4.8 compatible)
        /// </summary>
        public static bool VerifySignature(byte[] data, SignatureInfo signatureInfo, string publicKeyXml)
        {
            try
            {
                byte[] signature = Convert.FromBase64String(signatureInfo.Signature);
                
                using (var rsa = new RSACryptoServiceProvider(RSA_KEY_SIZE))
                {
                    rsa.FromXmlString(publicKeyXml);
                    
                    // Verify signature
                    bool isValid = rsa.VerifyData(data, new SHA256CryptoServiceProvider(), signature);
                    
                    // Additional check: verify fingerprint matches
                    string calculatedFingerprint = CalculatePublicKeyFingerprint(publicKeyXml);
                    bool fingerprintMatches = calculatedFingerprint == signatureInfo.PublicKeyFingerprint;
                    
                    return isValid && fingerprintMatches;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to verify signature: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verify downloaded file with server signature
        /// </summary>
        public static async Task<bool> VerifyDownloadedFileAsync(byte[] fileData, SignatureInfo signatureInfo)
        {
            try
            {
                return await Task.Run(() =>
                {
                    string serverPublicKey = GetServerPublicKey();
                    if (string.IsNullOrEmpty(serverPublicKey))
                    {
                        Console.WriteLine("[WARNING] Server public key not available, cannot verify file signature");
                        return false;
                    }

                    return VerifySignature(fileData, signatureInfo, serverPublicKey);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to verify downloaded file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Store server public key (received during initial connection)
        /// </summary>
        public static bool StoreServerPublicKey(string serverPublicKey)
        {
            try
            {
                string keysDir = GetClientKeysDirectory();
                if (!Directory.Exists(keysDir))
                {
                    Directory.CreateDirectory(keysDir);
                }

                string serverKeyPath = Path.Combine(keysDir, SERVER_PUBLIC_KEY_FILE);

                // Validate key format

                if (!IsValidPublicKey(serverPublicKey))
                {
                    throw new ArgumentException("Invalid server public key format");
                }

                File.WriteAllText(serverKeyPath, serverPublicKey);


                Console.WriteLine("[CLIENT SIGNATURE] Server public key stored");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to store server public key: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get stored server public key
        /// </summary>
        public static string GetServerPublicKey()
        {
            try
            {
                string keysDir = GetClientKeysDirectory();
                string serverKeyPath = Path.Combine(keysDir, SERVER_PUBLIC_KEY_FILE);

                if (!File.Exists(serverKeyPath))
                {
                    return null;
                }

                return File.ReadAllText(serverKeyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to get server public key: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get user public key for sharing with server
        /// </summary>
        public static async Task<string> GetUserPublicKeyAsync(string userPassword)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var userKeys = LoadUserKeys(userPassword);
                    return userKeys?.PublicKey;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to get user public key: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Calculate public key fingerprint (SHA256 hash) (.NET Framework 4.8 compatible)
        /// </summary>
        public static string CalculatePublicKeyFingerprint(string publicKeyXml)
        {
            try
            {
                byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKeyXml);
                using (var sha256 = new SHA256CryptoServiceProvider())
                {
                    byte[] hash = sha256.ComputeHash(publicKeyBytes);
                    return Convert.ToBase64String(hash);
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to calculate fingerprint: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Create signed file package for secure upload
        /// </summary>
        public static async Task<byte[]> CreateSignedUploadPackageAsync(string filePath, string userPassword)
        {
            try
            {
                var signedPackage = await SignFileForUploadAsync(filePath, userPassword);

                // Create package metadata
                var packageInfo = new
                {
                    FileName = signedPackage.FileName,
                    FileSize = signedPackage.FileSize,
                    FileHash = signedPackage.FileHash,
                    Signature = signedPackage.Signature,
                    CreatedAt = signedPackage.CreatedAt,
                    ClientVersion = "1.0",
                    CompressionUsed = true
                };

                string packageJson = JsonConvert.SerializeObject(packageInfo);
                byte[] packageBytes = Encoding.UTF8.GetBytes(packageJson);

                // Compress and encrypt file data
                byte[] compressedFile = CryptoHelper.CompressData(signedPackage.FileData, 
                    CryptoHelper.CompressionLevel.Optimal);
                byte[] encryptedFile = CryptoHelper.EncryptFileEnhanced(compressedFile, userPassword,
                    CryptoHelper.CompressionLevel.None); // Already compressed

                // Combine: [PACKAGE_SIZE(4)] + [PACKAGE_JSON] + [ENCRYPTED_FILE]
                byte[] result = new byte[4 + packageBytes.Length + encryptedFile.Length];
                
                Buffer.BlockCopy(BitConverter.GetBytes(packageBytes.Length), 0, result, 0, 4);
                Buffer.BlockCopy(packageBytes, 0, result, 4, packageBytes.Length);
                Buffer.BlockCopy(encryptedFile, 0, result, 4 + packageBytes.Length, encryptedFile.Length);

                return result;
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to create signed upload package: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verify and extract downloaded file package
        /// </summary>
        public static async Task<(bool isValid, string fileName, byte[] fileData)> VerifyDownloadPackageAsync(
            byte[] packageData, string userPassword)
        {
            try
            {
                if (packageData.Length < 4)
                    return (false, null, null);

                // Extract package size
                int packageSize = BitConverter.ToInt32(packageData, 0);
                if (packageSize <= 0 || packageSize > packageData.Length - 4)
                    return (false, null, null);

                // Extract package JSON
                byte[] packageBytes = new byte[packageSize];
                Buffer.BlockCopy(packageData, 4, packageBytes, 0, packageSize);
                string packageJson = Encoding.UTF8.GetString(packageBytes);

                // Parse package info using Newtonsoft.Json
                var packageInfo = JsonConvert.DeserializeObject<dynamic>(packageJson);
                
                string fileName = packageInfo.FileName;
                int expectedFileSize = packageInfo.FileSize;
                string expectedFileHash = packageInfo.FileHash;

                // Extract signature info
                var signatureData = packageInfo.Signature;
                var signature = new SignatureInfo
                {
                    Algorithm = signatureData.Algorithm,
                    Signature = signatureData.Signature,
                    PublicKeyFingerprint = signatureData.PublicKeyFingerprint,
                    SignerId = signatureData.SignerId,
                    SignedAt = DateTime.Parse(signatureData.SignedAt.ToString())
                };

                // Extract and decrypt file data
                byte[] encryptedFile = new byte[packageData.Length - 4 - packageSize];
                Buffer.BlockCopy(packageData, 4 + packageSize, encryptedFile, 0, encryptedFile.Length);

                byte[] compressedFile = CryptoHelper.DecryptFileEnhanced(encryptedFile, userPassword);
                byte[] fileData = CryptoHelper.DecompressData(compressedFile);

                // Verify file size and hash
                if (fileData.Length != expectedFileSize)
                    return (false, null, null);

                string calculatedHash = CryptoHelper.CalculateHash(fileData);
                if (calculatedHash != expectedFileHash)
                    return (false, null, null);

                // Verify signature
                bool signatureValid = await VerifyDownloadedFileAsync(fileData, signature);

                return (signatureValid, fileName, fileData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to verify download package: {ex.Message}");
                return (false, null, null);
            }
        }

        /// <summary>
        /// Save user keys encrypted with password
        /// </summary>
        private static void SaveUserKeys(KeyPairInfo keyPair, string userPassword)
        {
            try
            {
                string keysDir = GetClientKeysDirectory();
                string privateKeyPath = Path.Combine(keysDir, USER_PRIVATE_KEY_FILE);
                string publicKeyPath = Path.Combine(keysDir, USER_PUBLIC_KEY_FILE);

                // Encrypt and save private key
                byte[] privateKeyBytes = Encoding.UTF8.GetBytes(keyPair.PrivateKey);
                byte[] encryptedPrivateKey = CryptoHelper.EncryptFileEnhanced(privateKeyBytes, userPassword);
                File.WriteAllBytes(privateKeyPath, encryptedPrivateKey);

                // Save public key (not encrypted, but could be if needed)
                File.WriteAllText(publicKeyPath, keyPair.PublicKey);

                // Set file attributes
                try
                {
                    File.SetAttributes(privateKeyPath, FileAttributes.Hidden);
                }
                catch
                {
                    // Ignore permission errors
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to save user keys: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load user keys decrypted with password
        /// </summary>
        private static KeyPairInfo LoadUserKeys(string userPassword)
        {
            try
            {
                string keysDir = GetClientKeysDirectory();
                string privateKeyPath = Path.Combine(keysDir, USER_PRIVATE_KEY_FILE);
                string publicKeyPath = Path.Combine(keysDir, USER_PUBLIC_KEY_FILE);

                if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath))
                {
                    return null;
                }

                // Load public key
                string publicKey = File.ReadAllText(publicKeyPath);

                // Load and decrypt private key
                byte[] encryptedPrivateKey = File.ReadAllBytes(privateKeyPath);
                byte[] privateKeyBytes = CryptoHelper.DecryptFileEnhanced(encryptedPrivateKey, userPassword);
                string privateKey = Encoding.UTF8.GetString(privateKeyBytes);

                return new KeyPairInfo
                {
                    PublicKey = publicKey,
                    PrivateKey = privateKey,
                    Fingerprint = CalculatePublicKeyFingerprint(publicKey),
                    KeyId = "CLIENT_USER"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load user keys: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Verify key integrity
        /// </summary>
        private static bool VerifyKeyIntegrity(KeyPairInfo keyPair)
        {
            try
            {
                // Test sign and verify
                byte[] testData = Encoding.UTF8.GetBytes("KEY_INTEGRITY_TEST");
                var signature = SignData(testData, keyPair.PrivateKey, "TEST");
                bool isValid = VerifySignature(testData, signature, keyPair.PublicKey);

                return isValid;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate public key format (.NET Framework 4.8 compatible)
        /// </summary>
        private static bool IsValidPublicKey(string publicKeyXml)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(publicKeyXml);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get client keys directory
        /// </summary>
        private static string GetClientKeysDirectory()
        {
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appDataDir, "FileSharingClient", CLIENT_KEYS_DIR);
        }

        /// <summary>
        /// Export user public key for server registration
        /// </summary>
        public static string ExportUserPublicKeyForServer(string userPassword)
        {
            try
            {
                var userKeys = LoadUserKeys(userPassword);
                if (userKeys == null)
                    return null;

                var exportData = new
                {
                    PublicKey = userKeys.PublicKey,
                    Fingerprint = userKeys.Fingerprint,
                    KeyId = userKeys.KeyId,
                    CreatedAt = userKeys.CreatedAt,
                    KeySize = userKeys.KeySize
                };

                return JsonConvert.SerializeObject(exportData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to export user public key: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get signature statistics
        /// </summary>
        public static async Task<Dictionary<string, object>> GetSignatureStatisticsAsync(string userPassword)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var stats = new Dictionary<string, object>();

                    string keysDir = GetClientKeysDirectory();
                    stats["KeysDirectoryExists"] = Directory.Exists(keysDir);

                    var userKeys = LoadUserKeys(userPassword);
                    stats["UserKeysAvailable"] = userKeys != null;

                    if (userKeys != null)
                    {
                        stats["KeyFingerprint"] = userKeys.Fingerprint;
                        stats["KeyCreatedAt"] = userKeys.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                        stats["KeyIntegrityValid"] = VerifyKeyIntegrity(userKeys);
                    }

                    string serverKey = GetServerPublicKey();
                    stats["ServerPublicKeyAvailable"] = !string.IsNullOrEmpty(serverKey);

                    if (!string.IsNullOrEmpty(serverKey))
                    {
                        stats["ServerKeyFingerprint"] = CalculatePublicKeyFingerprint(serverKey);
                    }

                    return stats;
                });
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { ["Error"] = ex.Message };
            }
        }
    }
}
