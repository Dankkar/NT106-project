# Load Balancer Server - Failover & High Availability

## 🔧 **Cải tiến mới (New Improvements)**

### ✅ **1. Smart Failover Logic**
- **Retry Mechanism**: Load balancer tự động thử kết nối đến server khác khi 1 server bị lỗi
- **Max Retry**: 3 lần thử với các backend server khác nhau
- **Immediate Failover**: Đánh dấu server là "unhealthy" ngay khi kết nối thất bại 2 lần

### ✅ **2. Faster Health Check System**
```csharp
// Cải tiến health check
private const int HEALTH_CHECK_INTERVAL_MS = 3000; // 3 giây (trước: 10 giây)
private const int MAX_FAILED_CHECKS = 2;           // 2 lần (trước: 3 lần)
```

**Health Check Features:**
- ⚡ **Faster Detection**: Kiểm tra mỗi 3 giây thay vì 10 giây
- 🎯 **Quicker Failover**: Chỉ cần 2 lần thất bại thay vì 3 lần
- ⏱️ **Timeout Protection**: Timeout 2 giây cho mỗi health check
- 📊 **Better Logging**: Hiển thị response time và trạng thái chi tiết

### ✅ **3. Client-Side Retry Logic**
```csharp
// ApiService với retry capability
private const int MAX_RETRY_ATTEMPTS = 3;
private const int RETRY_DELAY_MS = 500;

// Login process với retry
const int MAX_LOGIN_ATTEMPTS = 3;
```

**Client Resilience:**
- 🔄 **Auto Retry**: Tự động thử lại khi kết nối thất bại
- 📈 **Exponential Backoff**: Tăng dần thời gian đợi giữa các lần thử
- 🎯 **Smart Recovery**: Phân biệt lỗi mạng vs lỗi authentication

## 🏗️ **Kiến trúc hoạt động (How it works)**

### Khi 1 Backend Server bị tắt:

1. **Load Balancer Detection**:
   ```
   [3 giây] Health check phát hiện server down
   [2 lần thất bại] Đánh dấu server unhealthy
   [Ngay lập tức] Loại bỏ khỏi danh sách available servers
   ```

2. **Client Request Handling**:
   ```
   Client Request → Load Balancer → Backend 1 (Failed)
                                  ↓ Auto Retry
                                  → Backend 2 (Success)
   ```

3. **Smart Retry Process**:
   ```
   Attempt 1: Backend 1 → Connection Failed
   Attempt 2: Backend 2 → Success → Response to Client
   ```

## 📊 **Monitoring & Logging**

### Load Balancer Status
```
=== LOAD BALANCER STATUS ===
  127.0.0.1:5100 - Healthy: True, Connections: 3, ResponseTime: 15.2ms
  127.0.0.1:5101 - Healthy: False, Connections: 0, ResponseTime: 2000.0ms
  Total Active Connections: 3
  Healthy Servers: 1/2
=============================
```

### Error Recovery Logs
```
[FAILURE] Backend 127.0.0.1:5101 marked as unhealthy after 2 failed checks
[RETRY] Attempt 1/3 failed for backend 127.0.0.1:5101: Connection refused
[SUCCESS] Attempt 2/3 succeeded with backend 127.0.0.1:5100
[RECOVERY] Backend 127.0.0.1:5101 is back online! (Response time: 12.8ms)
```

## 🎯 **Kết quả (Results)**

### ✅ **Trước khi cải tiến**:
- ❌ Client không thể đăng nhập khi 1 server down
- ⏰ Phải đợi 30 giây (3 lần × 10 giây) để phát hiện server down
- 💥 Load balancer trả về lỗi ngay khi kết nối thất bại

### ✅ **Sau khi cải tiến**:
- ✅ Client vẫn đăng nhập được dù 1 server down
- ⚡ Phát hiện server down trong 6 giây (2 lần × 3 giây)
- 🔄 Load balancer tự động thử server khác (failover)
- 💪 Client có retry logic để xử lý lỗi mạng tạm thời

## 🚀 **Testing Failover**

1. **Start all servers**:
   ```bash
   start_servers.bat
   ```

2. **Test normal operation**:
   - Client có thể đăng nhập thành công
   - Các requests được phân phối đều

3. **Simulate server failure**:
   - Tắt 1 backend server (port 5100 hoặc 5101)
   - Client vẫn có thể đăng nhập và sử dụng app
   - Check load balancer logs để xem failover process

4. **Recovery test**:
   - Bật lại server đã tắt
   - Load balancer sẽ tự động phát hiện và đưa server trở lại hoạt động

## 🔧 **Configuration Options**

```csharp
// LoadBalancerServer/Program.cs
private const int HEALTH_CHECK_INTERVAL_MS = 3000;  // Health check frequency
private const int MAX_FAILED_CHECKS = 2;            // Failures before marking unhealthy
private const int MAX_RETRY_ATTEMPTS = 3;           // Client-side retry attempts

// Client timeouts
client.ReceiveTimeout = 2000;  // 2 seconds
client.SendTimeout = 2000;     // 2 seconds
```

## 🎯 **Kết luận**

Với những cải tiến này, hệ thống giờ đây có khả năng **High Availability** thực sự:
- ✅ **Zero-downtime**: Client không bị gián đoạn khi 1 server down
- ⚡ **Fast Recovery**: Phát hiện và phục hồi nhanh chóng
- 🛡️ **Resilient**: Chống chịu lỗi mạng và server failures
- 📊 **Observable**: Logging chi tiết để monitoring và debugging