# 🛠️ Optimization & Refactoring Plan (Kế hoạch Tối ưu hóa & Tái cấu trúc)

**Date:** 2026-02-08
**Target:** SysAnti "Ultra" Core & Plugins

## 1. Code Quality & Standards (Chất lượng Mã nguồn)

The goal is to ensure all code is self-documenting and accessible to both Vietnamese and English speakers.
(Mục tiêu là đảm bảo mã nguồn dễ hiểu và dễ tiếp cận cho cả người Việt và người Anh.)

- [ ] **Bilingual Comments (Chú thích Song ngữ):**
  - Apply `// English / Vietnamese` format to all public methods and complex logic.
  - (Áp dụng định dạng chú thích song ngữ cho tất cả phương thức công khai và logic phức tạp.)
  - **Focus Areas (Khu vực trọng tâm):** `PluginManager.cs`, `NetworkWindow.xaml.cs`, `FileScannerService.cs`.

- [ ] **Localization (Đa ngôn ngữ):**
  - Move hardcoded strings (e.g., "Monitoring...", "Error loading...") to `Strings.resx` or `LanguageService`.
  - (Chuyển các chuỗi cứng sang `Strings.resx` hoặc `LanguageService` để hỗ trợ đa ngôn ngữ.)

## 2. Performance Optimization (Tối ưu hóa Hiệu năng)

### A. Core Services

- **`PluginManager`**:
  - **Issue:** Currently loads plugins sequentially using `await` inside a loop.
  - **Fix:** Use `Task.WhenAll` to load all DLLs in parallel.
  - (Sử dụng `Task.WhenAll` để tải tất cả DLL song song thay vì tuần tự.)

### B. Network Fortress (Giám sát Mạng)

- **`NetworkWindow`**:
  - **Issue:** Recreates `Polyline` objects every second.
  - **Fix:** Reuse existing `Polyline` and just update `Points` collection.
  - (Tái sử dụng đối tượng `Polyline` và chỉ cập nhật danh sách `Points` để giảm tải bộ nhớ.)
  - **Issue:** UI freezes if `GetIPStatistics` takes time.
  - **Fix:** Ensure stats collection runs on background thread, only update UI on Dispatcher.

### C. System Sweeper (Dọn dẹp Hệ thống)

- **`SweeperWindow`**:
  - **Issue:** File deletion in `foreach` might block if many files exist.
  - **Fix:** Batch file deletions or use `Parallel.ForEach`.

## 4. Completed Tasks (Các Tác vụ Đã hoàn thành) - 2026-02-08

- [x] **Core Services:** Optimized `PluginManager` with `Task.WhenAll` for faster startup. (Đã tối ưu hóa `PluginManager` sử dụng `Task.WhenAll` để khởi động nhanh hơn.)
- [x] **Network Fortress:** Optimized `DrawGraph` to reuse objects and added bilingual comments. (Đã tối ưu hóa thuật toán vẽ biểu đồ và thêm chú thích song ngữ.)
- [x] **System Sweeper:** Implemented `Parallel.ForEach` for file deletion to speed up cleaning. (Đã triển khai xóa file đa luồng để tăng tốc độ dọn dẹp.)
- [x] **Documentation:** Added bilingual comments to critical sections in `FileScannerService` and Plugin UIs. (Đã thêm chú thích song ngữ vào các phần quan trọng.)
