using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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
        private const int HEALTH_CHECK_INTERVAL_MS = 10000; // 10 seconds
        private const int MAX_FAILED_CHECKS = 3;

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
                    // Set timeout for health check
                    client.ReceiveTimeout = 5000;
                    client.SendTimeout = 5000;
                    
                    await client.ConnectAsync(backend.EndPoint.Address, backend.EndPoint.Port);
                    
                    using (var stream = client.GetStream())
                    using (var writer = new StreamWriter(stream) { AutoFlush = true })
                    using (var reader = new StreamReader(stream))
                    {
                        // Simple health check - try to get server status
                        await writer.WriteLineAsync("HEALTH_CHECK\n");
                        
                        // Wait for any response (even error is better than no response)
                        var response = await reader.ReadLineAsync();
                        
                        // Calculate response time
                        backend.ResponseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                        
                        // Mark as healthy if we got any response
                        backend.IsHealthy = true;
                        backend.FailedChecks = 0;
                        backend.LastHealthCheck = DateTime.UtcNow;
                        
                        if (!wasHealthy)
                        {
                            Console.WriteLine($"[RECOVERY] Backend {backend.EndPoint} is back online!");
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
                    // Still down, don't spam logs
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
            BackendServer backend = GetNextBackend(); // Or use GetLeastConnectionsBackend()
            
            if (backend == null)
            {
                Console.WriteLine("[ERROR] No available backend for client request");
                client.Close();
                return;
            }

            TcpClient server = new TcpClient();
            
            // Track connection
            Interlocked.Increment(ref backend.ActiveConnections);

            try
            {
                await server.ConnectAsync(backend.EndPoint.Address, backend.EndPoint.Port);
                Console.WriteLine($"[PROXY] {client.Client.RemoteEndPoint} -> {backend.EndPoint} (Active: {backend.ActiveConnections})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Could not connect to backend {backend.EndPoint}: {ex.Message}");
                
                // Mark backend as potentially unhealthy for immediate recheck
                backend.FailedChecks++;
                
                client.Close();
                Interlocked.Decrement(ref backend.ActiveConnections);
                return;
            }

            NetworkStream clientStream = client.GetStream();
            NetworkStream serverStream = server.GetStream();

            // Pump data bidirectionally until one side closes
            Task t1 = PumpAsync(clientStream, serverStream, $"Client->Server({backend.EndPoint})");
            Task t2 = PumpAsync(serverStream, clientStream, $"Server({backend.EndPoint})->Client");

            await Task.WhenAny(t1, t2);

            // Cleanup
            client.Close();
            server.Close();
            Interlocked.Decrement(ref backend.ActiveConnections);
            
            Console.WriteLine($"[CLOSED] Connection to {backend.EndPoint} closed (Active: {backend.ActiveConnections})");
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