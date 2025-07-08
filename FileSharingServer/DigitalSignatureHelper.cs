using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FileSharingServer
{
    public static class DigitalSignatureHelper
    {
        private const int RSA_KEY_SIZE = 2048; // RSA key size in bits
        private const string SIGNATURE_ALGORITHM = "SHA256withRSA";
        private const string SERVER_KEYS_DIR = "ServerKeys";
        private const string SERVER_PRIVATE_KEY_FILE = "server_private.key";
        private const string SERVER_PUBLIC_KEY_FILE = "server_public.key";
        private const string USER_KEYS_DIR = "UserKeys";

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
        /// Initialize server keys (create if not exists)
        /// </summary>
        public static async Task<bool> InitializeServerKeysAsync()
        {
            try
            {
                string keysDir = GetServerKeysDirectory();
                if (!Directory.Exists(keysDir))
                {
                    Directory.CreateDirectory(keysDir);
                }

                string privateKeyPath = Path.Combine(keysDir, SERVER_PRIVATE_KEY_FILE);
                string publicKeyPath = Path.Combine(keysDir, SERVER_PUBLIC_KEY_FILE);

                // Check if keys already exist
                if (File.Exists(privateKeyPath) && File.Exists(publicKeyPath))
                {
                    Console.WriteLine("[SIGNATURE] Server keys already exist");
                    return true;
                }

                // Generate new server keys
                var keyPair = GenerateKeyPair("SERVER");
                
                // Save keys to files (encrypted)
                await SaveKeyPairAsync(keyPair, privateKeyPath, publicKeyPath);
                
                Console.WriteLine("[SIGNATURE] Server keys generated and saved");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to initialize server keys: {ex.Message}");
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
        /// Sign file with server private key
        /// </summary>
        public static async Task<SignatureInfo> SignFileWithServerKeyAsync(byte[] fileData)
        {
            try
            {
                var serverKeys = await LoadServerKeysAsync();
                if (serverKeys == null)
                {
                    throw new InvalidOperationException("Server keys not available");
                }

                return SignData(fileData, serverKeys.PrivateKey, "SERVER");
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to sign file with server key: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verify file signature with server public key
        /// </summary>
        public static async Task<bool> VerifyFileWithServerKeyAsync(byte[] fileData, SignatureInfo signatureInfo)
        {
            try
            {
                var serverKeys = await LoadServerKeysAsync();
                if (serverKeys == null)
                {
                    return false;
                }

                return VerifySignature(fileData, signatureInfo, serverKeys.PublicKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to verify file with server key: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create signed file package (file + signature metadata)
        /// </summary>
        public static async Task<byte[]> CreateSignedFilePackageAsync(byte[] fileData, string signerId, string fileName)
        {
            try
            {
                return await Task.Run(async () =>
                {
                    // Sign the file
                    SignatureInfo signature;
                    if (signerId == "SERVER")
                    {
                        signature = await SignFileWithServerKeyAsync(fileData);
                    }
                    else
                    {
                        // For user signing, you'd need to pass the user's private key
                        throw new NotImplementedException("User signing not implemented in this method");
                    }

                    // Create package metadata
                    var package = new
                    {
                        FileName = fileName,
                        FileSize = fileData.Length,
                        FileHash = CryptoHelper.CalculateHash(fileData),
                        Signature = signature,
                        CreatedAt = DateTime.UtcNow
                    };

                    string packageJson = JsonConvert.SerializeObject(package);
                    byte[] packageBytes = Encoding.UTF8.GetBytes(packageJson);

                    // Combine: [PACKAGE_SIZE(4)] + [PACKAGE_JSON] + [FILE_DATA]
                    byte[] result = new byte[4 + packageBytes.Length + fileData.Length];
                    
                    Buffer.BlockCopy(BitConverter.GetBytes(packageBytes.Length), 0, result, 0, 4);
                    Buffer.BlockCopy(packageBytes, 0, result, 4, packageBytes.Length);
                    Buffer.BlockCopy(fileData, 0, result, 4 + packageBytes.Length, fileData.Length);

                    return result;
                });
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to create signed file package: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verify signed file package
        /// </summary>
        public static async Task<(bool isValid, string fileName, byte[] fileData)> VerifySignedFilePackageAsync(byte[] packageData)
        {
            try
            {
                return await Task.Run(async () =>
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

                    // Parse package using Newtonsoft.Json
                    var package = JsonConvert.DeserializeObject<dynamic>(packageJson);

                    // Extract file data
                    int fileSize = (int)package.FileSize;
                    byte[] fileData = new byte[fileSize];
                    Buffer.BlockCopy(packageData, 4 + packageSize, fileData, 0, fileSize);

                    // Verify file hash
                    string calculatedHash = CryptoHelper.CalculateHash(fileData);
                    string packageHash = package.FileHash;
                    
                    if (calculatedHash != packageHash)
                        return (false, null, null);

                    // Extract and verify signature
                    try
                    {
                        var signatureData = package.Signature;
                        var signatureInfo = JsonConvert.DeserializeObject<SignatureInfo>(signatureData.ToString());
                        
                        // Verify with server key
                        bool signatureValid = await VerifyFileWithServerKeyAsync(fileData, signatureInfo);
                        
                        string fileName = package.FileName;
                        
                        return (signatureValid, fileName, fileData);
                    }
                    catch
                    {
                        // If signature verification fails, return false
                        return (false, null, null);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to verify signed file package: {ex.Message}");
                return (false, null, null);
            }
        }

        /// <summary>
        /// Generate user key pair
        /// </summary>
        public static async Task<KeyPairInfo> GenerateUserKeyPairAsync(int userId, string username)
        {
            try
            {
                return await Task.Run(async () =>
                {
                    string keyId = $"USER_{userId}_{username}_{DateTime.UtcNow:yyyy-MM-dd}";
                    var keyPair = GenerateKeyPair(keyId);

                    // Create user keys directory
                    string userKeysDir = GetUserKeysDirectory(userId);
                    Directory.CreateDirectory(userKeysDir);

                    // Save key pair
                    string privateKeyPath = Path.Combine(userKeysDir, "private.key");
                    string publicKeyPath = Path.Combine(userKeysDir, "public.key");

                    await SaveKeyPairAsync(keyPair, privateKeyPath, publicKeyPath);

                    Console.WriteLine($"[INFO] Generated key pair for user {userId} ({username})");
                    return keyPair;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to generate user key pair for ID {userId}: {ex.Message}");
                throw new CryptographicException($"Failed to generate user key pair: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load user key pair
        /// </summary>
        public static async Task<KeyPairInfo> LoadUserKeysAsync(int userId)
        {
            try
            {
                string userKeysDir = GetUserKeysDirectory(userId);
                string privateKeyPath = Path.Combine(userKeysDir, "private.key");
                string publicKeyPath = Path.Combine(userKeysDir, "public.key");

                if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath))
                {
                    return null;
                }

                return await LoadKeyPairAsync(privateKeyPath, publicKeyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load user keys for ID {userId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get user public key
        /// </summary>
        public static async Task<string> GetUserPublicKeyAsync(int userId)
        {
            try
            {
                var userKeys = await LoadUserKeysAsync(userId);
                return userKeys?.PublicKey;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to get user public key for ID {userId}: {ex.Message}");
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
        /// Save key pair to files (.NET Framework 4.8 compatible)
        /// </summary>
        private static async Task SaveKeyPairAsync(KeyPairInfo keyPair, string privateKeyPath, string publicKeyPath)
        {
            try
            {
                await Task.Run(() =>
                {
                    // Encrypt private key before saving
                    string keyPassword = GenerateKeyPassword();
                    byte[] privateKeyBytes = Encoding.UTF8.GetBytes(keyPair.PrivateKey);
                    byte[] encryptedPrivateKey = CryptoHelper.EncryptFileEnhanced(privateKeyBytes, keyPassword);

                    // Save encrypted private key
                    File.WriteAllBytes(privateKeyPath, encryptedPrivateKey);
                    
                    // Save public key (not encrypted)
                    File.WriteAllText(publicKeyPath, keyPair.PublicKey);

                    // Save key password securely (in production, use proper key management)
                    string passwordFile = privateKeyPath + ".password";
                    File.WriteAllText(passwordFile, keyPassword);
                    
                    // Set file permissions (Windows)
                    try
                    {
                        File.SetAttributes(privateKeyPath, FileAttributes.Hidden);
                        File.SetAttributes(passwordFile, FileAttributes.Hidden);
                    }
                    catch
                    {
                        // Ignore permission errors
                    }
                });
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to save key pair: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load key pair from files (.NET Framework 4.8 compatible)
        /// </summary>
        private static async Task<KeyPairInfo> LoadKeyPairAsync(string privateKeyPath, string publicKeyPath)
        {
            try
            {
                return await Task.Run(() =>
                {
                    // Load public key
                    string publicKey = File.ReadAllText(publicKeyPath);

                    // Load and decrypt private key
                    string passwordFile = privateKeyPath + ".password";
                    string keyPassword = File.ReadAllText(passwordFile);
                    byte[] encryptedPrivateKey = File.ReadAllBytes(privateKeyPath);
                    byte[] privateKeyBytes = CryptoHelper.DecryptFileEnhanced(encryptedPrivateKey, keyPassword);
                    string privateKey = Encoding.UTF8.GetString(privateKeyBytes);

                    return new KeyPairInfo
                    {
                        PublicKey = publicKey,
                        PrivateKey = privateKey,
                        Fingerprint = CalculatePublicKeyFingerprint(publicKey)
                    };
                });
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to load key pair: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load server keys
        /// </summary>
        private static async Task<KeyPairInfo> LoadServerKeysAsync()
        {
            try
            {
                string keysDir = GetServerKeysDirectory();
                string privateKeyPath = Path.Combine(keysDir, SERVER_PRIVATE_KEY_FILE);
                string publicKeyPath = Path.Combine(keysDir, SERVER_PUBLIC_KEY_FILE);

                return await LoadKeyPairAsync(privateKeyPath, publicKeyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load server keys: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Generate secure password for key encryption
        /// </summary>
        private static string GenerateKeyPassword()
        {
            return CryptoHelper.GenerateSecurePassword(32, true);
        }

        /// <summary>
        /// Get server keys directory
        /// </summary>
        private static string GetServerKeysDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, SERVER_KEYS_DIR);
        }

        /// <summary>
        /// Get user keys directory
        /// </summary>
        private static string GetUserKeysDirectory(int userId)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, USER_KEYS_DIR, userId.ToString());
        }

        /// <summary>
        /// Get server public key for clients
        /// </summary>
        public static async Task<string> GetServerPublicKeyAsync()
        {
            try
            {
                var serverKeys = await LoadServerKeysAsync();
                return serverKeys?.PublicKey;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to get server public key: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Verify server key integrity
        /// </summary>
        public static async Task<bool> VerifyServerKeyIntegrityAsync()
        {
            try
            {
                var serverKeys = await LoadServerKeysAsync();
                if (serverKeys == null)
                    return false;

                // Test sign and verify with server keys
                byte[] testData = Encoding.UTF8.GetBytes("SIGNATURE_TEST_DATA");
                var signature = SignData(testData, serverKeys.PrivateKey, "SERVER");
                bool isValid = VerifySignature(testData, signature, serverKeys.PublicKey);

                return isValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Server key integrity check failed: {ex.Message}");
                return false;
            }
        }
    }
}
