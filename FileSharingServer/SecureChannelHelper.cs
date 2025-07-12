using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FileSharingServer
{
    public static class SecureChannelHelper
    {
        private const int DEFAULT_SECURE_PORT = 5001; // HTTPS/TLS port
        private const string CERTIFICATE_FILE = "server_certificate.pfx";
        private const string CERTIFICATE_PASSWORD = "FileSharingServer2024!";
        private const int CERTIFICATE_VALIDITY_DAYS = 365;

        /// <summary>
        /// Secure connection information
        /// </summary>
        public class SecureConnectionInfo
        {
            public string ClientId { get; set; }
            public IPEndPoint RemoteEndPoint { get; set; }
            public DateTime ConnectedAt { get; set; }
            public SslProtocols SslProtocol { get; set; }
            public string CipherSuite { get; set; }
            public bool IsAuthenticated { get; set; }
            public X509Certificate2 ClientCertificate { get; set; }
            public Dictionary<string, object> Metadata { get; set; }

            public SecureConnectionInfo()
            {
                ConnectedAt = DateTime.UtcNow;
                Metadata = new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Secure message format
        /// </summary>
        public class SecureMessage
        {
            public string MessageId { get; set; }
            public string MessageType { get; set; }
            public byte[] EncryptedPayload { get; set; }
            public string Signature { get; set; }
            public DateTime Timestamp { get; set; }
            public Dictionary<string, string> Headers { get; set; }

            public SecureMessage()
            {
                MessageId = Guid.NewGuid().ToString();
                Timestamp = DateTime.UtcNow;
                Headers = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Initialize secure server components
        /// </summary>
        public static async Task<bool> InitializeSecureServerAsync()
        {
            try
            {
                // Initialize server certificate
                var certificate = await GetOrCreateServerCertificateAsync();
                if (certificate == null)
                {
                    Console.WriteLine("[ERROR] Failed to initialize server certificate");
                    return false;
                }

                // Initialize digital signatures
                bool signaturesInitialized = await DigitalSignatureHelper.InitializeServerKeysAsync();
                if (!signaturesInitialized)
                {
                    Console.WriteLine("[ERROR] Failed to initialize server digital signatures");
                    return false;
                }

                Console.WriteLine("[SECURE CHANNEL] Server security components initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to initialize secure server: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create secure TCP listener
        /// </summary>
        public static async Task<TcpListener> CreateSecureTcpListenerAsync(int port = DEFAULT_SECURE_PORT)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();
                    
                    Console.WriteLine($"[SECURE CHANNEL] Secure TLS listener started on port {port}");
                    return listener;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to create secure TCP listener: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Handle secure client connection
        /// </summary>
        public static async Task<(SslStream sslStream, SecureConnectionInfo connectionInfo)> HandleSecureClientAsync(TcpClient client)
        {
            SslStream sslStream = null;
            SecureConnectionInfo connectionInfo = null;

            try
            {
                // Create SSL stream
                sslStream = new SslStream(client.GetStream(), false, ValidateClientCertificate);
                
                // Get server certificate
                var serverCertificate = await GetOrCreateServerCertificateAsync();
                if (serverCertificate == null)
                {
                    throw new InvalidOperationException("Server certificate not available");
                }

                // Authenticate as server
                await sslStream.AuthenticateAsServerAsync(
                    serverCertificate,
                    clientCertificateRequired: false, // Set to true if client certificates are required
                    SslProtocols.Tls12 | SslProtocols.Tls13,
                    checkCertificateRevocation: false);

                // Create connection info
                connectionInfo = new SecureConnectionInfo
                {
                    ClientId = GenerateClientId(),
                    RemoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint,
                    SslProtocol = sslStream.SslProtocol,
                    CipherSuite = sslStream.CipherAlgorithm.ToString(),
                    IsAuthenticated = sslStream.IsAuthenticated,
                    ClientCertificate = sslStream.RemoteCertificate as X509Certificate2
                };

                Console.WriteLine($"[SECURE CHANNEL] Secure connection established with {connectionInfo.RemoteEndPoint} using {connectionInfo.SslProtocol}");
                
                return (sslStream, connectionInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to establish secure connection: {ex.Message}");
                sslStream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Send secure message
        /// </summary>
        public static async Task<bool> SendSecureMessageAsync(SslStream sslStream, string messageType, byte[] payload, string sessionId = null)
        {
            try
            {
                // Create secure message
                var message = new SecureMessage
                {
                    MessageType = messageType
                };

                // Add session info if available
                if (!string.IsNullOrEmpty(sessionId))
                {
                    message.Headers["SessionId"] = sessionId;
                }

                // Encrypt payload with session key if available
                if (!string.IsNullOrEmpty(sessionId))
                {
                    var sessionManager = SessionKeyManager.Instance;
                    message.EncryptedPayload = await sessionManager.EncryptWithSessionAsync(sessionId, payload);
                }
                else
                {
                    // Fallback: encrypt with server key (less secure)
                    message.EncryptedPayload = payload; // Would encrypt here in production
                }

                // Sign message
                message.Signature = await SignMessageAsync(message);

                // Serialize and send (using Newtonsoft.Json for .NET Framework 4.8 compatibility)
                string messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageJson);
                
                // Send message length first
                byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
                await sslStream.WriteAsync(lengthBytes, 0, 4);
                
                // Send message
                await sslStream.WriteAsync(messageBytes, 0, messageBytes.Length);
                await sslStream.FlushAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send secure message: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Receive secure message
        /// </summary>
        public static async Task<(bool success, string messageType, byte[] payload)> ReceiveSecureMessageAsync(SslStream sslStream, string sessionId = null)
        {
            try
            {
                // Read message length
                byte[] lengthBytes = new byte[4];
                int bytesRead = await sslStream.ReadAsync(lengthBytes, 0, 4);
                if (bytesRead != 4)
                    return (false, null, null);

                int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                if (messageLength <= 0 || messageLength > 10 * 1024 * 1024) // 10MB limit
                    return (false, null, null);

                // Read message
                byte[] messageBytes = new byte[messageLength];
                int totalRead = 0;
                while (totalRead < messageLength)
                {
                    bytesRead = await sslStream.ReadAsync(messageBytes, totalRead, messageLength - totalRead);
                    if (bytesRead == 0)
                        return (false, null, null);
                    totalRead += bytesRead;
                }

                // Parse message (using Newtonsoft.Json for .NET Framework 4.8 compatibility)
                string messageJson = Encoding.UTF8.GetString(messageBytes);
                var message = Newtonsoft.Json.JsonConvert.DeserializeObject<SecureMessage>(messageJson);

                // Verify message signature
                if (!await VerifyMessageSignatureAsync(message))
                {
                    Console.WriteLine("[WARNING] Message signature verification failed");
                    return (false, null, null);
                }

                // Decrypt payload
                byte[] payload;
                if (!string.IsNullOrEmpty(sessionId) && message.Headers.ContainsKey("SessionId"))
                {
                    var sessionManager = SessionKeyManager.Instance;
                    payload = await sessionManager.DecryptWithSessionAsync(sessionId, message.EncryptedPayload);
                }
                else
                {
                    payload = message.EncryptedPayload; // Would decrypt here in production
                }

                return (true, message.MessageType, payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to receive secure message: {ex.Message}");
                return (false, null, null);
            }
        }

        /// <summary>
        /// Get or create server certificate
        /// </summary>
        public static async Task<X509Certificate2> GetOrCreateServerCertificateAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    string certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CERTIFICATE_FILE);

                    // Check if certificate exists and is valid
                    if (File.Exists(certPath))
                    {
                        try
                        {
                            var existingCert = new X509Certificate2(certPath, CERTIFICATE_PASSWORD);
                            if (existingCert.NotAfter > DateTime.Now.AddDays(30)) // Valid for at least 30 more days
                            {
                                return existingCert;
                            }
                        }
                        catch
                        {
                            // Certificate is invalid, create new one
                        }
                    }

                    // Create new self-signed certificate
                    var certificate = CreateSelfSignedCertificate();
                    
                    // Save certificate
                    byte[] certBytes = certificate.Export(X509ContentType.Pfx, CERTIFICATE_PASSWORD);
                    File.WriteAllBytes(certPath, certBytes);

                    Console.WriteLine("[SECURE CHANNEL] New server certificate created");
                    return certificate;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to get or create server certificate: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create self-signed certificate for server
        /// </summary>
        private static X509Certificate2 CreateSelfSignedCertificate()
        {
            try
            {
                using (var rsa = RSA.Create(2048))
                {
                    var request = new CertificateRequest(
                        "CN=FileSharingServer", 
                        rsa, 
                        HashAlgorithmName.SHA256, 
                        RSASignaturePadding.Pkcs1);

                    // Add extensions
                    request.CertificateExtensions.Add(
                        new X509BasicConstraintsExtension(false, false, 0, false));

                    request.CertificateExtensions.Add(
                        new X509KeyUsageExtension(
                            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, 
                            false));

                    request.CertificateExtensions.Add(
                        new X509EnhancedKeyUsageExtension(
                            new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, // Server Authentication
                            false));

                    // Create certificate
                    var certificate = request.CreateSelfSigned(
                        DateTime.Now.AddDays(-1), 
                        DateTime.Now.AddDays(CERTIFICATE_VALIDITY_DAYS));

                    return new X509Certificate2(certificate.Export(X509ContentType.Pfx, CERTIFICATE_PASSWORD), 
                        CERTIFICATE_PASSWORD, X509KeyStorageFlags.Exportable);
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to create self-signed certificate: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validate client certificate (if required)
        /// </summary>
        private static bool ValidateClientCertificate(object sender, X509Certificate certificate, 
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // For development, accept all certificates
            // In production, implement proper certificate validation
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine($"[WARNING] Client certificate validation failed: {sslPolicyErrors}");
            
            // Accept certificates with issues for now (development mode)
            return true;
        }

        /// <summary>
        /// Sign message for integrity
        /// </summary>
        private static async Task<string> SignMessageAsync(SecureMessage message)
        {
            try
            {
                // Create data to sign (exclude signature field)
                var dataToSign = $"{message.MessageId}:{message.MessageType}:{message.Timestamp:O}:{Convert.ToBase64String(message.EncryptedPayload)}";
                byte[] dataBytes = Encoding.UTF8.GetBytes(dataToSign);

                // Sign with server private key
                var signature = await DigitalSignatureHelper.SignFileWithServerKeyAsync(dataBytes);
                return signature.Signature;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to sign message: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Verify message signature
        /// </summary>
        private static async Task<bool> VerifyMessageSignatureAsync(SecureMessage message)
        {
            try
            {
                if (string.IsNullOrEmpty(message.Signature))
                    return false;

                // Recreate data that was signed
                var dataToSign = $"{message.MessageId}:{message.MessageType}:{message.Timestamp:O}:{Convert.ToBase64String(message.EncryptedPayload)}";
                byte[] dataBytes = Encoding.UTF8.GetBytes(dataToSign);

                // Create signature info
                var signatureInfo = new DigitalSignatureHelper.SignatureInfo
                {
                    Signature = message.Signature,
                    SignerId = "SERVER"
                };

                // Verify with server public key
                return await DigitalSignatureHelper.VerifyFileWithServerKeyAsync(dataBytes, signatureInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to verify message signature: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generate unique client ID
        /// </summary>
        private static string GenerateClientId()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[16];
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
            }
        }

        /// <summary>
        /// Encrypt file for secure transmission
        /// </summary>
        public static async Task<byte[]> EncryptForTransmissionAsync(byte[] data, string sessionId)
        {
            try
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    var sessionManager = SessionKeyManager.Instance;
                    return await sessionManager.EncryptWithSessionAsync(sessionId, data);
                }
                else
                {
                    // Fallback encryption (should use proper key management)
                    return await Task.Run(() => 
                        CryptoHelper.EncryptFileEnhanced(data, "DEFAULT_SERVER_KEY", 
                            CryptoHelper.CompressionLevel.Optimal));
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to encrypt for transmission: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Decrypt file from secure transmission
        /// </summary>
        public static async Task<byte[]> DecryptFromTransmissionAsync(byte[] encryptedData, string sessionId)
        {
            try
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    var sessionManager = SessionKeyManager.Instance;
                    return await sessionManager.DecryptWithSessionAsync(sessionId, encryptedData);
                }
                else
                {
                    // Fallback decryption
                    return await Task.Run(() => 
                        CryptoHelper.DecryptFileEnhanced(encryptedData, "DEFAULT_SERVER_KEY"));
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Failed to decrypt from transmission: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get security statistics
        /// </summary>
        public static Dictionary<string, object> GetSecurityStatistics()
        {
            var stats = new Dictionary<string, object>();

            try
            {
                // Certificate info
                var cert = GetOrCreateServerCertificateAsync().Result;
                if (cert != null)
                {
                    stats["CertificateSubject"] = cert.Subject;
                    stats["CertificateExpiry"] = cert.NotAfter.ToString("yyyy-MM-dd HH:mm:ss");
                    stats["CertificateValid"] = cert.NotAfter > DateTime.Now;
                }

                // Session manager stats
                var sessionStats = SessionKeyManager.Instance.GetSessionStatistics();
                foreach (var kvp in sessionStats)
                {
                    stats[$"Session_{kvp.Key}"] = kvp.Value;
                }

                // Server key integrity
                stats["ServerKeyIntegrityValid"] = DigitalSignatureHelper.VerifyServerKeyIntegrityAsync().Result;

                stats["SecureChannelEnabled"] = true;
                stats["TLSProtocolsSupported"] = "TLS 1.2, TLS 1.3";
                stats["EncryptionAlgorithm"] = "AES-256-CBC + HMAC-SHA256";
                stats["SignatureAlgorithm"] = "RSA-2048 + SHA256";

                return stats;
            }
            catch (Exception ex)
            {
                stats["Error"] = ex.Message;
                return stats;
            }
        }

        /// <summary>
        /// Create secure file transfer protocol
        /// </summary>
        public static async Task<bool> SecureFileTransferAsync(SslStream sslStream, string filePath, string sessionId)
        {
            try
            {
                // Read file asynchronously
                byte[] fileData = await Task.Run(() => File.ReadAllBytes(filePath));
                string fileName = Path.GetFileName(filePath);

                // Create signed package
                byte[] signedPackage = await DigitalSignatureHelper.CreateSignedFilePackageAsync(
                    fileData, "SERVER", fileName);

                // Send through secure channel
                return await SendSecureMessageAsync(sslStream, "FILE_TRANSFER", signedPackage, sessionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Secure file transfer failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Dispose secure resources
        /// </summary>
        public static void DisposeSecureResources()
        {
            try
            {
                SessionKeyManager.Instance.Dispose();
                Console.WriteLine("[SECURE CHANNEL] Security resources disposed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to dispose secure resources: {ex.Message}");
            }
        }
    }
}
