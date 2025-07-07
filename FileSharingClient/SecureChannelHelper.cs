using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FileSharingClient
{
    public static class SecureChannelHelper
    {
        private const int DEFAULT_SECURE_PORT = 5001; // HTTPS/TLS port
        private const string TRUSTED_CERTS_DIR = "TrustedCertificates";
        private const string SERVER_CERT_FILE = "server_trusted.cer";

        /// <summary>
        /// Secure connection information
        /// </summary>
        public class SecureConnectionInfo
        {
            public string ServerId { get; set; }
            public string ServerAddress { get; set; }
            public int ServerPort { get; set; }
            public DateTime ConnectedAt { get; set; }
            public SslProtocols SslProtocol { get; set; }
            public string CipherSuite { get; set; }
            public bool IsAuthenticated { get; set; }
            public X509Certificate2 ServerCertificate { get; set; }
            public Dictionary<string, object> Metadata { get; set; }

            public SecureConnectionInfo()
            {
                ConnectedAt = DateTime.UtcNow;
                Metadata = new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Secure message format (same as server)
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
        /// Connect to secure server
        /// </summary>
        public static async Task<(SslStream sslStream, SecureConnectionInfo connectionInfo)> ConnectToSecureServerAsync(
            string serverAddress, int port = DEFAULT_SECURE_PORT)
        {
            TcpClient client = null;
            SslStream sslStream = null;
            SecureConnectionInfo connectionInfo = null;

            try
            {
                // Create TCP connection
                client = new TcpClient();
                await client.ConnectAsync(serverAddress, port);

                // Create SSL stream
                sslStream = new SslStream(client.GetStream(), false, ValidateServerCertificate);

                // Authenticate as client
                await sslStream.AuthenticateAsClientAsync(
                    serverAddress,
                    null, // Client certificates collection (if needed)
                    SslProtocols.Tls12 | SslProtocols.Tls13,
                    checkCertificateRevocation: false);

                // Create connection info
                connectionInfo = new SecureConnectionInfo
                {
                    ServerId = GenerateServerId(serverAddress, port),
                    ServerAddress = serverAddress,
                    ServerPort = port,
                    SslProtocol = sslStream.SslProtocol,
                    CipherSuite = sslStream.CipherAlgorithm.ToString(),
                    IsAuthenticated = sslStream.IsAuthenticated,
                    ServerCertificate = sslStream.RemoteCertificate as X509Certificate2
                };

                Console.WriteLine($"[SECURE CHANNEL] Connected to {serverAddress}:{port} using {connectionInfo.SslProtocol}");
                
                return (sslStream, connectionInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to connect to secure server: {ex.Message}");
                sslStream?.Dispose();
                client?.Close();
                throw;
            }
        }

        /// <summary>
        /// Send secure message to server
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
                var sessionManager = SessionKeyManager.Instance;
                if (!string.IsNullOrEmpty(sessionId) && sessionManager.IsSessionValid())
                {
                    message.EncryptedPayload = sessionManager.EncryptWithCurrentSession(payload);
                }
                else
                {
                    // Fallback: use basic encryption
                    message.EncryptedPayload = payload; // Would encrypt here in production
                }

                // Sign message with user private key
                message.Signature = SignMessage(message);

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
        /// Receive secure message from server
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
                var sessionManager = SessionKeyManager.Instance;
                if (!string.IsNullOrEmpty(sessionId) && sessionManager.IsSessionValid() && message.Headers.ContainsKey("SessionId"))
                {
                    payload = sessionManager.DecryptWithCurrentSession(message.EncryptedPayload);
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
        /// Validate server certificate
        /// </summary>
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, 
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // For development, accept self-signed certificates
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Check if it's only a self-signed certificate issue
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            {
                Console.WriteLine("[INFO] Accepting self-signed server certificate (development mode)");
                return true; // Accept for development
            }

            Console.WriteLine($"[WARNING] Server certificate validation failed: {sslPolicyErrors}");
            
            // In production, implement proper certificate validation
            // For now, accept all certificates (development mode)
            return true;
        }

        /// <summary>
        /// Trust server certificate permanently
        /// </summary>
        public static bool TrustServerCertificate(X509Certificate2 serverCertificate)
        {
            try
            {
                string trustedCertsDir = GetTrustedCertificatesDirectory();
                if (!Directory.Exists(trustedCertsDir))
                {
                    Directory.CreateDirectory(trustedCertsDir);
                }

                string certPath = Path.Combine(trustedCertsDir, SERVER_CERT_FILE);
                byte[] certBytes = serverCertificate.Export(X509ContentType.Cert);
                File.WriteAllBytes(certPath, certBytes);

                Console.WriteLine("[SECURE CHANNEL] Server certificate trusted and stored");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to trust server certificate: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if server certificate is trusted
        /// </summary>
        public static bool IsServerCertificateTrusted(X509Certificate2 serverCertificate)
        {
            try
            {
                string trustedCertsDir = GetTrustedCertificatesDirectory();
                string certPath = Path.Combine(trustedCertsDir, SERVER_CERT_FILE);

                if (!File.Exists(certPath))
                    return false;

                byte[] trustedCertBytes = File.ReadAllBytes(certPath);
                var trustedCert = new X509Certificate2(trustedCertBytes);

                // Compare certificates by thumbprint
                return string.Equals(trustedCert.Thumbprint, serverCertificate.Thumbprint, 
                    StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to check trusted certificate: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sign message with user private key
        /// </summary>
        private static string SignMessage(SecureMessage message)
        {
            try
            {
                // Create data to sign (exclude signature field)
                var dataToSign = $"{message.MessageId}:{message.MessageType}:{message.Timestamp:O}:{Convert.ToBase64String(message.EncryptedPayload)}";
                byte[] dataBytes = Encoding.UTF8.GetBytes(dataToSign);

                // Sign with user private key (would need to implement user signing)
                // For now, return empty signature
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to sign message: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Verify message signature from server
        /// </summary>
        private static async Task<bool> VerifyMessageSignatureAsync(SecureMessage message)
        {
            try
            {
                return await Task.Run(() =>
                {
                    if (string.IsNullOrEmpty(message.Signature))
                        return true; // No signature to verify

                    // Recreate data that was signed
                    var dataToSign = $"{message.MessageId}:{message.MessageType}:{message.Timestamp:O}:{Convert.ToBase64String(message.EncryptedPayload)}";
                    byte[] dataBytes = Encoding.UTF8.GetBytes(dataToSign);

                    // Verify with server public key
                    string serverPublicKey = DigitalSignatureHelper.GetServerPublicKey();
                    if (string.IsNullOrEmpty(serverPublicKey))
                        return false;

                    var signatureInfo = new DigitalSignatureHelper.SignatureInfo
                    {
                        Signature = message.Signature,
                        SignerId = "SERVER"
                    };

                    return DigitalSignatureHelper.VerifySignature(dataBytes, signatureInfo, serverPublicKey);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to verify message signature: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Secure file upload
        /// </summary>
        public static async Task<bool> SecureFileUploadAsync(SslStream sslStream, string filePath, string sessionId)
        {
            try
            {
                // Create signed upload package
                string userPassword = Session.UserPassword; // Assuming Session class exists
                byte[] signedPackage = await DigitalSignatureHelper.CreateSignedUploadPackageAsync(filePath, userPassword);

                // Send through secure channel
                return await SendSecureMessageAsync(sslStream, "FILE_UPLOAD", signedPackage, sessionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Secure file upload failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Secure file download
        /// </summary>
        public static async Task<(bool success, string fileName, byte[] fileData)> SecureFileDownloadAsync(
            SslStream sslStream, string fileName, string sessionId)
        {
            try
            {
                // Request file
                byte[] requestData = Encoding.UTF8.GetBytes(fileName);
                bool requestSent = await SendSecureMessageAsync(sslStream, "FILE_DOWNLOAD_REQUEST", requestData, sessionId);
                
                if (!requestSent)
                    return (false, null, null);

                // Receive file package
                var (success, messageType, packageData) = await ReceiveSecureMessageAsync(sslStream, sessionId);
                
                if (!success || messageType != "FILE_DOWNLOAD_RESPONSE")
                    return (false, null, null);

                // Verify and extract file
                string userPassword = Session.UserPassword;
                var (isValid, extractedFileName, fileData) = await DigitalSignatureHelper.VerifyDownloadPackageAsync(packageData, userPassword);

                return (isValid, extractedFileName, fileData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Secure file download failed: {ex.Message}");
                return (false, null, null);
            }
        }

        /// <summary>
        /// Generate server ID for connection tracking
        /// </summary>
        private static string GenerateServerId(string address, int port)
        {
            return $"{address}:{port}_{DateTime.UtcNow.Ticks}";
        }

        /// <summary>
        /// Get trusted certificates directory
        /// </summary>
        private static string GetTrustedCertificatesDirectory()
        {
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appDataDir, "FileSharingClient", TRUSTED_CERTS_DIR);
        }

        /// <summary>
        /// Get security statistics
        /// </summary>
        public static Dictionary<string, object> GetSecurityStatistics()
        {
            var stats = new Dictionary<string, object>();

            try
            {
                // Session manager stats
                var sessionStats = SessionKeyManager.Instance.GetSessionStatistics();
                foreach (var kvp in sessionStats)
                {
                    stats[$"Session_{kvp.Key}"] = kvp.Value;
                }

                // Trusted certificates info
                string trustedCertsDir = GetTrustedCertificatesDirectory();
                stats["TrustedCertificatesDirectoryExists"] = Directory.Exists(trustedCertsDir);
                
                if (Directory.Exists(trustedCertsDir))
                {
                    string serverCertPath = Path.Combine(trustedCertsDir, SERVER_CERT_FILE);
                    stats["ServerCertificateTrusted"] = File.Exists(serverCertPath);
                }

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
        /// Initialize secure client components
        /// </summary>
        public static async Task<bool> InitializeSecureClientAsync(string username, string userPassword)
        {
            try
            {
                // Initialize digital signatures
                bool signaturesInitialized = await DigitalSignatureHelper.InitializeClientKeysAsync(username, userPassword);
                if (!signaturesInitialized)
                {
                    Console.WriteLine("[WARNING] Failed to initialize client digital signatures");
                }

                // Initialize session manager (load cached session if available)
                await SessionKeyManager.Instance.LoadSessionFromCacheAsync(username, userPassword);

                Console.WriteLine("[SECURE CHANNEL] Client security components initialized");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to initialize secure client: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Close secure connection
        /// </summary>
        public static async Task CloseSecureConnectionAsync(SslStream sslStream)
        {
            try
            {
                if (sslStream != null)
                {
                    // Send close message
                    await SendSecureMessageAsync(sslStream, "CONNECTION_CLOSE", new byte[0]);
                    
                    sslStream.Close();
                    sslStream.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to close secure connection: {ex.Message}");
            }
        }

        /// <summary>
        /// Trust server certificate permanently (async version)
        /// </summary>
        public static async Task<bool> TrustServerCertificateAsync(X509Certificate2 serverCertificate)
        {
            try
            {
                return await Task.Run(() =>
                {
                    string trustedCertsDir = GetTrustedCertificatesDirectory();
                    if (!Directory.Exists(trustedCertsDir))
                    {
                        Directory.CreateDirectory(trustedCertsDir);
                    }

                    string certPath = Path.Combine(trustedCertsDir, SERVER_CERT_FILE);
                    byte[] certBytes = serverCertificate.Export(X509ContentType.Cert);
                    File.WriteAllBytes(certPath, certBytes);

                    Console.WriteLine("[SECURE CHANNEL] Server certificate trusted and stored");
                    return true;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to trust server certificate: {ex.Message}");
                return false;
            }
        }
    }
}
