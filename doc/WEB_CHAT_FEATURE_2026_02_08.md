
# Hướng dẫn Tính năng Web Chat (Guest Chat)

## Tổng quan
Tính năng Web Chat cho phép khách truy cập (người dùng chưa đăng nhập hoặc đã đăng nhập) trò chuyện trực tiếp với Admin thông qua một widget nổi trên giao diện web.

## Cơ chế hoạt động
1.  **Định danh (Fingerprinting)**:
    *   Hệ thống sử dụng `localStorage` để lưu trữ một `Session ID` duy nhất cho mỗi trình duyệt.
    *   ID này có định dạng: `web_[random_string]_[timestamp]`.
    *   Điều này đảm bảo khi người dùng tải lại trang hoặc quay lại sau này, lịch sử chat của họ vẫn được giữ nguyên (trên cùng một trình duyệt).

2.  **Giao diện (Frontend Widget)**:
    *   File: `public/chat-widget.js`.
    *   Được nhúng vào `index.html` và `dashboard.html`.
    *   Hiển thị nút chat tròn (Floating Action Button) ở góc dưới bên phải.
    *   Có thông báo (badge) khi có tin nhắn mới từ Admin.

3.  **Giao tiếp (API)**:
    *   **Gửi tin nhắn**: `POST /api/chat/send`.
    *   **Nhận tin nhắn**: Sử dụng kỹ thuật **Polling** (liên tục kiểm tra sau mỗi 3 giây) qua API `GET /api/chat/poll/:sessionId`.
    *   **Lịch sử**: Tải lại toàn bộ lịch sử khi mở widget qua `GET /api/chat/history/:sessionId`.

4.  **Phía Admin**:
    *   Admin truy cập trang Dashboard `/admin.html`.
    *   Vào mục "Hỗ trợ khách hàng / Support".
    *   Sẽ thấy các phiên chat bắt đầu bằng `web_...` với tên "Guest Web User".
    *   Admin có thể trả lời trực tiếp, tin nhắn sẽ xuất hiện trên Web Widget của khách.

## Cài đặt & Triển khai
*   Đảm bảo `server.js` đang chạy.
*   Đảm bảo file `chat-widget.js` có trong thư mục `public`.
*   Các trang muốn hiện chat phải có thẻ `<script src="chat-widget.js"></script>` ở cuối `<body>`.

## Lưu ý
*   Hiện tại sử dụng Polling nên sẽ có độ trễ nhỏ (tối đa 3 giây) khi nhận tin nhắn từ Admin.
*   Nếu người dùng xóa Cache/LocalStorage, họ sẽ mất lịch sử chat cũ và tạo ra một phiên chat mới.
