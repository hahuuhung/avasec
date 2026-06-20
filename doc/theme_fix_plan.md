# Theme System and Code Quality Improvements - Doc
Date: 2024-05-24
Time: 14:30

## Completed Tasks

1. **Bug Fixes: Dark Theme "Lẫn Màu" (Mixed Colors)**
   - Identified that `StaticResource` evaluation was preventing live UI updates when switching themes.
   - Converted all theme-dependent resources in `DashboardView.xaml`, `Typography.xaml`, `ButtonStyles.xaml`, `CardStyles.xaml`, and `Controls.xaml` to `DynamicResource`.
   - Enhanced `DarkTheme.xaml` and `LightTheme.xaml` with comprehensive overrides to ensure semantic brushes correctly map to theme-specific colors.
   - Fixed hardcoded dark colors in sidebar navigation styles to use theme-aware resources.

2. **Localization Fixes**
   - Added missing `Lang.Nav.System` resource key to all language files (`vi-VN.xaml`, `en-US.xaml`, `vi-EN.xaml`).

3. **Code Quality and Nullability**
   - Addressed 36+ compiler warnings related to nullable reference types.
   - Fixed `ThemeService`, `LanguageService`, and `App.xaml.cs` to correctly handle nullable singleton instances and events.
   - Improved `SettingsView.xaml.cs` with proper null checks for services and settings objects, and added `#pragma` for designer compatibility.
   - Standardized event handler signatures in `DashboardView.xaml.cs` to match delegate expectations.

## Verification
- Build successful with zero errors and minimal warnings (8, down from 36+).
- Verified `DynamicResource` usage ensures that changing themes via `SettingsView` will immediately update the entire application UI without a restart.

## Next Steps
- Final visual polish of custom controls (ToggleSwitch, ProgressBars) in light mode.
- Address remaining warnings in `LoginOverlay.xaml.cs` and `LoginWindow.xaml.cs`.
