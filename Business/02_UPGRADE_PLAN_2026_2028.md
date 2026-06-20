# Kế Hoạch Nâng Cấp AVASecurity / Upgrade Plan 2026–2028

**Ngày:** 2026-06-16 | **Baseline:** [01_PROJECT_ANALYSIS_2026_06_16.md](./01_PROJECT_ANALYSIS_2026_06_16.md)

---

## 1. Mục Tiêu Chiến Lược

| Mục tiêu | Deadline |
|----------|----------|
| Production Release v2.0 (installer, CI) | Q3 2026 |
| Cloud sync + license online | Q4 2026 |
| Cloud Dashboard + Mobile beta | Q2 2027 |
| Headless API + Rust pilot | Q4 2027 |
| NexGen cross-platform (Tauri) | 2028 |

---

## 2. Phase A – Release Ready (Q3 2026)

### Sprint A1: UX (tuần 1–3)
- Adaptive Layout Dashboard (Compact/Normal/Large)
- Onboarding wizard lần đầu
- Accessibility: high contrast, font scale
- Enhanced Notification Center

### Sprint A2: Chất lượng (tuần 4–6)
- Fix 101 nullable warnings
- Đưa 5 plugins vào AVASecurity.sln
- Test coverage ≥ 40%
- Loại admin seed khỏi release build
- Wire navigation Benchmark / Privacy / GameMode

### Sprint A3: Release (tuần 7–10)
- Inno Setup / WiX installer
- Auto-update (UpdateCheckService)
- GitHub Actions CI/CD
- Changelog + hướng dẫn song ngữ
- Beta test 20–50 users

**Exit criteria:** Installer chạy trên Win10/11 VM sạch, CI green, coverage ≥ 40%.

---

## 3. Phase B – Cloud & Backend (Q4 2026)

- **Chọn Node.js làm backend chính** (ít migration nhất)
- Deprecate hoặc gộp SysAnti.API
- OpenAPI contract chuẩn
- **CloudSyncService:** theme, whitelist, scan profiles, license grace 7 ngày offline
- Landing page + pricing (Free / Ultra / Ecosystem / Lifetime)
- HTTPS production (Let's Encrypt)

---

## 4. Phase C – Ecosystem (2027)

| Sản phẩm | MVP |
|----------|-----|
| SysAnti Cloud | Quản lý 3 PC, lịch sử quét, remote scan |
| SysAnti Mobile | Cache clean, ảnh trùng, Wi-Fi scan (Android trước) |
| SysAnti VPN | WireGuard add-on (2028) |

---

## 5. Phase D – Headless + Rust (Q3–Q4 2027)

- Tách SysAnti.Core.Headless (REST/gRPC, không WPF)
- Pilot Rust rewrite DiskCleanerService (FFI từ C#)
- Target: 2x scan speed, -50% memory

---

## 6. Phase E – NexGen 2028

| | 2026 | 2028 |
|---|------|------|
| UI | WPF | Tauri + React |
| Core | C# | Rust |
| DB | SQLite | DuckDB |
| Size | ~150MB | < 15MB |
| AI | Heuristic | On-device LLM |

---

## 7. Quick Wins (tuần tới)

1. Wire sidebar navigation
2. Remove production admin seed
3. Add plugins to solution
4. Fix CS4014 async warnings

---

## 8. Rủi Ro

| Rủi ro | Giảm thiểu |
|--------|------------|
| WPF Mica thật không có | Simulated acrylic; plan Tauri 2028 |
| Backend migration | API v1/v2 song song 6 tháng |
| Scope creep Mobile | MVP only, không full parity |

Xem thêm: [03_BUSINESS_STRATEGY.md](./03_BUSINESS_STRATEGY.md) | [04_WORKFLOW_DIAGRAMS.md](./04_WORKFLOW_DIAGRAMS.md)