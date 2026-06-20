# Kế Hoạch Vibe Coding — AVA Security (avasec)

**Ngày:** 2026-06-17  
**Loại:** No-code plan — dùng AI (Cursor) mô tả ý tưởng, AI viết code, bạn review & ship  
**Baseline:** [01](./01_PROJECT_ANALYSIS_2026_06_16.md) · [02](./02_UPGRADE_PLAN_2026_2028.md) · [07](./07_AVA_SECURITY_BRAND.md)

---

## 1. Vibe Coding là gì (trong dự án này)?

**Vibe coding** = bạn không tự gõ code từng dòng. Bạn:

1. **Mô tả vibe** — cảm giác UX, luồng người dùng, kết quả mong muốn (tiếng Việt hoặc Anh đều được)
2. **Giao cho AI** — một session Cursor = một mục tiêu nhỏ, rõ, có tiêu chí “xong”
3. **Chạy & cảm nhận** — mở app/portal, thử tay, chụp lỗi hoặc mô tả “chưa đúng vibe”
4. **Lặp** — prompt sửa nhỏ cho đến khi đạt *done criteria*

> **Không phải:** nhồi 10 tính năng một lần, hoặc refactor toàn bộ solution.  
> **Là:** ship từng viên gạch — mỗi viên có thể demo được trong 30 phút.

---

## 2. North Star (neo mọi session)

| | |
|---|---|
| **Sản phẩm** | AVA Security — bảo vệ + tối ưu Windows, song ngữ Vi/En |
| **Cảm giác** | Nhẹ, hiện đại, không quảng cáo phiền — “máy nhanh hơn, an toàn hơn” |
| **Milestone gần** | **Release v2.0 Q3 2026** — installer, license online, beta 20–50 user |
| **Stack giữ nguyên** | WPF .NET 9 + Node portal + MySQL — không đổi stack trong 2026 |

Mỗi prompt nên kết thúc bằng: *“Làm xong thì tôi có thể thấy/chạm gì?”*

---

## 3. Checkpoint hiện tại (đã vibe xong)

Dùng làm điểm xuất phát — **không làm lại** trừ khi bug:

- [x] Rebrand AVA Security / `avasec`
- [x] Portal chạy port 3001 + MySQL (XAMPP)
- [x] Login web + đăng ký Trial 14 ngày
- [x] Admin: tab Người dùng, kho key, license
- [x] Onboarding wizard, accessibility (font/contrast)
- [x] CI build, script publish, Inno Setup khung
- [ ] ChatMessages table (chat widget 500)
- [ ] Installer test trên VM sạch
- [ ] Test coverage ≥ 40%
- [ ] Pricing page + store flow hoàn chỉnh

**Tiến độ ước lượng:** Phase A ~65% · Phase B ~15% · tổng kế hoạch 2026 ~58%

---

## 4. Quy tắc Vibe (bắt buộc)

### 4.1 Một session = một outcome

```
❌ "Hoàn thiện toàn bộ app"
✅ "Sau session: user đăng ký web → thấy license Trial trên dashboard"
```

### 4.2 Prompt template (copy-paste)

```text
Context: Dự án AVA Security (avasec), WPF + Node portal MySQL.
Mục tiêu: [mô tả 1 câu kết quả người dùng thấy được]
Vibe: [cảm giác UI — ví dụ: tối, cyan accent, song ngữ, không popup phiền]
Phạm vi: Chỉ sửa [file/khu vực nếu biết]. Không refactor ngoài phạm vi.
Done khi: [3 bullet có thể test tay]
Không làm: [những thứ tránh scope creep]
```

### 4.3 Sau mỗi session

| Bước | Hành động |
|------|-----------|
| 1 | `dotnet build avasec.sln` hoặc chạy portal |
| 2 | Thử đúng luồng trong *Done khi* |
| 3 | Ghi 1 dòng vào bảng tiến độ (cuối file này) |
| 4 | Commit **chỉ khi** bạn chủ động yêu cầu |

### 4.4 Khi vibe sai

Đừng sửa code tay — prompt lại:

```text
Vẫn chưa đúng vibe: [mô tả cảm giác / screenshot / URL]
Mong muốn: giống [ví dụ: tab Admin Người dùng — bảng rõ, nút 🔑🎁]
Giữ nguyên phần đã OK: [...]
```

---

## 5. Lộ trình Vibe — 8 sóng (Q3 2026)

Mỗi sóng = 1–3 session Cursor. Ưu tiên **dọc** (vertical slice), không **ngang** (refactor toàn cục).

---

### 🌊 Sóng 0 — Ổn định nền (3–5 ngày)

**Vibe:** *“Mở là chạy, không ERR_CONNECTION_REFUSED, admin thấy user.”*

| # | Session prompt (gợi ý) | Done khi |
|---|------------------------|----------|
| 0.1 | Tạo `scripts/start-all.ps1`: bật MySQL check + portal + mở browser admin | Một lệnh → health UP |
| 0.2 | Fix bảng ChatMessages + chat widget không 500 | Chat poll không lỗi console |
| 0.3 | Seed license Trial cho user `admin` trong `database.sql` | Login admin → dashboard có license |
| 0.4 | Doc `Business/09_QUICK_START.md` — 5 bước cho người mới | Người khác setup được trong 15 phút |

---

### 🌊 Sóng 1 — Luồng người dùng đầu tiên (1 tuần)

**Vibe:** *“Cài app → wizard → đăng nhập portal → thấy Trial → dọn disk một lần.”*

| # | Session prompt | Done khi |
|---|----------------|----------|
| 1.1 | First-run: onboarding → login portal → cache license offline 7 ngày | Máy offline vẫn dùng được sau khi đã login |
| 1.2 | Dashboard: sidebar mở đúng Benchmark, Privacy, Game Mode (wire nav) | Click sidebar → đúng view, không crash |
| 1.3 | Notification center: scan xong / cleanup xong → toast trong app | Hành động xong có thông báo |
| 1.4 | Desktop login DEBUG tách khỏi production — release chỉ portal auth | Build Release không có admin/admin hardcode |

---

### 🌊 Sóng 2 — Cửa hàng & tiền (1 tuần)

**Vibe:** *“Trang store đẹp, giá rõ, redeem key mượt — giống SaaS Việt hiện đại.”*

| # | Session prompt | Done khi |
|---|----------------|----------|
| 2.1 | `store.html`: 4 gói Free/Ultra/Ecosystem/Lifetime theo [03_BUSINESS_STRATEGY] | Giá VND + EN, CTA rõ |
| 2.2 | Redeem key từ kho admin → user được nâng cấp Ultra | Key admin tạo → user nhập → license đổi |
| 2.3 | `dashboard.html`: hiện plan + ngày hết hạn + nút Upgrade | User thường thấy trạng thái license |
| 2.4 | Trial 7 ngày Ultra trong UpgradeWindow desktop | Hết trial → nhắc mua, không khóa máy đột ngột |

---

### 🌊 Sóng 3 — Chất lượng ship (1–2 tuần)

**Vibe:** *“Build xanh, cài được trên máy sạch, không sợ demo.”*

| # | Session prompt | Done khi |
|---|----------------|----------|
| 3.1 | Đưa 5 plugins vào `avasec.sln`, build một lệnh | Solution build 0 errors |
| 3.2 | Fix nullable warnings batch 1 (Core + Auth) | Warnings giảm ≥50% |
| 3.3 | Test: LicenseService, AuthService, DiskCleaner — coverage ≥40% | `dotnet test` green + report |
| 3.4 | Inno Setup: cài trên Win10/11 VM, shortcut, uninstall sạch | Video 2 phút cài → mở app |

---

### 🌊 Sóng 4 — Polish UX (1 tuần)

**Vibe:** *“CCleaner + BKAV nhưng đẹp hơn — gamified nhẹ, không trẻ con.”*

| # | Session prompt | Done khi |
|---|----------------|----------|
| 4.1 | Dashboard adaptive layout Compact/Normal/Large | Resize cửa sổ → layout không vỡ |
| 4.2 | Game Mode Ultra: một nút bật/tắt + trạng thái rõ trên dashboard | Bật game mode → indicator đổi màu |
| 4.3 | Privacy Guardian: telemetry toggle có explain song ngữ | User hiểu bật/tắt làm gì |
| 4.4 | Empty states: chưa quét / chưa cleanup → CTA thân thiện | Không màn hình trống lạnh |

---

### 🌊 Sóng 5 — Beta & feedback (2 tuần)

**Vibe:** *“20 người dùng thật, biết họ bỏ app ở đâu.”*

| # | Session prompt | Done khi |
|---|----------------|----------|
| 5.1 | Telemetry opt-in + Plausible/GA4 trên web | Chỉ track khi user đồng ý |
| 5.2 | In-app feedback form → gửi portal API | Gửi ý kiến → admin thấy list |
| 5.3 | Changelog `CHANGELOG.md` + auto version trong About | User thấy version khi báo lỗi |
| 5.4 | Landing 1 trang `index.html` SEO cơ bản AVA Security VN | Google snippet có tagline |

---

### 🌊 Sóng 6 — Cloud sync MVP (Q4 2026)

**Vibe:** *“Đổi máy vẫn nhớ theme và whitelist.”*

| # | Session prompt | Done khi |
|---|----------------|----------|
| 6.1 | API sync settings user (theme, whitelist) Node only | Login 2 máy → settings khớp |
| 6.2 | Deprecate SysAnti.API — ghi README migration | Một backend duy nhất |
| 6.3 | HTTPS staging + `.env` production mẫu | Deploy doc rõ ADMIN_SECRET |

---

### 🌊 Sóng 7 — Chuẩn bị v2.0 launch

**Vibe:** *“Teaser → influencer → release -30% Lifetime.”*

| Checklist no-code (bạn làm, AI hỗ trợ copy) |
|---|
| Press kit 5 ảnh UI (Dashboard, Game Mode, Store, Admin, Onboarding) |
| Video demo 90s — script AI viết, bạn quay màn hình |
| Microsoft Store listing draft |
| Affiliate landing 30% commission |
| FAQ song ngữ 15 câu |

---

## 6. Nhịp làm việc đề xuất

```
Thứ 2–5: 1 vibe session / ngày (45–90 phút)
  → Prompt → AI code → Bạn test → Prompt fix (nếu cần)

Thứ 6: Review tuần (15 phút)
  → Cập nhật bảng tiến độ dưới
  → Chọn 1–2 session cho tuần sau

Cuối tháng: Demo 5 phút cho chính mình
  → Ghi lại “vibe đã đạt / chưa đạt”
```

---

## 7. Ma trận ưu tiên (khi bí ý prompt)

| Muốn cảm giác… | Session tiếp theo |
|----------------|-------------------|
| App professional, demo được | Sóng 3.4 Installer VM |
| Kiếm tiền sớm | Sóng 2 Store + redeem |
| Ít bug phiền | Sóng 0 Chat + seed license |
| Khác biệt vs Avast | Sóng 4 Game Mode + Privacy |
| Scale 2027 | Sóng 6 Cloud sync |

---

## 8. Anti-patterns (tránh phá vibe)

| Tránh | Vì sao |
|-------|--------|
| Rewrite sang Tauri/Rust 2026 | Lộ trình 2028 — scope creep |
| Gộp Node + ASP.NET một session | Quá lớn, khó test |
| UI redesign toàn app | Làm từng view khi wire nav |
| Thêm 10 tính năng AI | AI chat đủ MVP — on-device LLM là 2028 |
| Commit mỗi session tự động | Chỉ commit khi bạn hài lòng vibe |

---

## 9. Prompt mẫu — session tiếp theo (copy ngay)

```text
Context: AVA Security (avasec), portal Node MySQL port 3001.
Mục tiêu: User đăng nhập web bằng admin/admin123 thấy license Trial trên dashboard.
Vibe: Giống admin panel — bảng rõ, badge màu, song ngữ Vi/En.
Phạm vi: database.sql seed + dashboard fetch license. Không đổi desktop app.
Done khi:
  1) Login admin → dashboard hiện "Trial" + ngày hết hạn
  2) Tab admin Người dùng hiện license cho admin
  3) API /api/auth/user/1 trả license không null
Không làm: refactor auth, đổi port, rebrand lại.
```

---

## 10. Bảng tiến độ Vibe (tự cập nhật)

| Ngày | Sóng | Session | Kết quả | Vibe OK? |
|------|------|---------|---------|----------|
| 2026-06-17 | 0 | Admin tab Người dùng | API + UI users | ☐ |
| | | | | |
| | | | | |

---

## 11. Liên kết

- Chiến lược kinh doanh → [03_BUSINESS_STRATEGY.md](./03_BUSINESS_STRATEGY.md)
- Lộ trình kỹ thuật chi tiết → [02_UPGRADE_PLAN_2026_2028.md](./02_UPGRADE_PLAN_2026_2028.md)
- Cạnh tranh Avast/BKAV → [05_COMPETITIVE_SUPERIORITY_PLAN_2026_2028.md](./05_COMPETITIVE_SUPERIORITY_PLAN_2026_2028.md)

**Cập nhật:** 2026-06-17
