# Hướng Dẫn Đóng Gói và Triển Khai SysAnti (Miễn Phí) / SysAnti Packaging & Free Deployment Guide

Tài liệu này hướng dẫn cách đóng gói ứng dụng Desktop (File cài đặt `.exe`) và triển khai Web Server lên môi trường miễn phí.

---

## Phần 1: Đóng Gói Ứng Dụng Desktop (Tạo file Setup.exe)

Giả định ứng dụng chính là `SysAnti.UI` (WPF/WinForms).

### Cách 1: Sử dụng Visual Studio Setup Project (Khuyên dùng)

Đây là cách tạo file `.msi` hoặc `setup.exe` chuyên nghiệp.

1. **Cài đặt Extension**:
    * Mở Visual Studio.
    * Vào **Extensions** > **Manage Extensions**.
    * Tìm và cài đặt **Microsoft Visual Studio Installer Projects**.
    * Khởi động lại VS.

2. **Tạo Project Setup**:
    * Chuột phải vào **Solution 'SysAnti'** > **Add** > **New Project**.
    * Tìm **Setup Project**, đặt tên là `SysAntiSetup`.
    * Trong tab **File System** (hiện ra tự động), chuột phải vào **Application Folder** > **Add** > **Project Output**.
    * Chọn **SysAnti.UI** và chọn **Primary output**.

3. **Tạo Shortcut**:
    * Chuột phải vào `Primary output from SysAnti.UI` vừa thêm > **Create Shortcut...**.
    * Đặt tên shortcut (VD: `SysAnti Premium`).
    * Kéo shortcut này vào thư mục **User's Desktop** (để tạo icon ngoài màn hình) và **User's Programs Menu**.

4. **Thêm Icon**:
    * Chuột phải vào Project `SysAntiSetup` > **Properties**.
    * Tại mục **Icon**, chọn file `.ico` của bạn.

5. **Build**:
    * Chuột phải vào `SysAntiSetup` > **Build**.
    * Kết quả: File `setup.exe` và `SysAntiSetup.msi` sẽ nằm trong thư mục `Debug` hoặc `Release` của project setup.

---

### Cách 2: Sử dụng lệnh Publish (Đơn giản nhất)

Tạo một file `.exe` duy nhất chạy ngay (không cần cài đặt phức tạp).

1. Chuột phải vào project **SysAnti.UI** > **Publish**.
2. Chọn **Folder**.
3. Chọn **Show all settings**:
    * **Deployment mode**: Self-contained (Chạy không cần cài .NET tuỳ máy).
    * **Target Runtime**: win-x64 (hoặc x86).
    * **File publish options**: Chọn **Produce single file**.
4. Nhấn **Publish**. File `.exe` sẽ nằm trong thư mục `bin\Release\netX.X\publish`.

---

## Phần 2: Triển Khai Web Server (Node.js) Miễn Phí

`SysAnti.Server` sử dụng Node.js và MySQL. Chúng ta sẽ dùng **Render.com** (Web) và **Clever Cloud** hoặc **Aiven** (MySQL miễn phí).

### Bước 1: Chuẩn bị Database (MySQL)

Vì Render không miễn phí MySQL, ta dùng dịch vụ ngoài.
*Ví dụ dùng Aiven hoặc Clever Cloud (Free Tier).*

1. Đăng ký tài khoản tại [Console.aiven.io](https://console.aiven.io/) hoặc [Clever-cloud.com](https://www.clever-cloud.com/).
2. Tạo dịch vụ **MySQL**.
3. Lấy thông tin kết nối (Host, Port, User, Password, Database Name).
4. Dùng MySQL Workbench hoặc DBeaver kết nối vào và chạy script database của SysAnti (tạo bảng Users, Licenses...).

### Bước 2: Chuẩn bị Code (Github)

1. Đẩy code `SysAnti.Server` lên **GitHub**.
    * Đảm bảo có file `package.json`.
    * Tạo file `.gitignore` để KHÔNG up thư mục `node_modules` và file `.env`.

### Bước 3: Deploy lên Render.com

1. Đăng ký [Render.com](https://render.com/).
2. Chọn **New +** > **Web Service**.
3. Kết nối với GitHub repo của bạn.
4. Cấu hình:
    * **Name**: `avasec-server`
    * **Runtime**: **Node**
    * **Build Command**: `npm install`
    * **Start Command**: `node server.js`
    * **Instance Type**: Free
5. **Environment Variables** (Biến môi trường - *QUAN TRỌNG*):
    Thêm các biến giống trong file `.env` của bạn nhưng dùng thông tin Database thật ở Bước 1.
    * `DB_HOST`: (Host lấy từ Bước 1)
    * `DB_USER`: ...
    * `DB_PASS`: ...
    * `DB_NAME`: ...
    * `PORT`: `10000` (Render thường dùng cổng này, hoặc để mặc định).

6. Nhấn **Deploy**.
7. Sau khi xong, bạn sẽ có link: `https://avasec-server.onrender.com`.

### Bước 4: Cập Nhật App Desktop

* Vào project C# `SysAnti.UI` hoặc `SysAnti.Core`.
* Tìm chỗ gọi API (VD: `localhost:3000`).
* Đổi thành link mới: `https://avasec-server.onrender.com`.
* Build lại App Desktop (Phần 1).

---
**Lưu ý về gói Free:**

* **Render Free Tier**: Server sẽ "ngủ" (spin down) nếu không ai dùng trong 15 phút. Lần truy cập tiếp theo sẽ mất khoảng 30-50s để khởi động lại.
* **Database Free**: Thường giới hạn dung lượng và kết nối. Chỉ dùng cho demo/test.
