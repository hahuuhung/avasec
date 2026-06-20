# Support Chat Window (ChatWidget) Fix Report
## Date: 2026-02-07 22:55 (Local Time)

### Problem Description
The user reported two main issues with the support chat (ChatWidget):
1.  **UI Layout Error**: The header text (title and status) was partially cut off at the top.
2.  **Obstruction**: The chat widget was an overlay that obstructed the main application view. 

### Identity and Cause
-   **Cutoff**: Fixed height (60px) in `ChatWidget` header was insufficient for the applied padding (16px) and text style combined.
-   **Obstruction**: The widget was hosted inside `DashboardView` via a `ContentControl` overlay.

### Changes Made
1.  **ChatWidget.xaml Refinement**:
    *   Changed header `RowHeight` to `Auto` to prevent vertical truncation.
    *   Adjusted header padding and margins for better text alignment.
    *   Increased "Send" button width from 60 to 80 to prevent text truncation ("Sen...").
    *   Added localized strings for "Send" button.
2.  **Standalone ChatWindow**:
    *   Created `ChatWindow.xaml` and `ChatWindow.xaml.cs`.
    *   Configured the chat to open in a separate, modeless window that user can move anywhere on the screen.
    *   Added `WindowChrome` for a modern look with corner radius and shadows.
3.  **Dragging Support**:
    *   Implemented a `MouseDown` handler in `ChatWidget` header to allow dragging the entire `ChatWindow`.
4.  **Integration**:
    *   Updated `DashboardView.xaml.cs` to launch `ChatWindow` instead of toggling an internal overlay.
    *   Registered `ChatWindow` and `ChatWidget` in the DI container (`App.xaml.cs`).
5.  **New Converter**:
    *   Created `BoolToVisibilityConverter.cs` to handle conditional visibility (e.g., hiding redundant user names in chat messages).

### Verification
- Ran `dotnet build`: Success.
- Logic verified: Chat now appears in a separate window, can be dragged, and closed independently.

### Status
Complete / Đã hoàn thành.
