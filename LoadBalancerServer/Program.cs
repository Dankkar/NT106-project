using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LoadBalancerServer
{
    class Program
    {
        // Port on which the load-balancer itself listens (keep identical to the current client target so no client changes are needed)
        private const int LISTEN_PORT = 5000;

        // Configure backend FileSharingServer nodes here. Each server must run FileSharingServer on the given host:port.
        // NOTE: You can freely add/remove nodes; round-robin selection always works with the current list length.
        private static readonly List<IPEndPoint> _backends = new List<IPEndPoint>
        {
            new IPEndPoint(IPAddress.Loopback, 5100), // first backend
            new IPEndPoint(IPAddress.Loopback, 5101), // second backend
            // new IPEndPoint(IPAddress.Parse("192.168.1.100"), 5000), // another machine, for example
        };

        private static int _nextBackendIndex = 0; // round-robin pointer
        private static readonly object _lock = new object();

        static async Task Main(string[] args)
        {
            Console.Title = "FileSharing Load-Balancer";
            Console.WriteLine("Starting load-balancer on port " + LISTEN_PORT);
            TcpListener listener = new TcpListener(IPAddress.Any, LISTEN_PORT);
            listener.Start();

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client); // fire-and-forget
            }
        }

        // --- Core logic ------------------------------------------------------------
        private static IPEndPoint GetNextBackend()
        {
            lock (_lock)
            {
                var ep = _backends[_nextBackendIndex];
                _nextBackendIndex = (_nextBackendIndex + 1) % _backends.Count;
                return ep;
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            IPEndPoint backend = GetNextBackend();
            TcpClient server = new TcpClient();

            try
            {
                await server.ConnectAsync(backend.Address, backend.Port);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Could not connect to backend {backend}: {ex.Message}");
                client.Close();
                return;
            }

            Console.WriteLine($"Proxying {client.Client.RemoteEndPoint} -> {backend}");

            NetworkStream clientStream = client.GetStream();
            NetworkStream serverStream = server.GetStream();

            // Pump data bidirectionally until one side closes.
            Task t1 = PumpAsync(clientStream, serverStream);
            Task t2 = PumpAsync(serverStream, clientStream);

            await Task.WhenAny(t1, t2);

            client.Close();
            server.Close();
            Console.WriteLine($"Connection {client.Client.RemoteEndPoint} closed");
        }

        // Forward bytes from src to dst until EOF or exception.
        private static async Task PumpAsync(Stream src, Stream dst)
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
            catch
            {
                // swallow – either side closed or network error; outer method handles cleanup
            }
        }
    }
} 