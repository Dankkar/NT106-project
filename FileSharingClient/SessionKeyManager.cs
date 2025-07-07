using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FileSharingClient
{
    public class SessionKeyManager
    {
        private static readonly Lazy<SessionKeyManager> _instance = new Lazy<SessionKeyManager>(() => new SessionKeyManager());
        public static SessionKeyManager Instance => _instance.Value;

        private string _currentSessionId;
        private byte[] _currentSessionKey;
        private DateTime _sessionCreatedAt;
        private string _currentUsername;
        private readonly object _lockObject = new object();

        // Configuration
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(2);
        private readonly string _sessionCacheFile;

        private SessionKeyManager()
        {
            // Create cache file path in temp directory
            string tempDir = Path.GetTempPath();
            _sessionCacheFile = Path.Combine(tempDir, "FileSharingClient_Session.cache");
        }

        /// <summary>
        /// Client session information
        /// </summary>
        public class ClientSessionInfo
        {
            public string SessionId { get; set; }
            public string EncryptedSessionKey { get; set; } // Encrypted with user password
            public DateTime CreatedAt { get; set; }
            public string Username { get; set; }
            public Dictionary<string, object> Metadata { get; set; }

            public ClientSessionInfo()
            {
                Metadata = new Dictionary<string, object>();
            }

            public bool IsExpired(TimeSpan timeout)
            {
                return DateTime.UtcNow - CreatedAt > timeout;
            }
        }

        /// <summary>
        /// Create new session (called after successful login)
        /// </summary>
        public async Task<bool> CreateSessionAsync(string sessionId, byte[] sessionKey, string username)
        {
            try
            {
                lock (_lockObject)
                {
                    _currentSessionId = sessionId;
                    _currentSessionKey = sessionKey;
                    _sessionCreatedAt = DateTime.UtcNow;
                    _currentUsername = username;
                }

                // Optionally save to cache (encrypted with user password)
                if (!string.IsNullOrEmpty(Session.UserPassword))
                {
                    await SaveSessionToCacheAsync(username);
                }

                Console.WriteLine($"[CLIENT SESSION] Created session {sessionId} for user {username}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to create client session: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load session from cache (called during app startup)
        /// </summary>
        public async Task<bool> LoadSessionFromCacheAsync(string username, string userPassword)
        {
            try
            {
                if (!File.Exists(_sessionCacheFile))
                    return false;

                string encryptedData = File.ReadAllText(_sessionCacheFile);
                if (string.IsNullOrEmpty(encryptedData))
                    return false;

                // Decrypt session data
                byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
                byte[] decryptedBytes = CryptoHelper.DecryptFileEnhanced(encryptedBytes, userPassword);
                string jsonData = Encoding.UTF8.GetString(decryptedBytes);

                var sessionInfo = JsonConvert.DeserializeObject<ClientSessionInfo>(jsonData);
                
                // Check if session is expired
                if (sessionInfo.IsExpired(_sessionTimeout) || sessionInfo.Username != username)
                {
                    await ClearSessionCacheAsync();
                    return false;
                }

                // Decrypt session key
                byte[] sessionKeyBytes = Convert.FromBase64String(sessionInfo.EncryptedSessionKey);
                byte[] sessionKey = CryptoHelper.DecryptFileEnhanced(sessionKeyBytes, userPassword);

                lock (_lockObject)
                {
                    _currentSessionId = sessionInfo.SessionId;
                    _currentSessionKey = sessionKey;
                    _sessionCreatedAt = sessionInfo.CreatedAt;
                    _currentUsername = sessionInfo.Username;
                }

                Console.WriteLine($"[CLIENT SESSION] Loaded cached session for user {username}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load session from cache: {ex.Message}");
                await ClearSessionCacheAsync();
                return false;
            }
        }

        /// <summary>
        /// Save current session to cache (async version)
        /// </summary>
        private async Task SaveSessionToCacheAsync(string username)
        {
            await Task.Run(() => SaveSessionToCache(username));
        }

        /// <summary>
        /// Save current session to cache
        /// </summary>
        private void SaveSessionToCache(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(_currentSessionId) || _currentSessionKey == null)
                    return;

                // Encrypt session key with user password
                byte[] encryptedSessionKey = CryptoHelper.EncryptFileEnhanced(_currentSessionKey, Session.UserPassword);

                var sessionInfo = new ClientSessionInfo
                {
                    SessionId = _currentSessionId,
                    EncryptedSessionKey = Convert.ToBase64String(encryptedSessionKey),
                    CreatedAt = _sessionCreatedAt,
                    Username = username
                };

                string jsonData = JsonConvert.SerializeObject(sessionInfo);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);
                byte[] encryptedData = CryptoHelper.EncryptFileEnhanced(jsonBytes, Session.UserPassword);
                string base64Data = Convert.ToBase64String(encryptedData);

                File.WriteAllText(_sessionCacheFile, base64Data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to save session to cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear session cache (async version)
        /// </summary>
        public async Task ClearSessionCacheAsync()
        {
            await Task.Run(() => ClearSessionCache());
        }

        /// <summary>
        /// Clear session cache
        /// </summary>
        public void ClearSessionCache()
        {
            try
            {
                if (File.Exists(_sessionCacheFile))
                {
                    File.Delete(_sessionCacheFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to clear session cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear current session
        /// </summary>
        public async Task ClearSessionAsync()
        {
            lock (_lockObject)
            {
                _currentSessionId = null;
                _currentSessionKey = null;
                _sessionCreatedAt = DateTime.MinValue;
                _currentUsername = null;
            }

            await ClearSessionCacheAsync();
            Console.WriteLine("[CLIENT SESSION] Session cleared");
        }

        /// <summary>
        /// Check if current session is valid
        /// </summary>
        public bool IsSessionValid()
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(_currentSessionId) || _currentSessionKey == null)
                    return false;

                if (DateTime.UtcNow - _sessionCreatedAt > _sessionTimeout)
                {
                    // Session expired
                    Task.Run(async () => await ClearSessionAsync());
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Get current session ID
        /// </summary>
        public string GetCurrentSessionId()
        {
            lock (_lockObject)
            {
                return _currentSessionId;
            }
        }

        /// <summary>
        /// Get current session key
        /// </summary>
        public byte[] GetCurrentSessionKey()
        {
            lock (_lockObject)
            {
                return _currentSessionKey?.Clone() as byte[];
            }
        }

        /// <summary>
        /// Get current username
        /// </summary>
        public string GetCurrentUsername()
        {
            lock (_lockObject)
            {
                return _currentUsername;
            }
        }

        /// <summary>
        /// Encrypt data with current session key
        /// </summary>
        public byte[] EncryptWithCurrentSession(byte[] data, 
            CryptoHelper.CompressionLevel compression = CryptoHelper.CompressionLevel.Optimal)
        {
            if (!IsSessionValid())
                throw new UnauthorizedAccessException("No valid session available");

            lock (_lockObject)
            {
                return CryptoHelper.EncryptWithSessionKey(data, _currentSessionKey, compression);
            }
        }

        /// <summary>
        /// Decrypt data with current session key
        /// </summary>
        public byte[] DecryptWithCurrentSession(byte[] encryptedData)
        {
            if (!IsSessionValid())
                throw new UnauthorizedAccessException("No valid session available");

            lock (_lockObject)
            {
                return CryptoHelper.DecryptWithSessionKey(encryptedData, _currentSessionKey);
            }
        }

        /// <summary>
        /// Generate session authentication header for HTTP requests
        /// </summary>
        public string GenerateAuthHeader()
        {
            if (!IsSessionValid())
                throw new UnauthorizedAccessException("No valid session available");

            lock (_lockObject)
            {
                // Create timestamp
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                
                // Create signature: HMAC-SHA256(sessionKey, sessionId + timestamp)
                string dataToSign = $"{_currentSessionId}:{timestamp}";
                byte[] dataBytes = Encoding.UTF8.GetBytes(dataToSign);
                
                using (var hmac = new HMACSHA256(_currentSessionKey))
                {
                    byte[] signature = hmac.ComputeHash(dataBytes);
                    string signatureBase64 = Convert.ToBase64String(signature);
                    
                    // Format: "SessionAuth {sessionId}:{timestamp}:{signature}"
                    return $"SessionAuth {_currentSessionId}:{timestamp}:{signatureBase64}";
                }
            }
        }

        /// <summary>
        /// Create secure request identifier for tracking
        /// </summary>
        public string CreateRequestId()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[16];
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
            }
        }

        /// <summary>
        /// Get session statistics
        /// </summary>
        public Dictionary<string, object> GetSessionStatistics()
        {
            lock (_lockObject)
            {
                var stats = new Dictionary<string, object>();
                
                stats["HasValidSession"] = IsSessionValid();
                stats["SessionId"] = _currentSessionId ?? "None";
                stats["Username"] = _currentUsername ?? "None";
                stats["CreatedAt"] = _sessionCreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC");
                
                if (IsSessionValid())
                {
                    TimeSpan remaining = _sessionTimeout - (DateTime.UtcNow - _sessionCreatedAt);
                    stats["TimeRemaining"] = remaining.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    stats["TimeRemaining"] = "Expired";
                }
                
                stats["SessionTimeout"] = _sessionTimeout.ToString();
                stats["CacheFileExists"] = File.Exists(_sessionCacheFile);
                
                return stats;
            }
        }

        /// <summary>
        /// Refresh session by extending its lifetime
        /// </summary>
        public async Task<bool> RefreshSessionAsync()
        {
            if (!IsSessionValid())
                return false;

            try
            {
                lock (_lockObject)
                {
                    _sessionCreatedAt = DateTime.UtcNow;
                }

                // Update cache with new timestamp
                if (!string.IsNullOrEmpty(Session.UserPassword))
                {
                    await SaveSessionToCacheAsync(_currentUsername);
                }

                Console.WriteLine($"[CLIENT SESSION] Session refreshed for user {_currentUsername}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to refresh session: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate session key strength
        /// </summary>
        public bool ValidateSessionKeyStrength()
        {
            lock (_lockObject)
            {
                if (_currentSessionKey == null || _currentSessionKey.Length < 32)
                    return false;

                // Check for entropy (not all zeros or repeating patterns)
                bool hasVariation = false;
                byte firstByte = _currentSessionKey[0];
                
                for (int i = 1; i < _currentSessionKey.Length; i++)
                {
                    if (_currentSessionKey[i] != firstByte)
                    {
                        hasVariation = true;
                        break;
                    }
                }

                return hasVariation;
            }
        }
    }
}
