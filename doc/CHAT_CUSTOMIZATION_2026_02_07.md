# Chat Window Customization Report (Position and Size)
## Date: 2026-02-07 23:00 (Local Time)

### Problem Description
The user requested further customization of the Chat Window:
1.  **Position**: Display at the top-right corner of the screen.
2.  **Controls**: Add a "Minimize" button in addition to the existing "Close" button.
3.  **Size**: Increase the window width by 30%.

### Changes Made
1.  **Size Adjustment**:
    *   Increased `ChatWindow.xaml` Width from `400` to `520` (exactly 30% increase).
    *   Updated `ChatWidget.xaml` to use `d:DesignWidth="400"` instead of fixed `Width`, allowing it to stretch to fill the new parent window size.

2.  **Window Controls**:
    *   Added a **Minimize (─)** button to the `ChatWidget` header, placed next to the Close button.
    *   Implemented `MinimizeButton_Click` event handler in `ChatWidget.xaml.cs` to minimize the parent window.

3.  **Positioning Logic**:
    *   Updated `DashboardView.xaml.cs` to calculate the position based on `SystemParameters.WorkArea`.
    *   **New Logic**:
        ```csharp
        Left = SystemParameters.WorkArea.Right - Width - 10;
        Top = SystemParameters.WorkArea.Top + 10;
        ```
    *   This ensures the window always opens at the top-right corner of the user's primary screen/work area.

### Verification
-   **Build**: Verified `dotnet build` succeeds.
-   **Functionality**:
    *   Chat window should now open at the top-right.
    *   Chat window should be wider (520px).
    *   Minimize button should effectively minimize the standalone window to the taskbar.

### Status
Complete / Đã hoàn thành.
