# AVA Security Server (Node.js)

Đây là máy chủ trung tâm xử lý cả Giao diện Web (Frontend) và API Logic cho hệ thống SysAnti.

> [!IMPORTANT]
> Dự án này thay thế hoàn toàn cho `SysAnti.API` (C#) để tối ưu hóa việc chạy trên môi trường Linux hosting.

## Yêu cầu

- Node.js (v14 trở lên)
- MySQL

## Cài đặt

1. Cài đặt dependencies:

    ```bash
    npm install
    ```

2. Cấu hình Database:
    - Đổi tên file `.env.example` thành `.env` (đã có sẵn `.env`).
    - Cập nhật thông tin `DB_USER`, `DB_PASSWORD` trong `.env`.

3. Chạy server:
    - Development (tự động reload khi sửa code):

        ```bash
        npm run dev
        ```

    - Production:

        ```bash
        npm start
        ```

## Triển khai trên Linux (Ubuntu/CentOS)

1. **Chuẩn bị**:
    - Copy thư mục `SysAnti.Server` lên server.
    - Cài đặt Node.js: `curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash - && sudo apt-get install -y nodejs`

2. **Cài đặt & Chạy**:

    ```bash
    cd SysAnti.Server
    npm install
    ```

3. **Dùng PM2 để chạy nền**:
    Để server luôn chạy ngay cả khi tắt terminal hoặc khởi động lại máy:

    ```bash
    sudo npm install -g pm2
    pm2 start server.js --name "avasec-server"
    pm2 startup
    pm2 save
    ```

## Cấu trúc

- `server.js`: Entry point.
- `public/`: Chứa giao diện Web (Dashboard).
- `config/`: Cấu hình Database.
- `routes/`: Định nghĩa API (cần tạo thêm).
