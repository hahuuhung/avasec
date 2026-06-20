# Báo Cáo Thiết Kế Giao Diện và Tối Ưu Hóa Luồng Hệ Thống (UI/UX Report)
**Dự Án**: SysAnti - Secure System Protection Platform
**Ngày**: 2026-02-08
**Phiên bản**: v3.1 (SysAntiUltra3)

## 1. Tổng Quan
Báo cáo này tóm tắt các cải tiến mới nhất về Giao diện Người dùng (UI) và Trải nghiệm Tương tác (UX) cho tính năng **Hỗ trợ Trực tuyến (Live Chat)** giữa Khách truy cập (Guest) và Quản trị viên (Admin). Mục tiêu chính là tạo ra một kênh giao tiếp mượt mà, chuyên nghiệp và ổn định.

## 2. Thiết Kế Giao Diện (UI Design)

### 2.1. Widget Chat (Guest Side)
- **Phong cách**: Hiện đại (Modern), Tối giản (Minimalist), Dark Mode native.
- **Màu sắc chủ đạo**:
  - **Nút mở Chat**: Gradient từ `#6366f1` (Indigo-500) đến `#a855f7` (Purple-500), tạo điểm nhấn nổi bật nhưng hài hòa trên nền tối.
  - **Cửa sổ Chat**: Màu nền `#0f172a` (Slate-900) với viền mỏng trong suốt (translucent border) tạo cảm giác chiều sâu.
  - **Bong bóng tin nhắn**:
    - **Guest**: Xanh dương `#3b82f6` (Blue-500) - màu tiêu chuẩn của hành động tích cực.
    - **Admin**: Xám đậm `#334155` (Slate-700) - màu trung tính, dễ đọc trên nền tối.
- **Hiệu ứng**:
  - **Hover**: Nút Chat phóng to nhẹ (scale 1.05) khi di chuột.
  - **Transition**: Cửa sổ Chat trượt lên mượt mà (translateY) khi mở.
  - **Shadow**: Đổ bóng mềm (soft shadow) giúp tách biệt widget khỏi nội dung trang web.

### 2.2. Admin Support Dashboard
- **Vị trí**: Tích hợp trực tiếp vào Sidebar "Hỗ trợ khách hàng / Support".
- **Bố cục**:
  - **Danh sách phiên (Session List)**: Hiển thị bên trái, sắp xếp theo thời gian hoạt động gần nhất. Hiển thị rõ trạng thái Đã đọc/Chưa đọc.
  - **Khu vực Chat (Chat View)**: Hiển thị bên phải, tự động cuộn xuống tin nhắn mới nhất.
- **Tương tác**: Click vào tên người dùng để tải toàn bộ lịch sử và bắt đầu trả lời ngay lập tức.

## 3. Tối Ưu Hóa Luồng Xử Lý (Workflow Updates)

### 3.1. Cơ Chế Chống Trùng Lặp Tin Nhắn (Message Deduplication)
- **Vấn đề cũ**: Tin nhắn đôi khi hiển thị 2 lần do xung đột giữa cập nhật giao diện tức thời (Optimistic UI) và phản hồi từ Server (Polling).
- **Giải pháp mới**:
  1. **Tạo ID Tạm thời**: Khi người dùng nhấn Gửi, UI tạo ngay một tin nhắn với ID ảo (VD: `temp-170732...`) và trạng thái "Đang gửi" (mờ đi).
  2. **Gửi API**: Gửi tin nhắn lên Server.
  3. **Thăng hạng (Promotion)**:
     - Khi Server trả về ID thật (`MessageID`), hệ thống tự động tìm tin nhắn có ID ảo tương ứng và cập nhật thành ID thật.
     - Nếu Polling chạy trước khi API trả về, hệ thống đối chiếu nội dung tin nhắn và thực hiện cập nhật tương tự.
  4. **Kết quả**: Đảm bảo tin nhắn chỉ xuất hiện 1 lần, trạng thái chuyển từ "Đang gửi" sang "Đã gửi" mượt mà.

### 3.2. Quy Trình Giao Tiếp (Communication Flow)
**Bước 1: Khởi tạo Phiên (Guest)**
- Hệ thống tự động kiểm tra `localStorage` để lấy `SessionID`.
- Nếu chưa có, tự động tạo mới (Browser Fingerprinting đơn giản).

**Bước 2: Gửi Tin (Guest -> Server)**
- API: `POST /api/chat/send`
- Payload: `SessionID`, `Message`, `IsAgent: false`.
- Server lưu vào MySQL, trả về `MessageID`.

**Bước 3: Nhận Tin (Admin -> Server -> Guest)**
- **Admin**: Gửi tin qua Admin Panel.
- **Guest**: Widget tự động Poll `GET /api/chat/poll` mỗi 3 giây.
- **Thông báo**: Nếu Chat đang đóng, hiển thị Badge số lượng tin nhắn chưa đọc màu đỏ trên nút Chat.

## 4. Công Nghệ & Tối Ưu Hiệu Năng
- **Polling thông minh**: Chỉ tải tin nhắn mới hơn `lastMessageId`, giảm tải băng thông và xử lý phía Client.
- **SQL Optimization**: Sử dụng `sql_mode=only_full_group_by` compliant queries để đảm bảo tính nhất quán dữ liệu khi Group By theo Session.
- **Vanilla JS**: Không phụ thuộc thư viện ngoài (jQuery, React, Vue) cho Widget, giúp script siêu nhẹ (<5KB) và dễ dàng nhúng vào mọi trang.

## 5. Kết Luận
Hệ thống Chat hiện tại đã đạt độ ổn định cao với trải nghiệm người dùng hiện đại. Các lỗi hiển thị đã được khắc phục hoàn toàn bằng logic deduplication chặt chẽ. Giao diện đồng bộ với thiết kế Dark Theme cao cấp của toàn bộ dự án SysAnti.
