# Sơ Đồ Luồng Nghiệp Vụ / Workflow Diagrams

**Ngày:** 2026-06-16 | **Dự án:** AVASecurity

---

## 1. Kiến Trúc Tổng Thể

```mermaid
graph TB
    subgraph Client["Desktop WPF .NET 9"]
        UI[SysAnti.UI]
        Core[SysAnti.Core]
        PM[PluginManager]
        UI --> Core
        Core --> PM
    end
    subgraph Plugins["Plugin DLLs"]
        GB[GameBooster]
        PM --> GB
    end
    subgraph Server["Backend"]
        Node[SysAnti.Server]
        MySQL[(MySQL)]
        Node --> MySQL
    end
    UI -->|REST JWT| Node
```

---

## 2. Luồng Khởi Động App

```mermaid
sequenceDiagram
    participant App
    participant Splash
    participant DB as SQLite
    participant License
    participant Main as Dashboard
    App->>Splash: Show
    App->>DB: EnsureCreated
    App->>License: ValidateLicense
    License-->>App: OK / Limited
    App->>Main: Show Dashboard
    App->>Splash: Close
```

---

## 3. Luồng Quét Virus

```mermaid
sequenceDiagram
    participant UI as VirusScanner
    participant Scan as FileScanner
    participant AI as AIDetection
    UI->>Scan: ScanDirectory
    Scan->>AI: AnalyzeSuspicious
    AI-->>UI: Threat list
```

---

## 4. Luồng Disk Cleanup

```mermaid
sequenceDiagram
    participant User
    participant UI as DiskCleanup
    participant Disk as DiskCleanerService
    User->>UI: Chọn category
    UI->>Disk: GetCleanupItems
    User->>UI: Clean Now
    UI->>Disk: PerformDeletion
    Disk-->>User: Freed space + achievement
```

---

## 5. Luồng Auth & License

```mermaid
flowchart TD
    A[Mở app] --> B{Đã login?}
    B -->|Không| C[LoginWindow]
    C --> D[Auth local hoặc web API]
    D --> E{License hợp lệ?}
    E -->|Ultra| F[Full features]
    E -->|Free/Expired| G[Limited + UpgradeWindow]
    F --> H[Dashboard]
    G --> H
```

---

## 6. Cloud Sync (Planned)

```mermaid
sequenceDiagram
    participant UI
    participant Sync as CloudSyncService
    participant API as Node Server
    UI->>Sync: SyncNow
    Sync->>API: push/pull settings
    Sync-->>UI: Last synced timestamp
```

---

## 7. Lộ Trình Nâng Cấp

```mermaid
gantt
    title Upgrade Timeline
    dateFormat YYYY-MM
    section Phase A
    Release Ready    :a1, 2026-07, 3M
    section Phase B
    Cloud Backend    :a2, 2026-10, 3M
    section Phase C
    Ecosystem        :a3, 2027-01, 6M
    section Phase D
    Headless Rust    :a4, 2027-07, 6M
    section Phase E
    NexGen 2028      :a5, 2028-01, 12M
```

Xem thêm: [02_UPGRADE_PLAN_2026_2028.md](./02_UPGRADE_PLAN_2026_2028.md)