# avasec — AVA Security AI Computer Guardian

**Offline-first app** + **Website portal lite** cho tài khoản & license key.

## Mô hình tiết kiệm chi phí

| Thành phần | Chi phí | Ghi chú |
|------------|---------|---------|
| **Desktop app** | $0 | Quét, AI, dọn rác — 100% local |
| **Website** | ~$5/tháng | VPS nhỏ + MySQL (hoặc free tier) |
| **Cloud sync / VirusTotal** | $0 | Đã tắt |

```
┌─────────────┐     đăng ký / login / mua key      ┌──────────────────┐
│  AVA Security App  │ ◄────────────────────────────────► │  Website Portal  │
│  (offline)  │     kích hoạt AVA-XXXX-...       │  Node + MySQL    │
└─────────────┘                                    └──────────────────┘
       │                                                    │
       └── license cache 7 ngày offline ──────────────────┘
```

## Chạy nhanh

**App desktop:**
```powershell
cd E:\DEV28\Antigravity\avasec
dotnet run --project avasec.ui
```

**Website portal (dev):**
```powershell
cd avasec.server
npm install
# Cấu hình MySQL trong .env
set ADMIN_SECRET=your-secret
node server.js
```

| URL | Mục đích |
|-----|----------|
| http://localhost:3001/index.html | Đăng ký / Đăng nhập |
| http://localhost:3001/store.html | Bảng giá & kích hoạt key |
| http://localhost:3001/dashboard.html | Quản lý tài khoản |

## Luồng mua bán đơn giản

1. Khách **đăng ký** trên website → nhận Trial 14 ngày
2. **Thanh toán** (chuyển khoản / Momo / Stripe sau) → Admin tạo key:
   ```bash
   curl -X POST http://localhost:3001/api/license/admin/create-keys \
     -H "x-admin-secret: your-secret" \
     -H "Content-Type: application/json" \
     -d "{\"planId\":\"ultra\",\"count\":1}"
   ```
3. Gửi key `AVA-XXXX-...` cho khách
4. Khách **kích hoạt** trên store.html hoặc trong app (Upgrade → nhập key)
5. App **cache license** — dùng offline 7 ngày không cần server

## Gói giá (mặc định)

| Gói | Giá | Key |
|-----|-----|-----|
| Trial | Free | Tự động khi đăng ký |
| Ultra | $19/năm | `AVA-...` |
| Lifetime | $99 | `AVA-...` |

## Cấu hình app

`avasec.ui/appsettings.Development.json`:
```json
{
  "ApiBaseUrl": "http://localhost:3001",
  "PortalUrl": "http://localhost:3001/store.html",
  "LoginUrl": "http://localhost:3001/index.html"
}
```

## Kế hoạch phát triển (no-code / vibe coding)

Lập trình bằng AI (Cursor): bạn mô tả → AI code → bạn test.

| Tài liệu | Nội dung |
|----------|----------|
| [Business/11_MODULAR_CODEBASE_PLAN.md](./Business/11_MODULAR_CODEBASE_PLAN.md) | **18 module** — thứ tự hoàn thiện codebase |
| [Business/10_NO_CODE_MASTER_PLAN_FROM_DOC.md](./Business/10_NO_CODE_MASTER_PLAN_FROM_DOC.md) | **Tổng hợp `doc/`** — 45 session, 7 phase (bắt đầu từ đây) |
| [doc/NO_CODE_INDEX.md](./doc/NO_CODE_INDEX.md) | Index 118 file doc → đọc khi nào |
| [Business/09_NO_CODE_PLAN_A_TO_Z.md](./Business/09_NO_CODE_PLAN_A_TO_Z.md) | A→Z bám README — 30 session |
| [Business/08_VIBE_CODING_PLAN.md](./Business/08_VIBE_CODING_PLAN.md) | 8 sóng Q3 2026 — release rộng hơn |

**Admin dev:** http://localhost:3001/admin.html?secret=dev-secret

© 2026 avasec / AVA Security Team
