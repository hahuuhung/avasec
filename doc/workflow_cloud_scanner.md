# Quy Trình Quét File Trên Đám Mây (Cloud Threat Scanner)

```mermaid
graph TD
    A[Người dùng / Hệ thống yêu cầu quét file] --> B{Hệ thống tính toán Hash: SHA-256}
    B --> C(Hash Cục Bộ / Local Memory Cache)
    
    C -->|Hash đã tồn tại?| D{Kiểm tra Database Local}
    D -- Có (Độc hại) --> E[Kết lụận: Malware / Xóa File]
    D -- Có (An toàn) --> F[Kết lụận: Clean / Cho qua]
    
    D -- Không có trong Cache --> G[Truy vấn VirusTotal API online]
    G -->|Giới hạn RateLimit API| H{Phản hồi tử Cloud}
    
    H -- Phát hiện Malware / Có điểm số độc hại --> I[Lưu Local Cache: MALICIOUS]
    I --> E
    
    H -- Không có rủi ro --> J[Lưu Local Cache: SAFE]
    J --> F
```

**Mục tiêu**:
Service `CloudThreatScanner` sẽ chịu trách nhiệm chính giao tiếp với API ngoài (VirusTotal).
Mọi file check thực tế đều do `MemoryCache` hoặc SQLite cache nội địa gánh tải để đảm bảo tránh tình trạng Rate-Limit của nhà cung cấp VirusTotal API.
Chỉ những mã Hash lạ (Unknown) mới phải gửi request HTTP đến Server Cloud.
