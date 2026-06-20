# Toast-Only Notification Mode Report
## Date: 2026-02-08 00:05 (Local Time)

### Problem Description
The user requested a specific behavior for notifications on the web server:
1.  **No Persistent List**: "Notifications không cập nhận nội dung từ admin" - interpreted as the notification panel list should not be populated or updated with persistent content.
2.  **Quick Alerts Only**: "chỉ hiển thị thông báo nhanh" - interpreted as showing transient "Toast" notifications when new events occur.
3.  **No Badge/Icon**: "không hiện biểu tượng vàng thông báo đến" - interpreted as hiding the notification badge/counter on the bell icon.

### Changes Made
1.  **Modified `notification.js`**:
    *   **Tracking**: Introduced `seenNotificationIds` (Set) and `isFirstLoad` (Boolean) to track which notifications have been processed.
    *   **Fetching Logic**:
        *   On each fetch (10s interval), it filters for *new* notification IDs that aren't in the `seenNotificationIds` set.
        *   On `isFirstLoad`, it simply marks all existing notifications as seen *without* showing toasts (to prevent spamming old alerts on page refresh).
        *   On subsequent fetches, any new notification triggers `showToast(message, type)`.
    *   **Visual Updates**:
        *   **Badge**: Explicitly hides the `#notificationBadge` element (`display: none`).
        *   **Panel List**: Explicitly sets the `#notificationList` content to a static "handled via quick alerts" message, effectively disabling the list view.
        *   **Stubs**: Replaced `updateNotificationBadge` and `renderNotificationList` functions with empty stubs to prevent any specific logic from re-enabling them.

### Verification
*   **Visual**: The red badge on the bell icon should disappear. The notification panel list will be empty.
*   **Behavior**: When a new notification is sent (e.g., from Admin Panel), a "Toast" popup should appear briefly on the user's dashboard, but the bell icon will remain unchanged.

### Status
Complete / Đã hoàn thành.
