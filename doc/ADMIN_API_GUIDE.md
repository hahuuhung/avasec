# Admin API Documentation

**Base URL**: `http://localhost:3000` (Development) / `http://<your-server-ip>:3000` (Production)

## Overview

Bộ API dành cho quản trị viên (Admin) để quản lý Người dùng và Giấy phép bản quyền (System Licenses).

---

## 1. User Management

### 1.1 List All Users

Lấy danh sách tất cả người dùng kèm thông tin bản quyền của họ.

- **Endpoint**: `GET /api/admin/users`
- **Response**:

    ```json
    {
        "success": true,
        "data": [
            {
                "userId": 1,
                "username": "admin",
                "email": "admin@avasec.app",
                "licenseType": "Premium",
                "licenseExpiry": "2027-01-01T00:00:00.000Z",
                "remainingDays": 360
            }
        ]
    }
    ```

---

## 2. License Management

### 2.1 Extend License (Gia hạn)

Gia hạn thêm số ngày sử dụng cho một người dùng cụ thể.

- **Endpoint**: `POST /api/admin/licenses/extend`
- **Body**:

    ```json
    {
        "userId": 1,
        "days": 30
    }
    ```

- **Response**:

    ```json
    {
        "success": true,
        "message": "License extended by 30 days"
    }
    ```

### 2.2 Change License Type (Đổi gói)

Nâng cấp hoặc hạ cấp gói bản quyền (Trial, Pro, Premium).
*Lưu ý: Hành động này sẽ reset ngày hết hạn theo quy tắc của gói mới.*

- **Endpoint**: `POST /api/admin/licenses/change-type`
- **Body**:

    ```json
    {
        "userId": 1,
        "licenseType": "Pro" 
    }
    ```

    *(Values: "Trial", "Pro", "Premium")*
- **Response**:

    ```json
    {
        "success": true,
        "message": "License changed to Pro"
    }
    ```

---

## 3. API Keys

### 3.1 List API Keys

Xem danh sách các API Key đang hoạt động trong hệ thống.

- **Endpoint**: `GET /api/apikeys`
- **Response**:

    ```json
    {
        "success": true,
        "data": [
            {
                "KeyId": 1,
                "Title": "Default Web Client",
                "ApiKey": "sk_live_...",
                "IsActive": 1
            }
        ]
    }
    ```

### 3.2 Create API Key

Tạo một API Key mới.

- **Endpoint**: `POST /api/apikeys`
- **Body**:

    ```json
    {
        "title": "Mobile App Client"
    }
    ```

- **Response**:

    ```json
    {
        "success": true,
        "message": "API Key generated",
        "apiKey": "sk_live_..."
    }
    ```
