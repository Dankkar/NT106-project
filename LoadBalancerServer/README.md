# FileSharing Load Balancer

This small console application acts as a **TCP reverse-proxy** sitting in front of one or many `FileSharingServer` instances.  
It applies a **round-robin** algorithm to distribute incoming client connections evenly across the configured backend nodes.

## 1. Build / Compile

Nếu bạn dùng **Visual Studio 2019/2022** (hoặc MSBuild 16+), chỉ cần mở solution/ folder và **Build**.  
Project `LoadBalancerServer` đã được retarget sang **.NET Framework 4.8** (`net48`) nên **không yêu cầu .NET 6 SDK**.

Nếu thích dùng dòng lệnh, chạy:
```powershell
msbuild LoadBalancerServer\LoadBalancerServer.csproj /p:Configuration=Release
```

## 2. Run backend servers
Start as many `FileSharingServer` instances as you like. Each one needs an exclusive port. Example:
```bash
# Terminal 1 – backend #1 on port 5100
dotnet run --project FileSharingServer -- 5100

# Terminal 2 – backend #2 on port 5101
dotnet run --project FileSharingServer -- 5101
```

> The server code was updated so the first CLI argument is interpreted as the listening port (default `5000`).

## 3. Run the load balancer
```bash
# Terminal 3 – load-balancer on canonical port 5000
dotnet run --project LoadBalancerServer
```

The load balancer listens on port `5000`, exactly where the client code already connects, so **no change on the client side is necessary**.

## 4. Configure backend list
Edit `Program.cs` in `LoadBalancerServer` and adjust the `_backends` list:
```csharp
private static readonly List<IPEndPoint> _backends = new()
{
    new("192.168.1.10", 5000), // physical machine A
    new("192.168.1.11", 5000), // physical machine B
    // …
};
```
Add or remove entries as required. The round-robin index automatically adapts.

## 5. Health-checks / advanced routing
For simplicity, the implementation doesn't include health-checking or latency-based routing.  
Those can be added by:
1. Keeping a `ConcurrentDictionary<IPEndPoint, bool>` with node health status.
2. Periodically testing reachability with `TcpClient.ConnectAsync`.
3. Skipping unhealthy nodes in `GetNextBackend()`. 