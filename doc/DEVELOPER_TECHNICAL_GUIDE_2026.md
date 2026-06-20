# Developer Technical Guide / Hướng Dẫn Kỹ Thuật Cho Lập Trình Viên

> [!NOTE]
> This document outlines the architecture, setup, and coding standards for SysAnti.
> Tài liệu này phác thảo kiến trúc, thiết lập và tiêu chuẩn mã hóa cho SysAnti.

## 1. Tech Stack / Ngăn Xếp Công Nghệ

* **Frontend (Desktop)**: .NET 8, WPF (Windows Presentation Foundation).
* **Backend (API)**: Node.js (v18+), Express.js.
* **Database**: SQLite (Local embedded) & MongoDB (Cloud sync - planned).
* **AI/ML**: Python (TensorFlow/PyTorch) for local threat analysis.

---

## 2. Project Structure / Cấu Trúc Dự Án

```
f:\VStudio\SysAnti\
├── SysAnti.UI/             # Main WPF Application (Giao diện chính)
│   ├── Views/              # XAML Windows & Pages
│   ├── UserControls/       # Reusable UI components
│   └── Resources/          # Styles, Themes, Icons
├── SysAnti.Core/           # Business Logic & Models (Logic nghiệp vụ)
├── SysAnti.Server/         # Node.js Backend API
└── doc/                    # Documentation (Tài liệu)
```

---

## 3. Setup & Installation / Thiết Lập & Cài Đặt

### Prerequisites (Yêu cầu tiên quyết)

1. **Visual Studio 2022**: Workload ".NET Desktop Development".
2. **Node.js**: LTS version installed.
3. **Git**: For version control.

### Getting Started (Bắt đầu)

1. **Clone Repo**: `git clone <repo-url>`
2. **Restore NuGet**: Open `SysAnti.sln` -> Right click Solution -> "Restore NuGet Packages".
3. **Run Server**:

    ```bash
    cd SysAnti.Server
    npm install
    node server.js
    ```

4. **Run Client**: Set `SysAnti.UI` as Startup Project -> F5.

---

## 4. Coding Standards / Tiêu Chuẩn Mã Hóa

### C# (Back-end logic)

* **Naming**: PascalCase for Methods/Classes (`CalculateScore`), camelCase for local variables (`userScore`).
* **Async**: Always use `async/await` for I/O operations. Suffix async methods with `Async` (e.g., `LoadDataAsync`).
* **Comments**: Use XML documentation `///` for public APIs.

### XAML (UI)

* **Binding**: Use `Binding` with `UpdateSourceTrigger=PropertyChanged` for inputs.
* **Styles**: Do NOT hardcode colors used repeatedly. Use `StaticResource` from `App.xaml` or dictionaries.
* **Không hardcode màu sắc**: Sử dụng `StaticResource` từ `App.xaml` hoặc từ điển.

### Version Control (Git)

* **Commit Messages**: `[Category] Description` (e.g., `[UI] Fix login button alignment`).
* **Branching**: `feature/feature-name` or `fix/bug-name`.

---

## 5. Contribution / Đóng Góp

1. Create a branch from `develop`.
2. Implement feature/fix.
3. Run unit tests (if available).
4. Submit Pull Request (PR) for review.

> [!WARNING]
> **Security Note**: Never commit API keys or clear-text passwords. Use `.env` or User Secrets.
> **Lưu ý Bảo mật**: Không bao giờ commit API key hoặc mật khẩu dạng văn bản. Sử dụng `.env` hoặc User Secrets.
