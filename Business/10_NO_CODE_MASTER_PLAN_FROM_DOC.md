# Kế Hoạch Thực Hiện No-Code — Tổng Hợp Từ `doc/`

**Ngày:** 2026-06-17  
**Nguồn:** 118 file trong `doc/` + [README.md](../README.md) + [09_NO_CODE_PLAN_A_TO_Z.md](./09_NO_CODE_PLAN_A_TO_Z.md)  
**Cách làm:** Bạn đọc kế hoạch → copy **prompt session** vào Cursor → test tay → tick checklist. **Không tự viết code.**

---

## 1. Tóm tắt điều hành

### Sản phẩm (neo từ README + doc)

| Lớp | Vai trò | Doc tham chiếu |
|-----|---------|----------------|
| **Desktop WPF** `avasec.ui` | Quét, AI, dọn rác, Game Mode — **offline** | `IMPLEMENTATION_PLAN_2026`, `FEATURE_REQUIREMENTS` |
| **Portal Node** `avasec.server` | Login, license, store, admin | `ADMIN_API_GUIDE`, `DEPLOYMENT_GUIDE` |
| **Không làm v1** | Cloud sync, Flutter mobile, MAUI, Monitoring API | `HYBRID_*`, `MONITORING_ONLY_*` → **2027+** |

### Tiến độ tổng hợp (cập nhật từ doc + thực tế)

| Phase (IMPLEMENTATION_PLAN) | Doc ghi | Thực tế 2026-06 | Việc no-code còn lại |
|-----------------------------|---------|-----------------|----------------------|
| **1 Win11 Design** | 75% | ~85% | Adaptive layout 1.4 |
| **2 AI & Cloud** | 50% | AI ✅, Cloud **bỏ v1** | Chỉ license sync |
| **3 New Features** | 100% | ~90% | Wire nav sidebar |
| **4 UX Polish** | 0% | ~60% | Onboarding ✅, help, notif |
| **5 Release** | 0% | ~25% | Installer, deploy, beta |

**Mục tiêu ship:** v1.0 trong **8–10 tuần vibe** (1 session/ngày).

---

## 2. Bộ lọc chiến lược — Doc nào áp dụng, doc nào hoãn

Từ `doc/` có **3 hướng mâu thuẫn**. Kế hoạch no-code **chọn 1 hướng**:

```
✅ ÁP DỤNG NGAY (v1.0)
├── README.md — offline-first + portal lite ~$5/tháng
├── IMPLEMENTATION_PLAN_2026 — Phase 1,3,4,5 (bỏ 2.2 Cloud Sync)
├── FEATURE_REQUIREMENTS — Must-have ✅ giữ, Advanced ⏳ sau
├── DESIGN_GUIDELINES_2026 — vibe UI mọi session
├── DEPLOYMENT_GUIDE + INSTALLER_GUIDE — Phase 5
├── TESTING_GUIDE — checklist test tay
├── ADMIN_API_GUIDE — admin user/license
├── OPTIMIZATION_PLAN — chỉ khi app chậm (session riêng)
└── PROJECT_ISSUES_REPORT — fix duplicate HTML trước polish

⏸️ HOÃN (không prompt v1)
├── HYBRID_IMPLEMENTATION_PLAN — MAUI/Flutter/React PWA
├── MONITORING_ONLY_STRATEGY — mobile monitor 2027
├── IMPLEMENTATION_PROGRESS Monitoring API — ASP.NET monitoring
├── PROJECT_2028_PLAN / CROSS_PLATFORM — Tauri/Rust 2028
└── REMOTE_MANAGEMENT_STRATEGY — điều khiển từ xa
```

---

## 3. Ma trận nguồn doc → Phase thực hiện

| Phase | Tài liệu `doc/` chính | Kết quả người dùng thấy |
|-------|------------------------|-------------------------|
| **0 Nền** | `RUN_INSTRUCTIONS`, `RUN_GUIDE`, `LOGIN_FIX_GUIDE` | Một lệnh chạy app + portal |
| **1 Desktop core** | `IMPLEMENTATION_PLAN` 3.1–3.3, `FEATURE_REQUIREMENTS` §1–3 | Sidebar mở Benchmark/Privacy/Game/Disk |
| **2 Web & license** | `ADMIN_API_GUIDE`, `SEED_DATA_PLAN`, `WEB_*` | Đăng ký → Trial → store → admin |
| **3 UX** | `DESIGN_GUIDELINES`, `UI_*`, `LOGIN_REDESIGN`, `NOTIFICATION_*` | Onboarding, toast, song ngữ |
| **4 Chất lượng** | `PLAN_COMPLETION`, `OPTIMIZATION_PLAN`, `PROJECT_ISSUES` | Build 0 lỗi, HTML sạch |
| **5 Ship** | `INSTALLER_GUIDE`, `DEPLOYMENT_GUIDE`, `TESTING_GUIDE` | .exe installer + VPS HTTPS |

---

## 4. Design bible (dán vào mọi prompt)

Từ `DESIGN_GUIDELINES_2026.md` + `WEB_DESIGN_CONCEPT_2026.md`:

| Token | Giá trị | Dùng khi |
|-------|---------|----------|
| Nền | `#0f172a` / `#121212` | Desktop + web admin |
| Accent | `#06b6d4` cyan (web), `#007AFF` (desktop) | CTA, tab active |
| Success | `#10B981` | License OK, scan sạch |
| Warning | `#F59E0B` | Trial sắp hết |
| Danger | `#EF4444` | Threat, lỗi |
| Font | Inter / Segoe UI 14px | Body song ngữ |

**Vibe câu:** *Cyberpunk Professional — tối, neon nhẹ, glass, không popup quảng cáo.*

---

## 5. Kế hoạch 7 Phase — 45 session no-code

### PHASE 0 — Nền tảng (Tuần 1) · 5 session

| ID | Nguồn doc | Prompt một dòng | Done khi |
|----|-----------|-----------------|----------|
| 0.1 | `RUN_INSTRUCTIONS` | `scripts/start-all.ps1` check MySQL + portal + in 4 URL | Health UP |
| 0.2 | `PLAN_COMPLETION` | Fix XAML resources thiếu (`AccentWarningBrush`, `Lang.*`) nếu còn crash | App mở không XamlParseException |
| 0.3 | `LOGIN_FIX_GUIDE` | Portal login/register bcrypt, `avasec_user` localStorage | Đăng ký → dashboard |
| 0.4 | `SEED_DATA_PLAN` | `database.sql` seed admin + Trial; script seed 5 user test | Admin có license |
| 0.5 | `PROJECT_ISSUES` | Xóa duplicate Bootstrap/button trong `donate.html` | HTML validator sạch |

**Prompt mẫu 0.1:**
```text
Đọc doc/RUN_INSTRUCTIONS_2026_02_07.md và README.md.
Tạo scripts/start-all.ps1: kiểm tra MySQL, start avasec.server, in URL index/store/dashboard/admin.
Done: một lệnh → /api/health UP. Không cloud sync.
```

---

### PHASE 1 — Desktop: wire tính năng đã code (Tuần 2) · 8 session

Doc: `IMPLEMENTATION_PLAN` Task 1.4, 3.1–3.3, `UPGRADE_FEATURES_PLAN`, `functional_fix_plan`

| ID | Tính năng (doc) | Prompt | Done |
|----|-----------------|--------|------|
| 1.1 | Navigation | Wire sidebar → BenchmarkView | Click → view, không crash |
| 1.2 | Navigation | Wire → PrivacyGuardianView | Tab tracker/telemetry hiện |
| 1.3 | Navigation | Wire → GameModeView | Toggle Game Mode |
| 1.4 | Navigation | Wire → Disk Cleanup | Scan → clean → GB recovered |
| 1.5 | FEATURE_REQ §2 | RAM optimizer + process list | Số RAM đổi sau optimize |
| 1.6 | FEATURE_REQ §3 | Virus scan + quarantine | 1 click scan có kết quả |
| 1.7 | Task 1.4 | Adaptive layout Compact/Normal/Large | Resize cửa sổ không vỡ |
| 1.8 | `REALTIME_METRICS` | Dashboard CPU/RAM live | Số liệu cập nhật 5s |

---

### PHASE 2 — Portal & kinh doanh (Tuần 3) · 7 session

Doc: `ADMIN_API_GUIDE`, README luồng mua, `WEB_CHAT`, `donate.html`

| ID | Prompt | Done |
|----|--------|------|
| 2.1 | `store.html` 3 gói Trial/Ultra/Lifetime + VND | Khớp README pricing |
| 2.2 | Redeem key `AVA-...` API + UI | Key admin → user Ultra |
| 2.3 | `dashboard.html` license + expiry + link store | User thấy plan |
| 2.4 | `admin.html` tab Người dùng polish + secret URL | Quản lý user 🔑🎁🔒 |
| 2.5 | Admin create-keys UI (thay curl) | Tạo 5 key 1 click |
| 2.6 | Fix `ChatMessages` table + `chat-widget.js` | Không 500 poll |
| 2.7 | `donate.html` / Momo UI — confirm payment flow | 1 nút confirm, không duplicate |

---

### PHASE 3 — Desktop ↔ License (Tuần 4) · 6 session

Doc: README offline 7 ngày, `appsettings`

| ID | Prompt | Done |
|----|--------|------|
| 3.1 | `appsettings` ApiBaseUrl/PortalUrl/LoginUrl | App mở đúng URL dev |
| 3.2 | Login app ↔ portal license fetch | Plan khớp web |
| 3.3 | Upgrade window nhập key redeem | Key → Ultra trong app |
| 3.4 | License cache SQLite 7 ngày offline | Tắt WiFi vẫn Pro |
| 3.5 | Grace banner sau 7 ngày offline | Nhắc sync, không khóa đột ngột |
| 3.6 | Release build: bỏ DEBUG admin/admin | Chỉ portal auth production |

---

### PHASE 4 — UX Polish (Tuần 5) · 7 session

Doc: `IMPLEMENTATION_PLAN` Phase 4, `TOAST_NOTIFICATION`, `UI_PROFESSIONAL_*`, `ONBOARDING` (đã có → polish)

| ID | Task doc | Prompt | Done |
|----|----------|--------|------|
| 4.1 | 4.1 Onboarding | Polish wizard 3 bước: welcome → scan → login portal | First-run hoàn tất |
| 4.2 | 4.3 Accessibility | Font scale + high contrast settings | Đổi setting → UI đổi |
| 4.3 | 4.4 Notification | Local toast scan/cleanup xong | Toast hiện đúng |
| 4.4 | `NOTIFICATION_UI` | Web notification bell dashboard | Badge số thông báo |
| 4.5 | 4.2 Contextual help | Tooltip ? trên Dashboard cards | Hover có giải thích Vi/En |
| 4.6 | `LOGIN_REDESIGN` | Login overlay desktop khớp web vibe | Cùng brand AVA |
| 4.7 | Empty states | Chưa quét / chưa cleanup → CTA | Không màn trống |

---

### PHASE 5 — Chất lượng & tối ưu (Tuần 6) · 6 session

Doc: `OPTIMIZATION_PLAN`, `TESTING_GUIDE`, `IMPLEMENTATION_PLAN` nullable/tests

| ID | Prompt | Done |
|----|--------|------|
| 5.1 | Đưa 5 plugins vào `avasec.sln` | Build 0 errors |
| 5.2 | Nullable warnings batch Core/UI | Warnings giảm 50%+ |
| 5.3 | Unit test License + Auth + DiskCleaner | Coverage ≥ 40% |
| 5.4 | `PluginManager` parallel load (doc OPTIMIZATION) | Startup nhanh hơn |
| 5.5 | Chạy full `TESTING_GUIDE` test case A–D | Bảng pass/fail |
| 5.6 | `PROJECT_ISSUES` sweep toàn `public/` | 0 duplicate ID |

---

### PHASE 6 — Đóng gói (Tuần 7) · 4 session

Doc: `INSTALLER_GUIDE`, `DEPLOYMENT_AND_PACKAGING_GUIDE`, `resources/setup_script.iss`

| ID | Prompt | Done |
|----|--------|------|
| 6.1 | `scripts/publish.ps1` → `AVASecurity.exe` Release | Chạy được ngoài Visual Studio |
| 6.2 | Inno Setup `install/avasec-setup.iss` brand AVA | `AVA-Security-Setup.exe` |
| 6.3 | Test cài Win10/11 VM sạch | Cài → mở → gỡ sạch |
| 6.4 | `CHANGELOG.md` + version About dialog | User báo lỗi có version |

---

### PHASE 7 — Production & Beta (Tuần 8–10) · 6 session

Doc: `DEPLOYMENT_GUIDE_2026_02_08`, `TESTING_GUIDE`, README $5 VPS

| ID | Prompt | Done |
|----|--------|------|
| 7.1 | Deploy Node + PM2 + MySQL VPS doc | `https://domain/api/health` UP |
| 7.2 | Let's Encrypt HTTPS | Store/login không mixed content |
| 7.3 | `appsettings.Production.json` domain thật | App release → VPS |
| 7.4 | Đổi `ADMIN_SECRET` production | Dev secret không dùng prod |
| 7.5 | FAQ 15 câu Vi/En footer store | Khách tự phục vụ |
| 7.6 | Beta 20 user — form feedback → admin list | 5+ phản hồi thật |

---

## 6. Checklist test tay (rút từ `TESTING_GUIDE` + README)

Chạy **trước beta** và **trước v1.0 tag**:

### Web (TESTING_GUIDE §A + README)

- [ ] Đăng ký → Trial 14 ngày
- [ ] Login → dashboard plan + expiry
- [ ] Admin secret → tab Người dùng + tạo key
- [ ] Redeem key store → Ultra
- [ ] Chat widget không 500
- [ ] Notification gửi/nhận (nếu bật)

### Desktop (TESTING_GUIDE §B + FEATURE_REQUIREMENTS)

- [ ] Onboarding lần đầu
- [ ] Scan virus → kết quả + quarantine
- [ ] Disk cleanup → GB recovered
- [ ] Game Mode bật/tắt
- [ ] Benchmark chạy xong có điểm
- [ ] Privacy Guardian hosts/telemetry
- [ ] Upgrade nhập key → plan đổi
- [ ] Offline 3 ngày → vẫn licensed

### Release

- [ ] Installer VM sạch
- [ ] `dotnet build avasec.sln` 0 errors
- [ ] VPS HTTPS health UP

---

## 7. Luồng người dùng bắt buộc (10 journey)

| # | Journey | Doc | Pass |
|---|---------|-----|------|
| J1 | Khách mới: web đăng ký → Trial | README | 14 ngày active |
| J2 | Admin: tạo key → Zalo gửi khách | ADMIN_API | Key redeem OK |
| J3 | Khách: store kích hoạt → dashboard Ultra | README §4 | Plan đổi |
| J4 | Cài app → onboarding → portal login | DESIGN §5.A | First-run xong |
| J5 | Quét + dọn không internet | README offline | Không cần server |
| J6 | Gamer: Game Mode + RAM | IMPLEMENTATION 3.3 | FPS feel OK |
| J7 | Hết Trial → nhắc store | BUSINESS | Không khóa máy |
| J8 | Mất mạng 7 ngày | README | App vẫn Pro |
| J9 | Admin khóa user → login fail | ADMIN | IsActive=false |
| J10 | Gỡ cài → không file rác | INSTALLER | Uninstall sạch |

---

## 8. Template prompt chuẩn (mọi session)

```text
Bối cảnh: avasec / AVA Security. Đọc:
- README.md (offline + portal lite)
- Business/10_NO_CODE_MASTER_PLAN_FROM_DOC.md Phase [X] session [ID]
- doc/[tên file liên quan]

Mục tiêu: [1 câu — user thấy/chạm gì]
Vibe: DESIGN_GUIDELINES — Cyberpunk Professional, #0f172a, cyan accent, song ngữ Vi/En
Phạm vi: [folder/file cụ thể]
Done khi:
  1) ...
  2) ...
  3) ...
Không làm: cloud sync, Flutter, MAUI, Monitoring API, refactor ngoài phạm vi
Sau xong: hướng dẫn test tay 2 bước
```

---

## 9. Nhịp tuần & ưu tiên

```
Thứ 2–6: 1 session (45–90 phút) theo thứ tự Phase 0→7
Thứ 7:   Chạy 2–3 journey J* + tick checklist §6
Chủ nhật: Ghi tiến độ bảng §10, chọn session tuần sau
```

**Nếu chỉ có 30 phút:** làm session có **$** (tiền/ship trước):

`2.2` → `2.5` → `3.3` → `6.2` → `7.1`

---

## 10. Bảng tiến độ (tự điền)

| Tuần | Phase | Sessions | Pass | Ghi chú |
|------|-------|----------|------|---------|
| 1 | 0 | 0.1–0.5 | ☐ | |
| 2 | 1 | 1.1–1.8 | ☐ | |
| 3 | 2 | 2.1–2.7 | ☐ | |
| 4 | 3 | 3.1–3.6 | ☐ | |
| 5 | 4 | 4.1–4.7 | ☐ | |
| 6 | 5 | 5.1–5.6 | ☐ | |
| 7 | 6 | 6.1–6.4 | ☐ | |
| 8–10 | 7 | 7.1–7.6 | ☐ | **v1.0 ship** |

---

## 11. Định nghĩa DONE v1.0 (tổng hợp doc + README)

Sản phẩm **shippable** khi đủ **7 điều**:

1. **FEATURE_REQUIREMENTS** must-have: scan, cleanup, RAM — chạy offline  
2. **IMPLEMENTATION_PLAN** Phase 3 views — sidebar wire hết  
3. **README** luồng license — đăng ký → Trial → key → activate  
4. **README** offline cache — 7 ngày không server  
5. **INSTALLER_GUIDE** — cài VM sạch được  
6. **DEPLOYMENT_GUIDE** — portal VPS HTTPS ~$5/tháng  
7. **TESTING_GUIDE** — 10 journey J* pass  

→ Tag **v1.0.0** + beta 20 user + thu tiền thủ công (Momo/chuyển khoản + admin gửi key).

---

## 12. Index tài liệu `doc/` theo chủ đề

| Chủ đề | File đọc khi cần |
|--------|------------------|
| **Kế hoạch tổng** | `IMPLEMENTATION_PLAN_2026.md`, `Project_Plan_2026.md` |
| **Tính năng** | `FEATURE_REQUIREMENTS_2026_02_06_1001.md`, `UPGRADE_FEATURES_*` |
| **UI/UX** | `DESIGN_GUIDELINES_2026.md`, `UI_UX_*`, `WEB_DESIGN_CONCEPT_2026.md` |
| **Chạy & debug** | `RUN_INSTRUCTIONS_*`, `LOGIN_FIX_GUIDE.md`, `DESKTOP_UI_TROUBLESHOOTING.md` |
| **Admin & API** | `ADMIN_API_GUIDE.md`, `report_admin_features_*` |
| **Deploy** | `DEPLOYMENT_GUIDE_2026_02_08.md`, `INSTALLER_GUIDE.md` |
| **Test** | `TESTING_GUIDE_2026_02_08.md`, `TEST_RESULTS_*` |
| **Lỗi đã biết** | `PROJECT_ISSUES_REPORT_2026_02_08.md`, `UI_ERROR_FIX_*` |
| **Tối ưu** | `OPTIMIZATION_PLAN_20260208.md` |
| **Hoãn 2027+** | `HYBRID_*`, `MONITORING_ONLY_*`, `PROJECT_2028_PLAN.md` |

---

## 13. Session tiếp theo (bắt đầu ngay)

**Phase 0.1** — copy prompt:

```text
Đọc doc/RUN_INSTRUCTIONS_2026_02_07.md, doc/RUN_GUIDE_2026_02_04_1812.md, README.md.
Tạo scripts/start-all.ps1 và Business/11_QUICK_START.md (15 phút từ zero).
Done: .\scripts\start-all.ps1 → MySQL check + portal port 3001 + in 4 URL + health UP.
Vibe: AVA Security, hướng dẫn song ngữ Vi/En.
Không: cloud sync, đổi stack.
```

---

**Liên kết:** [09_NO_CODE_PLAN_A_TO_Z.md](./09_NO_CODE_PLAN_A_TO_Z.md) · [08_VIBE_CODING_PLAN.md](./08_VIBE_CODING_PLAN.md) · [doc/NO_CODE_INDEX.md](../doc/NO_CODE_INDEX.md)

**Cập nhật:** 2026-06-17
