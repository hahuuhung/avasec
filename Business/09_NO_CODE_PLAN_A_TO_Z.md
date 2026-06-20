# Kế Hoạch Lập Trình No-Code A → Z — avasec / AVA Security

**Nguồn:** [README.md](../README.md)  
**Ngày:** 2026-06-17  
**Cách làm:** Bạn mô tả → Cursor (AI) code → bạn test tay → lặp. **Bạn không cần viết code.**

---

## Bản đồ A → Z (1 trang)

```
A Architecture    → Hiểu 2 phần: App offline + Website lite
B Business        → Trial / Ultra / Lifetime — ai trả tiền, khi nào
C Components      → File/folder nào làm gì
D Dev setup       → Máy dev chạy được trong 15 phút
E Environment     → .env, secret, MySQL
F First run       → Mở app + portal lần đầu
G Register/Login  → Khách có tài khoản
H License core    → Trial 14 ngày tự động
I Store           → Bảng giá + kích hoạt key
J Admin           → Tạo key, quản lý user
K Desktop sync    → App đọc license từ portal
L Offline 7 ngày  → Mất mạng vẫn dùng
M Local power     → Quét, AI, dọn rác (100% local)
N Web polish      → Dashboard, UI, song ngữ
O Package         → Installer .exe cho khách
P Production      → VPS ~$5/tháng, domain, HTTPS
Q Quality         → Checklist test tay
R Release         → Ship v1.0 cho 20–50 beta user
S Support         → Chat, FAQ, báo lỗi
T Test journeys   → 10 luồng bắt buộc pass
U User personas   → Gamer / văn phòng / admin
V Vibe rules      → 1 session = 1 kết quả
W Weekly rhythm   → Nhịp làm mỗi tuần
X Anti-patterns   → Không làm gì
Y Yield / KPI     → Đo thành công
Z Zero → Ship     → Định nghĩa “xong sản phẩm”
```

---

# PHẦN I — HIỂU SẢN PHẨM (A → F)

## A — Architecture / Kiến trúc

**Mô hình tiết kiệm** (từ README):

| Thành phần | Vai trò | Chi phí |
|------------|---------|---------|
| **Desktop** `avasec.ui` | Quét virus, AI, dọn rác — **100% local** | $0 |
| **Website** `avasec.server` | Đăng ký, login, mua key, admin | ~$5/tháng VPS |
| **Cloud sync / VirusTotal** | **Tắt** — không build | $0 |

```
┌─────────────────┐   đăng ký / login / key    ┌──────────────────┐
│  AVA Security   │ ◄────────────────────────► │  Website Portal  │
│  (offline)      │   AVA-XXXX-...             │  Node + MySQL    │
└─────────────────┘                            └──────────────────┘
        │                                                │
        └── cache license 7 ngày offline ────────────────┘
```

**Nguyên tắc vibe:** Mọi tính năng nặng chạy trên máy khách. Website chỉ lo **tài khoản + tiền + key**.

---

## B — Business / Kinh doanh

### Luồng tiền (đơn giản)

1. Khách **đăng ký** web → Trial **14 ngày** tự động  
2. Khách **thanh toán** (chuyển khoản / Momo — Stripe sau)  
3. **Admin** tạo key `AVA-...` → gửi khách  
4. Khách **kích hoạt** (`store.html` hoặc app → Upgrade)  
5. App **cache** — offline **7 ngày** không cần server  

### Gói giá (mặc định README)

| Gói | Giá | Cách có |
|-----|-----|---------|
| Trial | Free | Tự động khi đăng ký |
| Ultra | $19/năm | Key admin tạo |
| Lifetime | $99 | Key admin tạo |

**Vibe session B:** *“Làm store.html hiển thị đúng 3 gói + CTA ‘Liên hệ / Kích hoạt’ — không cần Stripe v1.”*

---

## C — Components / Thành phần codebase

| Đường dẫn | Là gì | Ai vibe |
|-----------|-------|---------|
| `avasec.ui/` | App WPF desktop | Session K, L, M, O |
| `avasec.core/` | Logic license, brand | Session K, L |
| `avasec.server/` | Portal Node.js | Session G–J, N, P |
| `avasec.server/public/` | HTML/JS web | Session G–J, I, N |
| `avasec.server/routes/` | API auth, license | Session G–J |
| `avasec.server/database.sql` | Schema MySQL | Session E, H |
| `scripts/` | Chạy nhanh | Session D, F |
| `install/` | Inno Setup | Session O |
| `Business/` | Chiến lược (không code) | Đọc trước khi vibe |

**URL dev (README):**

| URL | Mục đích |
|-----|----------|
| `/index.html` | Đăng ký / Đăng nhập |
| `/store.html` | Bảng giá & kích hoạt key |
| `/dashboard.html` | Quản lý tài khoản |
| `/admin.html` | Admin key + user |

---

## D — Dev setup / Cài môi trường (Session 1)

**Mục tiêu:** Người mới clone repo → chạy được trong 15 phút.

**Prompt gợi ý (copy Cursor):**

```text
Context: avasec README — desktop .NET 9 + portal Node MySQL port 3001.
Mục tiêu: Một script scripts/start-all.ps1 kiểm tra MySQL, start portal, in ra 4 URL.
Done khi:
  1) .\scripts\start-all.ps1 → http://localhost:3001/api/health = UP
  2) README.md cập nhật đường dẫn avasec.ui (không SysAnti.UI)
  3) File Business/10_QUICK_START.md — 5 bước từ zero
Không làm: cloud sync, Stripe, refactor solution.
```

**Cần cài (một lần):** .NET 9 SDK · Node 20 · XAMPP MySQL (hoặc MySQL riêng)

---

## E — Environment / Biến môi trường

**File:** `avasec.server/.env`

| Biến | Dev | Production |
|------|-----|------------|
| `PORT` | 3001 | 3001 hoặc 80 |
| `ADMIN_SECRET` | `dev-secret` | Chuỗi random dài |
| `DB_HOST` | localhost | IP VPS |
| `DB_USER` / `DB_PASS` | root / (trống) | User riêng, mật khẩu mạnh |
| `DB_NAME` | `avasec_db` | `avasec_db` |

**Prompt E:** *“Tạo .env.example + hướng dẫn trong QUICK_START — không commit .env thật.”*

---

## F — First run / Chạy lần đầu

**Thứ tự test tay (không code):**

1. XAMPP → Start **MySQL**  
2. Import `avasec.server/database.sql` → DB `avasec_db`  
3. `cd avasec.server && npm install && node server.js`  
4. Mở http://localhost:3001/index.html  
5. `dotnet run --project avasec.ui`  
6. App mở → onboarding (nếu lần đầu)  

**Done F:** Cả web và app mở không lỗi đỏ trong console.

---

# PHẦN II — XÂY SẢN PHẨM (G → P)

## G — Register & Login

**User story:** Khách tạo tài khoản hoặc đăng nhập trên web.

| Bước vibe | Prompt tóm tắt | Done |
|-----------|----------------|------|
| G.1 | Đăng ký → tạo user MySQL + bcrypt password | Form ĐĂNG KÝ → success toast |
| G.2 | Đăng nhập → `localStorage` `avasec_user` | Login → redirect dashboard |
| G.3 | Đăng xuất xóa session | Nút logout → về index |

**Test tay:** Tạo user `test01` → login → thấy tên trên dashboard.

---

## H — License core (Trial 14 ngày)

**User story:** Mỗi user mới có Trial tự động — đúng README.

| Bước vibe | Prompt | Done |
|-----------|--------|------|
| H.1 | `POST /register` insert Licenses Trial +14 ngày | API trả licenseKey |
| H.2 | `GET /api/auth/user/:id` trả license | Dashboard hiện Trial + ngày hết |
| H.3 | Seed `admin` có Trial trong `database.sql` | Admin login thấy license |

---

## I — Store & kích hoạt key

**User story:** Khách xem giá và nhập key `AVA-...`.

| Bước vibe | Prompt | Done |
|-----------|--------|------|
| I.1 | `store.html` — 3 gói Trial/Ultra/Lifetime, giá VND+EN | Giống README bảng giá |
| I.2 | Form nhập key → redeem API | Key hợp lệ → license đổi Ultra |
| I.3 | Key sai / hết hạn → message rõ song ngữ | Không crash, có toast đỏ |

**Luồng README hoàn chỉnh:** Đăng ký → Trial → (trả tiền ngoài app) → admin tạo key → khách activate store.

---

## J — Admin (bán hàng thủ công)

**User story:** Bạn (admin) tạo key và quản lý khách — chi phí web chỉ ~$5/tháng, không cần payment gateway v1.

| Bước vibe | Prompt | Done |
|-----------|--------|------|
| J.1 | `admin.html?secret=...` — tab Người dùng | Thấy all users + license |
| J.2 | Tạo key batch `curl` hoặc UI tab Tạo key | Key `AVA-...` vào kho |
| J.3 | Tab Kho key — copy gửi khách | Filter chưa dùng / đã dùng |
| J.4 | Khóa user / đổi mật khẩu / cấp Trial | Nút 🔑 🎁 🔒 hoạt động |

**Admin dev:** http://localhost:3001/admin.html?secret=dev-secret

---

## K — Desktop ↔ Portal (license sync)

**User story:** App biết user đã login portal và có license gì.

**Cấu hình app** (`avasec.ui/appsettings.Development.json`):

```json
{
  "ApiBaseUrl": "http://localhost:3001",
  "PortalUrl": "http://localhost:3001/store.html",
  "LoginUrl": "http://localhost:3001/index.html"
}
```

| Bước vibe | Prompt | Done |
|-----------|--------|------|
| K.1 | Login app mở browser portal hoặc form nhúng | User portal → app nhận license |
| K.2 | Upgrade window nhập key `AVA-...` | Key redeem → UI đổi Ultra |
| K.3 | Hiện plan + expiry trên dashboard desktop | Khớp với web |

---

## L — Offline cache 7 ngày

**User story:** Mất mạng 7 ngày — app vẫn coi là licensed (README).

| Bước vibe | Prompt | Done |
|-----------|--------|------|
| L.1 | License cache SQLite/local + timestamp | Tắt WiFi → app vẫn mở Pro |
| L.2 | Sau 7 ngày offline → nhắc kết nối | Banner “Cần sync license” |
| L.3 | Sync lại → gia hạn cache | Bật mạng → tự refresh |

---

## M — Local power (100% offline features)

**User story:** Giá trị chính **không cần server** — đúng triết lý README.

| Tính năng | Vibe ưu tiên | Done |
|-----------|--------------|------|
| Virus scan | Quét xong → kết quả + quarantine | 1 click scan |
| Disk cleanup | 3 cột dung lượng → dọn | Thấy GB recovered |
| RAM / Startup | Optimize → thông báo | Số liệu đổi trên dashboard |
| Game Mode | Bật/tắt 1 nút | Indicator rõ |
| AI local | Heuristic / chat offline | Không gọi API cloud |

**Prompt M (mẫu):** *“Wire sidebar Dashboard → Disk Cleanup view, không crash, giữ song ngữ.”*

---

## N — Web polish

| Bước vibe | Prompt | Done |
|-----------|--------|------|
| N.1 | Rebrand hết SysAnti → AVA Security | Không chữ cũ trên web |
| N.2 | Dashboard hiện license + link store | User thường hiểu trạng thái |
| N.3 | Fix chat widget (bảng ChatMessages) | Không 500 console |
| N.4 | Mobile responsive cơ bản | Đọc được trên điện thoại |

---

## O — Package / Installer

**User story:** Khách Windows cài bằng 1 file `.exe` — không cần `dotnet run`.

| Bước vibe | Prompt | Done |
|-----------|--------|------|
| O.1 | `scripts/publish.ps1` → folder release | AVASecurity.exe chạy |
| O.2 | Inno Setup `install/avasec-setup.iss` | Cài → shortcut → gỡ sạch |
| O.3 | Test VM Win10/11 sạch | Video 2 phút demo cài |

---

## P — Production (~$5/tháng)

**User story:** Portal lên VPS — app desktop vẫn trỏ URL production.

| Bước vibe | Prompt | Done |
|-----------|--------|------|
| P.1 | Deploy Node + PM2 + MySQL trên VPS | `https://yourdomain/api/health` UP |
| P.2 | Let's Encrypt HTTPS | Store/login không mixed content |
| P.3 | `appsettings.Production.json` ApiBaseUrl domain thật | App release build kết nối VPS |
| P.4 | Đổi `ADMIN_SECRET` production | Dev secret không dùng prod |

**Chi phí mục tiêu:** VPS $5 + domain ~$10/năm ≈ **<$10/tháng** giai đoạn đầu.

---

# PHẦN III — SHIP & VẬN HÀNH (Q → Z)

## Q — Quality checklist (test tay, không code)

Trước mỗi release, tick từng ô:

- [ ] `dotnet build avasec.sln` — 0 errors  
- [ ] Portal health UP + MySQL connected  
- [ ] Đăng ký → Trial 14 ngày  
- [ ] Admin tạo key → redeem store → license Ultra  
- [ ] App nhập key → dashboard đổi plan  
- [ ] Tắt mạng 24h → app vẫn licensed  
- [ ] Scan + cleanup chạy không server  
- [ ] Installer cài VM sạch  

---

## R — Release v1.0

**Thứ tự ship (no-code marketing + vibe code song song):**

1. **Beta nội bộ** — 5 máy (bạn + bạn bè)  
2. **Beta công khai** — 20–50 user, form feedback  
3. **v1.0 tag** — GitHub release + installer + CHANGELOG  
4. **Hướng dẫn** — QUICK_START + video 90s cài + kích hoạt key  

---

## S — Support

| Kênh | Vibe session | Done |
|------|--------------|------|
| Chat widget web | Fix DB + bot trả lời FAQ 5 câu | Không 500 |
| FAQ trên store | 10 câu Vi/En | Link từ footer |
| Email hỗ trợ | Hiện trên store + About | Khách biết liên hệ |

---

## T — Test journeys (10 luồng bắt buộc)

| # | Journey | Pass khi |
|---|---------|----------|
| T1 | Khách mới đăng ký web | Trial active 14 ngày |
| T2 | Khách login dashboard | Thấy plan + expiry |
| T3 | Admin tạo 1 key Ultra | Key trong kho |
| T4 | Khách redeem key store | Plan → Ultra |
| T5 | Cài app lần đầu | Onboarding xong |
| T6 | App login + sync license | Khớp web |
| T7 | App offline 3 ngày | Vẫn Pro |
| T8 | Quét virus + dọn disk | Không cần internet |
| T9 | Hết Trial → nhắc mua | Link store, không khóa máy đột ngột |
| T10 | Gỡ cài app | Không file rác critical |

---

## U — User personas (prompt theo đối tượng)

| Persona | Vibe ưu tiên session |
|---------|----------------------|
| **Gamer** | Game Mode, RAM, FPS feel dashboard |
| **Văn phòng** | Cleanup, startup, ít popup |
| **Admin (bạn)** | J — tạo key nhanh, copy gửi Zalo |
| **Khách trả tiền** | I + store + email key rõ ràng |

---

## V — Vibe rules (luật vàng)

```
1 session = 1 outcome test được trong 30 phút
Prompt phải có: Mục tiêu · Vibe · Done khi (3 bullet) · Không làm
Sửa vibe sai → prompt lại, đừng sửa code tay (trừ khi bạn muốn)
Không thêm: cloud sync, VirusTotal, Stripe, Tauri — ngoài README v1
```

**Template copy mọi session:**

```text
Đọc README.md và Business/09_NO_CODE_PLAN_A_TO_Z.md.
Mục tiêu: […]
Vibe: AVA Security — tối, cyan, song ngữ Vi/En, nhẹ, không quảng cáo.
Phạm vi: [folder/file]
Done khi: 1) … 2) … 3) …
Không làm: refactor toàn app, đổi stack, thêm payment gateway.
```

---

## W — Weekly rhythm

| Ngày | Việc |
|------|------|
| T2–T5 | 1 vibe session (45–90 phút) theo thứ tự G→P |
| T6 | Chạy checklist Q + 2 journey T |
| CN | Nghỉ hoặc ghi ý tưởng persona U |

**Thứ tự đề xuất 6 tuần:**

| Tuần | Chữ cái | Focus |
|------|---------|-------|
| 1 | D–F–G–H | Chạy được + auth + trial |
| 2 | I–J | Store + admin bán key |
| 3 | K–L | App license + offline |
| 4 | M | Local features wire |
| 5 | N–O | Web polish + installer |
| 6 | P–Q–R | VPS + test + beta release |

---

## X — Anti-patterns (không vibe)

| ❌ Không | ✅ Thay bằng |
|----------|--------------|
| Cloud sync đa thiết bị | Chỉ license cache 7 ngày (README) |
| VirusTotal API ($) | Local heuristic + signature |
| Stripe ngay v1 | Chuyển khoản + admin gửi key |
| 2 backend Node + ASP.NET | Chỉ Node portal |
| Rewrite Rust/Tauri 2026 | WPF ship trước |
| 10 tính năng 1 prompt | Tách session V |

---

## Y — Yield / KPI (đo bằng tay giai đoạn đầu)

| KPI | Mục tiêu v1.0 |
|-----|---------------|
| Setup time người mới | < 15 phút (QUICK_START) |
| Luồng đăng ký → Trial | < 2 phút |
| Admin tạo + gửi key | < 5 phút |
| App offline licensed | 7 ngày |
| Chi phí hạ tầng | ≤ $10/tháng |
| Beta users | 20–50 |
| Paid (thủ công) | 5–10 key Ultra/Lifetime |

---

## Z — Zero → Shippable (định nghĩa “XONG”)

Sản phẩm **shippable v1.0** khi đủ **5 câu**:

1. **Khách** đăng ký web → Trial 14 ngày → thấy trên dashboard  
2. **Bạn** admin tạo key → khách kích hoạt → Ultra/Lifetime  
3. **App** cài bằng installer → login/kích hoạt → quét + dọn **không cần server**  
4. **Mất mạng** 7 ngày → app vẫn dùng được  
5. **Portal** chạy trên VPS HTTPS ~$5/tháng — không cloud sync  

→ Đạt Z = tag **v1.0** + gửi beta + thu tiền thủ công qua key.

---

# PHỤ LỤC — Thư viện prompt theo thứ tự (30 session)

| # | ID | Prompt một dòng |
|---|-----|-----------------|
| 1 | D | Script start-all + QUICK_START 15 phút |
| 2 | E | .env.example + doc secrets |
| 3 | G.1 | Register bcrypt + validation |
| 4 | G.2 | Login + localStorage + redirect |
| 5 | H.1 | Auto Trial 14 ngày on register |
| 6 | H.2 | Dashboard fetch license hiển thị |
| 7 | H.3 | Seed admin license database.sql |
| 8 | I.1 | store.html 3 gói giá README |
| 9 | I.2 | Redeem key API + UI |
| 10 | J.1 | Admin users tab (đã có → polish) |
| 11 | J.2 | Admin create-keys UI |
| 12 | K.1 | App appsettings portal URLs |
| 13 | K.2 | Upgrade window redeem key |
| 14 | K.3 | Desktop dashboard plan display |
| 15 | L.1 | License cache local 7 ngày |
| 16 | L.2 | Offline grace banner |
| 17 | M.1 | Wire disk cleanup navigation |
| 18 | M.2 | Wire virus scan + results |
| 19 | M.3 | Game Mode toggle dashboard |
| 20 | N.1 | Fix ChatMessages + widget |
| 21 | N.2 | Remove SysAnti strings web |
| 22 | O.1 | publish.ps1 single exe folder |
| 23 | O.2 | Inno Setup test VM |
| 24 | P.1 | VPS deploy doc + PM2 |
| 25 | P.2 | HTTPS + production appsettings |
| 26 | Q | Full manual checklist pass |
| 27 | T | Run 10 test journeys document failures |
| 28 | S | FAQ 10 câu store footer |
| 29 | R | CHANGELOG + GitHub release |
| 30 | Z | Beta invite 20 users |

---

## Bảng tiến độ (tự điền)

| Tuần | Session # | ID | Pass? | Ghi chú |
|------|-----------|-----|-------|---------|
| 1 | | D | ☐ | |
| 1 | | G–H | ☐ | |
| 2 | | I–J | ☐ | |
| … | | | | |

---

**Liên kết:** [README.md](../README.md) · [08_VIBE_CODING_PLAN.md](./08_VIBE_CODING_PLAN.md) · [03_BUSINESS_STRATEGY.md](./03_BUSINESS_STRATEGY.md)

**Cập nhật:** 2026-06-17
