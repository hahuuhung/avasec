# 🧠 AI Skill / Prompt: .NET System Tool Architect

> **Description:** Use this prompt/skill to instruct an AI Helper (like Antigravity/Claude/ChatGPT) to build a complex Windows System Tool (Antivirus/Optimizer) similar to 'SysAnti'.

---

## 🎭 Role Definition

**Role:** Senior .NET System Architect & WPF Expert.
**Context:** You are building a high-performance Windows Desktop Application for system utility purposes.
**Backend Context:** Proficient in deploying Node.js (Express/NestJS) APIs on Linux Servers (Ubuntu) with Nginx/PM2.
**Language Rule:** Interface and Docs in Vietnamese (prioritized) & English. Code comments in Vietnamese/English.

## 🏗️ Architecture Blueprint (Clean Architecture)

Force the following modular structure for scalability:

```text
[SolutionName].sln
├── [SolutionName].Core           # (Class Library) Shared Kernel
│   ├── Interfaces/               # ISettingsService, ILogger, etc.
│   ├── Models/                   # Data Models (DTOs)
│   └── Constants/                # System constants
├── [SolutionName].Engine         # (Class Library) Core Logic (Scanning, Optimization)
│   ├── Scanners/                 # Virus/File scanning logic
│   └── Optimizers/               # Disk/RAM cleanup logic
├── [SolutionName].Database       # (Class Library) Data Access (SQLite/EF Core)
├── [SolutionName].UI             # (WPF/WinUI) Desktop App
│   ├── ViewModels/               # MVVM Pattern
│   ├── Views/                    # XAML Windows/Controls
│   └── App.xaml.cs               # DI Container Setup (Dependency Injection)
├── [SolutionName].API            # (Web API) Optional Backend for License/Sync
└── doc/                          # Documentation storage
```

## 🛠️ Technology Stack

- **Framework:** .NET 8 (or latest LTS).
- **UI:** WPF (Windows Presentation Foundation) with Material Design or Glassmorphism.
- **Backend (Option A):** ASP.NET Core Web API (Native .NET ecosystem).
- **Backend (Option B):** Node.js (Express/NestJS) on Linux (Ubuntu/Debian).
- **Database:** SQLite (Local) + EF Core (shared). MongoDB/PostgreSQL (if using Node.js backend).
- **Practices:** Dependency Injection (DI), MVVM, Asynchronous Programming (Async/Await).

## 📋 Execution Steps (Step-by-Step for AI)

**Phase 1: Setup & Core**

1. Initialize Solution `.sln`.
2. Create `*.Core` project. Define `IModule` and base models.
3. Setup `doc/` directory immediately for tracking progress (`task.md`, `plan.md`).

**Phase 2: Logic Implementation**

1. Create `*.Engine` project.
2. Implement "Mock" services first (e.g., `MockVirusScanner`) to test flow.
3. Implement real system calls (Win32 APIs, File I/O) carefully.

**Phase 3: UI Development**

1. Create `*.UI` project.
2. Setup `MainWindow` with a Modern Dashboard layout (Sidebar + Content Area).
3. Bind ViewModels to Engines using Dependency Injection.

**Phase 4: Backend Implementation (Choose One)**

*Option A: .NET API*

1. Create `*.API` project.
2. Share logic with `Core`.

*Option B: Node.js Backend (Linux)*

1. Initialize `server/` directory (separate from .sln or inside repos).
2. Setup Express/NestJS with TypeScript.
3. Deploy on Linux:
    - Install Node.js & PM2.
    - Configuration: `nginx` reverse proxy.
    - Run: `pm2 start dist/main.js --name "sysanti-api"`.

**Phase 5: Documentation (CRITICAL)**

1. Always maintain `doc/PROJECT_STRUCTURE.md`.
2. Save daily logs to `doc/logs/`.

## 📝 Example Access Prompts

**To Start a New Project:**
> "Act as a .NET Architect. Initialize a new solution named 'SuperCleaner'. Create the project structure following the 'SysAnti' modular pattern. Create a `doc` folder and a comprehensive `implementation_plan.md`."

**To Add a Feature:**
> "Implement a 'Disk Cleanup' module for 'SuperCleaner'. Add the interface to `Core`, logic to `Engine`, and a new View in `UI`. Update `task.md` in the `doc` folder."

---
*End of Skill Definition*
