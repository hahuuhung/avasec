# CLAUDE.md – AI Coding Rules for This Project

## 1. General Principles

- Always prioritize readability over cleverness.
- Follow existing project structure and conventions.
- Do NOT refactor unrelated code.
- Do NOT change public APIs unless explicitly asked.
- Keep changes minimal and focused on the task.

## 2. Code Style

- Use meaningful variable and function names.
- No unused variables or dead code.
- Keep functions small and single-purpose.
- Prefer explicit code over magic abstractions.

## 3. Project Architecture

- vẽ Workflow Diagram
- Respect the current folder structure.
- Do not introduce new layers or patterns unless requested.
- Reuse existing utilities and components.

## 4. Testing

- If adding logic, also add or update tests.
- Do not break existing tests.
- Prefer simple, readable tests over complex ones.

## 5. Dependencies

- Do NOT add new dependencies unless explicitly allowed.
- If a new dependency is needed, explain why first.

## 6. Git & Workflow

- Do not touch files unrelated to the task.
- Each change should be easy to review.
- Assume the developer will review and commit manually.

## 7. Communication Rules

- If requirements are unclear, ask questions first.
- If something seems risky, warn before changing it.
- Explain non-trivial decisions briefly.

## 8. UI Design & Mockup Rules

- **ALWAYS create visual mockups** when planning new UI features or screens.
- Generate mockup images using the `generate_image` tool.
- Save mockups to `doc/` folder with descriptive names (e.g., `UI_MOCKUP_DarkMode_2026_02_06.png`).
- Include mockups in implementation plans and design documents.
- Update `doc/UI_MOCKUPS_INDEX_[DATE].md` with all new mockups.
- Mockups should show:
  - Layout and component placement
  - Color scheme and styling
  - Interactive elements (buttons, inputs, etc.)
  - Typical content/data examples
- **Luôn tạo hình ảnh mockup** khi lập kế hoạch tính năng UI mới hoặc màn hình mới.

## 9. Language & Localization (QUAN TRỌNG / IMPORTANT)

- **Song Ngữ / Bilingual UI:** Tất cả văn bản hiển thị trên UI (User Interface) **BẮT BUỘC** phải là song ngữ.
- **Format:** `Tiếng Việt / English` (Vietnamese first).
- **Phạm vi áp dụng / Scope:**
  - Tiêu đề cửa sổ, Header (e.g., "Tối ưu hóa / Optimization")
  - Nút bấm (e.g., "Đóng / Close", "Quét / Scan")
  - Tooltips, Descriptions, Status messages
  - Thông báo lỗi và hộp thoại (MessageBox)
- **Ngoại lệ / Exceptions:** Các thuật ngữ kỹ thuật chuyên ngành không có dịch chính xác (e.g., "CPU", "RAM", "Trojan").
