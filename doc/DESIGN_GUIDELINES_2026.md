# Design Guidelines & User Flow / Hướng Dẫn Thiết Kế & Luồng Người Dùng

> [!NOTE]
> This document serves as the visual bible for SysAnti.
> Tài liệu này đóng vai trò là kim chỉ nam về hình ảnh cho SysAnti.

## 1. Design Philosophy / Triết Lý Thiết Kế

**"Cyberpunk Professional"**

* **Vibe**: High-tech, Secure, Fast.
* **Keywords**: Dark Mode, Neon Accents, Glassmorphism, Clean Lines.
* **Cam Kết**: Không sử dụng màu sắc chung chung (đỏ thuần, xanh thuần). Sử dụng bảng màu tinh chỉnh.

---

## 2. Color Palette / Bảng Màu

### Primary Colors (Màu Chính)

| Token | Hex | Usage / Cách dùng |
| :--- | :--- | :--- |
| `PrimaryBlue` | `#007AFF` | Action buttons, active states (Nút hành động, trạng thái hoạt động). |
| `BackgroundDark` | `#121212` | Main window background (Nền cửa sổ chính). |
| `SurfaceDark` | `#1E1E1E` | Cards, panels (Thẻ, bảng). |

### Functional Colors (Màu Chức Năng)

| Token | Hex | Usage / Cách dùng |
| :--- | :--- | :--- |
| `SuccessGreen` | `#10B981` | Safe status, completion (Trạng thái an toàn, hoàn thành). |
| `WarningYellow` | `#F59E0B` | Alerts, attention needed (Cảnh báo, cần chú ý). |
| `DangerRed` | `#EF4444` | Threats, critical errors (Mối đe dọa, lỗi nghiêm trọng). |
| `TextWhite` | `#FFFFFF` | Primary headers (Tiêu đề chính). |
| `TextGray` | `#A1A1AA` | Body text, descriptions (Văn bản thân, mô tả). |

---

## 3. Typography / Kiểu Chữ

**Primary Font**: `Segoe UI` (System default for consistency) or `Inter` (if embedded).
**Phông chữ chính**: `Segoe UI` (Mặc định hệ thống để nhất quán) hoặc `Inter` (nếu nhúng).

* **H1 (Page Title)**: 24px, Bold.
* **H2 (Section Header)**: 20px, Semi-Bold.
* **Body**: 14px, Regular.
* **Button Text**: 14px, Medium, Uppercase (optional).

---

## 4. UI Components / Thành Phần Giao Diện

### Cards (Thẻ)

* **Style**: Rounded corners (`CornerRadius="10"`), subtle drop shadow.
* **Effect**: Slight hover lift (TranslateY -2px).
* **Nội dung**: Phải có icon minh họa và tiêu đề rõ ràng.

### Buttons (Nút)

* **Primary**: Solid Color (`PrimaryBlue`) + White Text + Shadow.
* **Secondary**: Border (`1px Solid Gray`) + Transparent Background.
* **Interaction**: Hover changes background brightness (+10%).

### Icons (Biểu Tượng)

* Use vector icons (SVG/Path geometry).
* **Style**: Line icons or Filled icons depending on context (Consistently).
* **Phong cách**: Icon dạng nét (Line) hoặc dạng khối (Filled) tùy ngữ cảnh (Phải nhất quán).

---

## 5. User Flows / Luồng Người Dùng

### A. First Run Experience (Trải nghiệm Lần đầu)

1. **Welcome Screen / Màn hình Chào**: Logo pulse animation -> "Get Started" button.
2. **Initial Scan / Quét Ban đầu**: Auto-scan system (> 10s) -> Show health score.
3. **Dashboard / Bảng điều khiển**: Land on main dashboard showing score and quick actions.
    * *Goal*: User feels secure immediately. (Mục tiêu: Người dùng cảm thấy an toàn ngay lập tức).

### B. Virus Scan (Quét Virus)

1. **Select Mode**: Quick Scan / Full Scan / Custom.
2. **Progress**: Circular progress bar + File count animation.
3. **Result**:
    * *Safe*: Green checkmark shield.
    * *Threats*: Red alert list with "Fix All" button.

### C. Optimization (Tối Ưu Hóa)

1. **One-Click Optimization**: "Boost Now" button on Dashboard.
2. **Animation**: Particles flying into center -> Score increases.
3. **Completion**: "System Optimized" toast notification.

---

> [!TIP]
> **Designer Note**: verify all mockups against these guidelines before handover to developers.
> **Lưu ý cho Thiết kế**: kiểm tra tất cả mockup dựa trên các hướng dẫn này trước khi bàn giao cho lập trình viên.
