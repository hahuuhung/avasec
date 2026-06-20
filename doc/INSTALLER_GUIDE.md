# Hướng Dẫn Tạo Bộ Cài Đặt (Installer) cho SysAnti

Tài liệu này hướng dẫn cách đóng gói ứng dụng SysAnti thành một file cài đặt (`.exe`) để triển khai trên các máy tính khác. Chúng ta sẽ sử dụng **Inno Setup**, một công cụ miễn phí và phổ biến để tạo bộ cài đặt.

## Mục Lục

1. [Chuẩn Bị](#1-chuẩn-bị)
2. [Bước 1: Publish Ứng Dụng](#bước-1-publish-ứng-dụng)
3. [Bước 2: Cấu Hình Script Inno Setup](#bước-2-cấu-hình-script-inno-setup)
4. [Bước 3: Biên Dịch Bộ Cài Đặt](#bước-3-biên-dịch-bộ-cài-đặt)
5. [Kiểm Thử](#kiểm-thử)

---

## 1. Chuẩn Bị

Trước khi bắt đầu, hãy đảm bảo bạn đã cài đặt các công cụ sau:

- **Inno Setup**: Tải và cài đặt phiên bản mới nhất từ [jrsoftware.org](https://jrsoftware.org/isdl.php). (Chọn bản Unicode).
- **.NET SDK**: Máy tính của bạn đã có, nhưng đảm bảo project build thành công.

## 2. Bước 1: Publish Ứng Dụng

Chúng ta cần tạo ra bản build "sạch" và đầy đủ của ứng dụng để đóng gói.

1. Mở Command Prompt (cmd) hoặc PowerShell tại thư mục gốc của dự án (`f:\VStudio\SysAnti`).
2. Chạy lệnh sau để publish ứng dụng (chế độ Release, cho Windows x64):

    ```powershell
    dotnet publish SysAnti.UI/SysAnti.UI.csproj -c Release -r win-x64 --self-contained false -o Publish/SysAntiApp
    ```

    *Giải thích:*
    - `-c Release`: Build ở chế độ tối ưu cho người dùng cuối.
    - `-r win-x64`: Nhắm mục tiêu Windows 64-bit.
    - `--self-contained false`: Yêu cầu máy người dùng cài .NET Runtime (giúp bộ cài nhẹ hơn). Nếu muốn đóng gói cả .NET Runtime vào, đổi thành `true`.
    - `-o Publish/SysAntiApp`: Xuất file ra thư mục `Publish/SysAntiApp`.

3. Kiểm tra thư mục `Publish/SysAntiApp`, bạn sẽ thấy file `SysAnti.UI.exe` và các file DLL khác.

## 3. Bước 2: Cấu Hình Script Inno Setup

Chúng tôi đã chuẩn bị sẵn một script mẫu tại `doc/resources/setup_script.iss`. Các thông tin chính trong script:

- **AppName**: Tên ứng dụng (SysAnti).
- **AppVersion**: Phiên bản (ví dụ: 1.0).
- **DefaultDirName**: Thư mục cài đặt mặc định (`{autopf}\SysAnti` -> C:\Program Files\SysAnti).
- **Files**: Copy toàn bộ nội dung từ thư mục `Publish/SysAntiApp` vào thư mục cài đặt của người dùng.
- **Icons**: Tạo shortcut trên Desktop và Start Menu.

**Lưu ý:** Nếu đường dẫn dự án của bạn thay đổi, bạn cần mở file `.iss` và sửa lại dòng `Source`. Mặc định script đang trỏ tới `..\..\Publish\SysAntiApp\*`.

## 4. Bước 3: Biên Dịch Bộ Cài Đặt

1. Mở file `doc/resources/setup_script.iss` bằng **Inno Setup Compiler**.
2. Nếu bạn chưa cài Inno Setup, hãy cài đặt nó ngay.
3. Trên thanh công cụ của Inno Setup, nhấn nút **Compile** (biểu tượng nút Play hoặc nhấn F9).
4. Chờ quá trình chạy xong.
5. Sau khi hoàn tất, file cài đặt `SysAnti_Setup.exe` sẽ được tạo ra trong thư mục `Output` (ngang hàng với file script) hoặc thư mục được chỉ định trong script.

## Kiểm Thử

1. Copy file `SysAnti_Setup.exe` sang máy tính khác hoặc máy ảo để test.
2. Chạy file setup.
3. Kiểm tra xem ứng dụng có được cài vào `C:\Program Files\SysAnti` không.
4. Chạy shortcut trên Desktop để đảm bảo ứng dụng khởi động thành công.

---
**Troubleshooting:**

- Nếu ứng dụng không chạy trên máy khách, hãy chắc chắn máy đó đã cài **.NET Desktop Runtime 9.0** (vì chúng ta publish với `--self-contained false`). Nếu muốn tiện lợi nhất, hãy publish lại với `--self-contained true` (bộ cài sẽ nặng hơn khoảng 60-100MB).
