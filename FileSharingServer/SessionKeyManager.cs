using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FileSharingServer
{
    public class SessionKeyManager
    {
        private static readonly Lazy<SessionKeyManager> _instance = new Lazy<SessionKeyManager>(() => new SessionKeyManager());
        public static SessionKeyManager Instance => _instance.Value;

        private readonly ConcurrentDictionary<string, SessionInfo> _activeSessions;
        private readonly Timer _cleanupTimer;
        private readonly object _lockObject = new object();

        // Configuration
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(2); // 2 hours session timeout
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(10); // Cleanup every 10 minutes
        private readonly int _maxSessionsPerUser = 5; // Maximum concurrent sessions per user

        private SessionKeyManager()
        {
            _activeSessions = new ConcurrentDictionary<string, SessionInfo>();
            
            // Setup cleanup timer
            _cleanupTimer = new Timer(_cleanupInterval.TotalMilliseconds);
            _cleanupTimer.Elapsed += CleanupExpiredSessions;
            _cleanupTimer.AutoReset = true;
            _cleanupTimer.Start();
        }

        /// <summary>
        /// Session information class
        /// </summary>
        public class SessionInfo
        {
            public string SessionId { get; set; }
            public int UserId { get; set; }
            public string Username { get; set; }
            public byte[] SessionKey { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastAccessedAt { get; set; }
            public string ClientIpAddress { get; set; }
            public bool IsActive { get; set; }
            public Dictionary<string, object> Metadata { get; set; }

            public SessionInfo()
            {
                Metadata = new Dictionary<string, object>();
                IsActive = true;
            }

            public bool IsExpired(TimeSpan timeout)
            {
                return DateTime.UtcNow - LastAccessedAt > timeout;
            }

            public void UpdateLastAccessed()
            {
                LastAccessedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Create a new secure session for a user
        /// </summary>
        public async Task<(string sessionId, byte[] sessionKey)> CreateSessionAsync(int userId, string username, string clientIpAddress = null)
        {
            try
            {
                // Check and cleanup existing sessions for this user if they exceed the limit
                await CleanupUserSessionsIfNeeded(userId);

                // Generate session ID and key
                string sessionId = GenerateSessionId();
                byte[] sessionKey = CryptoHelper.GenerateSessionKey();

                // Create session info
                var sessionInfo = new SessionInfo
                {
                    SessionId = sessionId,
                    UserId = userId,
                    Username = username,
                    SessionKey = sessionKey,
                    CreatedAt = DateTime.UtcNow,
                    LastAccessedAt = DateTime.UtcNow,
                    ClientIpAddress = clientIpAddress ?? "Unknown",
                    IsActive = true
                };

                // Store session
                _activeSessions.TryAdd(sessionId, sessionInfo);

                Console.WriteLine($"[SESSION] Created new session {sessionId} for user {username} (ID: {userId})");
                return (sessionId, sessionKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to create session for user {username}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Validate session and get session key
        /// </summary>
        public async Task<(bool isValid, byte[] sessionKey, SessionInfo sessionInfo)> ValidateSessionAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return (false, null, null);

            try
            {
                if (_activeSessions.TryGetValue(sessionId, out SessionInfo sessionInfo))
                {
                    // Check if session is expired
                    if (sessionInfo.IsExpired(_sessionTimeout) || !sessionInfo.IsActive)
                    {
                        await InvalidateSessionAsync(sessionId);
                        return (false, null, null);
                    }

                    // Update last accessed time
                    sessionInfo.UpdateLastAccessed();
                    return (true, sessionInfo.SessionKey, sessionInfo);
                }

                return (false, null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to validate session {sessionId}: {ex.Message}");
                return (false, null, null);
            }
        }

        /// <summary>
        /// Invalidate a specific session
        /// </summary>
        public async Task<bool> InvalidateSessionAsync(string sessionId)
        {
            try
            {
                return await Task.Run(() =>
                {
                    if (_activeSessions.TryRemove(sessionId, out SessionInfo sessionInfo))
                    {
                        sessionInfo.IsActive = false;
                        Console.WriteLine($"[SESSION] Invalidated session {sessionId} for user {sessionInfo.Username}");
                        return true;
                    }
                    return false;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to invalidate session {sessionId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Invalidate all sessions for a specific user
        /// </summary>
        public async Task<int> InvalidateUserSessionsAsync(int userId)
        {
            int invalidatedCount = 0;
            var sessionsToRemove = new List<string>();

            try
            {
                foreach (var kvp in _activeSessions)
                {
                    if (kvp.Value.UserId == userId)
                    {
                        sessionsToRemove.Add(kvp.Key);
                    }
                }

                foreach (string sessionId in sessionsToRemove)
                {
                    if (await InvalidateSessionAsync(sessionId))
                    {
                        invalidatedCount++;
                    }
                }

                Console.WriteLine($"[SESSION] Invalidated {invalidatedCount} sessions for user ID {userId}");
                return invalidatedCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to invalidate sessions for user {userId}: {ex.Message}");
                return invalidatedCount;
            }
        }

        /// <summary>
        /// Get session information
        /// </summary>
        public SessionInfo GetSessionInfo(string sessionId)
        {
            _activeSessions.TryGetValue(sessionId, out SessionInfo sessionInfo);
            return sessionInfo;
        }

        /// <summary>
        /// Get all active sessions for a user
        /// </summary>
        public List<SessionInfo> GetUserSessions(int userId)
        {
            var userSessions = new List<SessionInfo>();

            foreach (var kvp in _activeSessions)
            {
                if (kvp.Value.UserId == userId && kvp.Value.IsActive)
                {
                    userSessions.Add(kvp.Value);
                }
            }

            return userSessions;
        }

        /// <summary>
        /// Get total active session count
        /// </summary>
        public int GetActiveSessionCount()
        {
            return _activeSessions.Count;
        }

        /// <summary>
        /// Get active session count for a specific user
        /// </summary>
        public int GetUserSessionCount(int userId)
        {
            int count = 0;
            foreach (var kvp in _activeSessions)
            {
                if (kvp.Value.UserId == userId && kvp.Value.IsActive)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Encrypt data using session key
        /// </summary>
        public async Task<byte[]> EncryptWithSessionAsync(string sessionId, byte[] data, 
            CryptoHelper.CompressionLevel compression = CryptoHelper.CompressionLevel.Optimal)
        {
            var (isValid, sessionKey, _) = await ValidateSessionAsync(sessionId);
            if (!isValid)
                throw new UnauthorizedAccessException("Invalid or expired session");

            return CryptoHelper.EncryptWithSessionKey(data, sessionKey, compression);
        }

        /// <summary>
        /// Decrypt data using session key
        /// </summary>
        public async Task<byte[]> DecryptWithSessionAsync(string sessionId, byte[] encryptedData)
        {
            var (isValid, sessionKey, _) = await ValidateSessionAsync(sessionId);
            if (!isValid)
                throw new UnauthorizedAccessException("Invalid or expired session");

            return CryptoHelper.DecryptWithSessionKey(encryptedData, sessionKey);
        }

        /// <summary>
        /// Generate a cryptographically secure session ID
        /// </summary>
        private string GenerateSessionId()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] sessionBytes = new byte[32]; // 256 bits
                rng.GetBytes(sessionBytes);
                
                // Add timestamp to ensure uniqueness
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                byte[] timestampBytes = BitConverter.GetBytes(timestamp);
                
                byte[] combined = new byte[sessionBytes.Length + timestampBytes.Length];
                Buffer.BlockCopy(sessionBytes, 0, combined, 0, sessionBytes.Length);
                Buffer.BlockCopy(timestampBytes, 0, combined, sessionBytes.Length, timestampBytes.Length);
                
                // Hash the combined data to create session ID
                using (var sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(combined);
                    return Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_").TrimEnd('=');
                }
            }
        }

        /// <summary>
        /// Cleanup expired sessions
        /// </summary>
        private void CleanupExpiredSessions(object sender, ElapsedEventArgs e)
        {
            lock (_lockObject)
            {
                var expiredSessions = new List<string>();

                foreach (var kvp in _activeSessions)
                {
                    if (kvp.Value.IsExpired(_sessionTimeout) || !kvp.Value.IsActive)
                    {
                        expiredSessions.Add(kvp.Key);
                    }
                }

                int cleanedCount = 0;
                foreach (string sessionId in expiredSessions)
                {
                    if (_activeSessions.TryRemove(sessionId, out SessionInfo sessionInfo))
                    {
                        cleanedCount++;
                    }
                }

                if (cleanedCount > 0)
                {
                    Console.WriteLine($"[SESSION] Cleaned up {cleanedCount} expired sessions");
                }
            }
        }

        /// <summary>
        /// Cleanup user sessions if they exceed the maximum limit
        /// </summary>
        private async Task CleanupUserSessionsIfNeeded(int userId)
        {
            var userSessions = GetUserSessions(userId);
            
            if (userSessions.Count >= _maxSessionsPerUser)
            {
                // Sort by last accessed time (oldest first)
                userSessions.Sort((a, b) => a.LastAccessedAt.CompareTo(b.LastAccessedAt));
                
                // Remove oldest sessions to make room for new one
                int sessionsToRemove = userSessions.Count - _maxSessionsPerUser + 1;
                for (int i = 0; i < sessionsToRemove && i < userSessions.Count; i++)
                {
                    await InvalidateSessionAsync(userSessions[i].SessionId);
                }
            }
        }

        /// <summary>
        /// Update session metadata
        /// </summary>
        public bool UpdateSessionMetadata(string sessionId, string key, object value)
        {
            if (_activeSessions.TryGetValue(sessionId, out SessionInfo sessionInfo))
            {
                sessionInfo.Metadata[key] = value;
                sessionInfo.UpdateLastAccessed();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get session statistics
        /// </summary>
        public Dictionary<string, object> GetSessionStatistics()
        {
            var stats = new Dictionary<string, object>();
            var userSessionCounts = new Dictionary<int, int>();

            foreach (var kvp in _activeSessions)
            {
                if (kvp.Value.IsActive)
                {
                    int userId = kvp.Value.UserId;
                    if (userSessionCounts.ContainsKey(userId))
                        userSessionCounts[userId]++;
                    else
                        userSessionCounts[userId] = 1;
                }
            }

            stats["TotalActiveSessions"] = _activeSessions.Count;
            stats["UniqueActiveUsers"] = userSessionCounts.Count;
            stats["SessionTimeout"] = _sessionTimeout.ToString();
            stats["MaxSessionsPerUser"] = _maxSessionsPerUser;
            stats["CleanupInterval"] = _cleanupInterval.ToString();
            
            return stats;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _cleanupTimer?.Stop();
            _cleanupTimer?.Dispose();
            _activeSessions?.Clear();
        }
    }
}
