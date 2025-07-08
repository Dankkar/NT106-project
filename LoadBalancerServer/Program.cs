using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LoadBalancerServer
{
    // Backend server status tracking
    public class BackendServer
    {
        public IPEndPoint EndPoint { get; set; }
        public bool IsHealthy { get; set; } = false;
        public int ActiveConnections = 0; // Changed to field for Interlocked operations
        public DateTime LastHealthCheck { get; set; } = DateTime.MinValue;
        public int FailedChecks { get; set; } = 0;
        public double ResponseTime { get; set; } = 0; // milliseconds

        public override string ToString()
        {
            return $"{EndPoint} - Healthy: {IsHealthy}, Connections: {ActiveConnections}, ResponseTime: {ResponseTime:F1}ms";
        }
    }

    class Program
    {
        // Port on which the load-balancer itself listens
        private const int LISTEN_PORT = 5000;
        private const int HEALTH_CHECK_INTERVAL_MS = 3000; // 3 seconds for faster detection
        private const int MAX_FAILED_CHECKS = 2; // Reduced to 2 for faster failover
        private const string CERTIFICATE_PATH = "LoadBalancerServer/cert/lb_certificate.pfx";
        private const string CERTIFICATE_PASSWORD = "kha123456789";

        // Backend servers configuration
        private static readonly List<BackendServer> _backends = new List<BackendServer>
        {
            new BackendServer { EndPoint = new IPEndPoint(IPAddress.Loopback, 5100) },
            new BackendServer { EndPoint = new IPEndPoint(IPAddress.Loopback, 5101) },
            // Add more backends as needed
        };

        private static int _nextBackendIndex = 0;
        private static readonly object _lock = new object();
        private static readonly Timer _healthCheckTimer;

        static Program()
        {
            // Initialize health check timer
            _healthCheckTimer = new Timer(PerformHealthChecks, null, 
                TimeSpan.Zero, TimeSpan.FromMilliseconds(HEALTH_CHECK_INTERVAL_MS));
        }

        static async Task Main(string[] args)
        {
            Console.Title = "FileSharing Smart Load-Balancer";
            Console.WriteLine("Starting smart load-balancer on port " + LISTEN_PORT);
            Console.WriteLine($"Backend servers: {_backends.Count}");
            
            // Initial health check
            await CheckAllBackendsHealth();
            
            TcpListener listener = new TcpListener(IPAddress.Any, LISTEN_PORT);
            listener.Start();

            // Start status monitoring
            _ = Task.Run(StatusMonitorAsync);

            Console.WriteLine("Load balancer ready. Press Ctrl+C to stop.");

            while (true)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client); // fire-and-forget
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Accepting client: {ex.Message}");
                }
            }
        }

        // --- Health Check System ---------------------------------------------------
        private static async void PerformHealthChecks(object state)
        {
            await CheckAllBackendsHealth();
        }

        private static async Task CheckAllBackendsHealth()
        {
            var tasks = _backends.Select(CheckBackendHealth).ToArray();
            await Task.WhenAll(tasks);
            
            int healthyCount = _backends.Count(b => b.IsHealthy);
            if (healthyCount == 0)
            {
                Console.WriteLine("[CRITICAL] No healthy backends available!");
            }
        }

        private static async Task CheckBackendHealth(BackendServer backend)
        {
            var startTime = DateTime.UtcNow;
            bool wasHealthy = backend.IsHealthy;
            
            try
            {
                using (var client = new TcpClient())
                {
                    // Set timeout for health check - shorter timeout for faster detection
                    client.ReceiveTimeout = 2000; // 2 seconds
                    client.SendTimeout = 2000;   // 2 seconds
                    
                    // Use timeout for connection attempt
                    var connectTask = client.ConnectAsync(backend.EndPoint.Address, backend.EndPoint.Port);
                    if (await Task.WhenAny(connectTask, Task.Delay(2000)) != connectTask)
                    {
                        throw new TimeoutException("Connection timeout");
                    }
                    
                    using (var stream = client.GetStream())
                    using (var writer = new StreamWriter(stream) { AutoFlush = true })
                    using (var reader = new StreamReader(stream))
                    {
                        // Simple health check - try to get server status
                        await writer.WriteLineAsync("HEALTH_CHECK\n");
                        
                        // Wait for any response with timeout
                        var readTask = reader.ReadLineAsync();
                        if (await Task.WhenAny(readTask, Task.Delay(2000)) != readTask)
                        {
                            throw new TimeoutException("Response timeout");
                        }
                        
                        var response = await readTask;
                        
                        // Calculate response time
                        backend.ResponseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                        
                        // Mark as healthy if we got any response
                        backend.IsHealthy = true;
                        backend.FailedChecks = 0;
                        backend.LastHealthCheck = DateTime.UtcNow;
                        
                        if (!wasHealthy)
                        {
                            Console.WriteLine($"[RECOVERY] Backend {backend.EndPoint} is back online! (Response time: {backend.ResponseTime:F1}ms)");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                backend.FailedChecks++;
                backend.ResponseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                if (backend.FailedChecks >= MAX_FAILED_CHECKS && backend.IsHealthy)
                {
                    backend.IsHealthy = false;
                    Console.WriteLine($"[FAILURE] Backend {backend.EndPoint} marked as unhealthy after {backend.FailedChecks} failed checks. Error: {ex.Message}");
                }
                else if (!backend.IsHealthy)
                {
                    // Still down, don't spam logs unless it's been a while
                    if ((DateTime.UtcNow - backend.LastHealthCheck).TotalMinutes > 1)
                    {
                        Console.WriteLine($"[STATUS] Backend {backend.EndPoint} still unhealthy (down for {(DateTime.UtcNow - backend.LastHealthCheck).TotalMinutes:F1} minutes)");
                        backend.LastHealthCheck = DateTime.UtcNow; // Update to prevent spam
                    }
                }
                else
                {
                    Console.WriteLine($"[WARNING] Health check failed for {backend.EndPoint} (attempt {backend.FailedChecks}/{MAX_FAILED_CHECKS}): {ex.Message}");
                }
            }
        }

        // --- Smart Backend Selection -----------------------------------------------
        private static BackendServer GetNextBackend()
        {
            lock (_lock)
            {
                var healthyBackends = _backends.Where(b => b.IsHealthy).ToList();
                
                if (healthyBackends.Count == 0)
                {
                    Console.WriteLine("[ERROR] No healthy backends available for request!");
                    return null;
                }

                // Round robin among healthy backends
                var backend = healthyBackends[_nextBackendIndex % healthyBackends.Count];
                _nextBackendIndex = (_nextBackendIndex + 1) % healthyBackends.Count;
                
                return backend;
            }
        }

        // Alternative: Least connections algorithm
        private static BackendServer GetLeastConnectionsBackend()
        {
            lock (_lock)
            {
                var healthyBackends = _backends.Where(b => b.IsHealthy).ToList();
                
                if (healthyBackends.Count == 0)
                    return null;

                return healthyBackends.OrderBy(b => b.ActiveConnections)
                                    .ThenBy(b => b.ResponseTime)
                                    .First();
            }
        }

        // --- Connection Handling ---------------------------------------------------
        private static async Task HandleClientAsync(TcpClient client)
        {
            const int MAX_RETRY_ATTEMPTS = 3;
            BackendServer backend = null;
            TcpClient server = null;
            SslStream sslStream = null;
            NetworkStream clientStream = null;

            for (int attempt = 0; attempt < MAX_RETRY_ATTEMPTS; attempt++)
            {
                try
                {
                    backend = GetNextBackend();
                    
                    if (backend == null)
                    {
                        Console.WriteLine("[ERROR] No available backend for client request");
                        break;
                    }

                    server = new TcpClient();
                    
                    // Track connection
                    Interlocked.Increment(ref backend.ActiveConnections);

                    Console.WriteLine($"[DEBUG] Attempt {attempt + 1}/{MAX_RETRY_ATTEMPTS}: Connecting to backend {backend.EndPoint}...");
                    await server.ConnectAsync(backend.EndPoint.Address, backend.EndPoint.Port);
                    Console.WriteLine($"[DEBUG] Successfully connected to backend {backend.EndPoint}");
                    Console.WriteLine($"[PROXY] {client.Client.RemoteEndPoint} -> {backend.EndPoint} (Active: {backend.ActiveConnections})");

                    // TLS termination: wrap client stream with SslStream
                    clientStream = client.GetStream();
                    sslStream = new SslStream(clientStream, false);
                    
                    X509Certificate2 certificate = null;
                    try
                    {
                        Console.WriteLine($"[DEBUG] Loading certificate from: {CERTIFICATE_PATH}");
                        certificate = new X509Certificate2(CERTIFICATE_PATH, CERTIFICATE_PASSWORD);
                        Console.WriteLine($"[DEBUG] Certificate loaded successfully. Subject: {certificate.Subject}");
                        Console.WriteLine($"[DEBUG] Certificate valid from: {certificate.NotBefore} to: {certificate.NotAfter}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CERT ERROR] Failed to load certificate: {ex.Message}");
                        Console.WriteLine($"[CERT ERROR] Certificate path: {Path.GetFullPath(CERTIFICATE_PATH)}");
                        Console.WriteLine($"[CERT ERROR] File exists: {File.Exists(CERTIFICATE_PATH)}");
                        client.Close();
                        server.Close();
                        Interlocked.Decrement(ref backend.ActiveConnections);
                        return;
                    }
                    
                    try
                    {
                        Console.WriteLine($"[DEBUG] Starting TLS handshake with client...");
                        await Task.Factory.FromAsync(
                            (callback, state) => sslStream.BeginAuthenticateAsServer(certificate, false, false, callback, state),
                            sslStream.EndAuthenticateAsServer,
                            null);
                        Console.WriteLine($"[DEBUG] TLS handshake completed successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TLS ERROR] Handshake failed: {ex.Message}");
                        Console.WriteLine($"[TLS ERROR] Inner exception: {ex.InnerException?.Message}");
                        Console.WriteLine($"[TLS ERROR] Stack trace: {ex.StackTrace}");
                        client.Close();
                        server.Close();
                        Interlocked.Decrement(ref backend.ActiveConnections);
                        return;
                    }

                    NetworkStream serverStream = server.GetStream();

                    // Pump data bidirectionally until one side closes
                    Task t1 = PumpAsync(sslStream, serverStream, $"Client(TLS)->Server({backend.EndPoint})");
                    Task t2 = PumpAsync(serverStream, sslStream, $"Server({backend.EndPoint})->Client(TLS)");

                    await Task.WhenAny(t1, t2);

                    // Cleanup
                    client.Close();
                    server.Close();
                    Interlocked.Decrement(ref backend.ActiveConnections);
                    Console.WriteLine($"[CLOSED] Connection to {backend.EndPoint} closed (Active: {backend.ActiveConnections})");
                    return; // Success - exit the retry loop
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Attempt {attempt + 1}/{MAX_RETRY_ATTEMPTS} failed for backend {backend?.EndPoint}: {ex.Message}");
                    Console.WriteLine($"[DEBUG] Exception type: {ex.GetType().Name}");
                    
                    if (backend != null)
                    {
                        Console.WriteLine($"[DEBUG] Backend health status: {backend.IsHealthy}, Last check: {backend.LastHealthCheck}");
                        backend.FailedChecks++;
                        
                        // Mark as unhealthy immediately if connection fails
                        if (backend.FailedChecks >= 2)
                        {
                            backend.IsHealthy = false;
                            Console.WriteLine($"[IMMEDIATE FAILURE] Backend {backend.EndPoint} marked as unhealthy due to connection failure");
                        }
                        
                        Interlocked.Decrement(ref backend.ActiveConnections);
                    }

                    // Cleanup current attempt
                    try
                    {
                        server?.Close();
                        if (sslStream != null) sslStream.Dispose();
                        if (clientStream != null && attempt == MAX_RETRY_ATTEMPTS - 1) clientStream.Dispose();
                    }
                    catch { }

                    // If this was the last attempt, close client connection
                    if (attempt == MAX_RETRY_ATTEMPTS - 1)
                    {
                        Console.WriteLine($"[FINAL ERROR] All {MAX_RETRY_ATTEMPTS} attempts failed. Closing client connection.");
                        client.Close();
                        return;
                    }

                    // Wait a bit before retrying
                    await Task.Delay(100);
                }
            }
            
            // If we reach here, all attempts failed
            Console.WriteLine("[ERROR] All backend connection attempts failed");
            client.Close();
        }

        // --- Data Pumping -----------------------------------------------------------
        private static async Task PumpAsync(Stream src, Stream dst, string direction)
        {
            byte[] buffer = new byte[8192];
            try
            {
                while (true)
                {
                    int read = await src.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0) break; // EOF
                    await dst.WriteAsync(buffer, 0, read);
                }
            }
            catch (Exception ex)
            {
                // Connection closed or network error
                Console.WriteLine($"[PUMP] {direction} stream closed: {ex.Message}");
            }
        }

        // --- Status Monitoring ------------------------------------------------------
        private static async Task StatusMonitorAsync()
        {
            while (true)
            {
                await Task.Delay(30000); // Print status every 30 seconds
                
                Console.WriteLine("\n=== LOAD BALANCER STATUS ===");
                foreach (var backend in _backends)
                {
                    Console.WriteLine($"  {backend}");
                }
                
                int totalConnections = _backends.Sum(b => b.ActiveConnections);
                int healthyServers = _backends.Count(b => b.IsHealthy);
                
                Console.WriteLine($"  Total Active Connections: {totalConnections}");
                Console.WriteLine($"  Healthy Servers: {healthyServers}/{_backends.Count}");
                Console.WriteLine("=============================\n");
            }
        }
    }
} 