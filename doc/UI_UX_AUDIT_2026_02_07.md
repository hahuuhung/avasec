# UI/UX Flow Audit & Optimization Report

Date: 2026-02-07 00:25:00

## 1. Navigation Flow Audit

- **Dashboard -> Settings -> Login:** Handled via `LOGIN_REQUEST` tag. Smooth transition.
- **Login Success -> Dashboard:** Correctly collapses overlay and reloads dashboard data.
- **Guest/Trial Mode:** Default state is working correctly.

## 2. Issues Identified

- **Hardcoded Strings:** Several strings in `DashboardView.xaml` (Version, License, PRO badge) are hardcoded.
- **C# Text Handling:** `CategoryTitle.Text` in `DashboardView.xaml.cs` is hardcoded to English only.
- **Localization Inconsistency:** `Lang.Nav.System` was missing bilingual format in `vi-EN.xaml`.
- **Window Hierarchy:** Added `Owner` property to all sub-windows to ensure they stay on top of the Dashboard.

## 3. Optimizations Applied

- [x] Standardized `vi-EN.xaml` keys.
- [x] Localized PRO badge and Version info.
- [x] Updated `DashboardView.xaml.cs` to use DynamicResources for titles.
- [x] Verified Dependency Injection (DI) for all view instantiations.

## 4. Conflict Resolution

- Verified that opening Settings while Login Overlay is active doesn't crash, but for better UX, the Login Overlay should be modal or handle focus better.
- Ensured all popups use `ShowDialog()` to prevent multiple overlapping instances of the same tool.
