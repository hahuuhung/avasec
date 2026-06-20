# UI Runtime Error Fix Report
## Date: 2026-02-07 22:35 (Local Time)

### Problem Description
The application crashed during startup (or shortly after) with a `XamlParseException` related to missing resources. 

### Identity and Cause
The error log `startup_error.txt` revealed multiple broken `StaticResource` references:
1. `BodyStyle`: Used in `ChatWidget.xaml` but missing in `Typography.xaml` (the correct name is `BodyTextStyle`).
2. `TextButtonStyle`: Used in `DashboardView.xaml` but not defined (should be `LinkButtonStyle`).
3. `PrimaryLightBrush`: Used in `ChatWidget.xaml` but missing in `Colors.xaml`.
4. `TinyStyle`: Used in `ChatWidget.xaml` but missing in `Typography.xaml`.

These errors were likely introduced during a recent redesign phase and triggered because `DashboardView` (which contains `ChatWidget`) is instantiated on startup.

### Changes Made
1. **ChatWidget.xaml**:
   - Replaced `BodyStyle` with `BodyTextStyle`.
   - Replaced `TinyStyle` with `CaptionStyle`.
   - Replaced `PrimaryLightBrush` with `SelectedItemBgBrush`.
2. **DashboardView.xaml**:
   - Replaced `TextButtonStyle` with `LinkButtonStyle`.
3. **ProcessManagerWindow.xaml**:
   - Converted several `StaticResource` references to `DynamicResource` and fixed explicit `Setter` syntax to prevent similar runtime lookup issues in the future.

### Verification
- Ran `dotnet build`: Success.
- Ran `dotnet run --project SysAnti.UI`: Application started successfully and remained running without immediate crash.

### Status
Fixed / Đã sửa.
