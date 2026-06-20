// Shared Notification Logic

function getUserId() {
    const userStr = localStorage.getItem('avasec_user');
    if (!userStr) return null;
    return JSON.parse(userStr).userId;
}

// State to track seen notifications
let seenNotificationIds = new Set();
let isFirstLoad = true;

async function fetchNotifications() {
    try {
        const userId = getUserId();
        if (!userId) return;

        const response = await fetch(`/api/notifications/${userId}`);
        const result = await response.json();

        if (result.success) {
            const notifications = result.data;

            // Filter for new notifications
            const newNotifications = notifications.filter(n => !seenNotificationIds.has(n.NotificationId));

            // Update seen set
            newNotifications.forEach(n => seenNotificationIds.add(n.NotificationId));

            // On first load, don't spam toasts, just acknowledge existing state
            if (isFirstLoad) {
                isFirstLoad = false;
                // Optional: Toast a welcome or summary
            } else {
                // Show toast for new notifications
                newNotifications.forEach(n => {
                    // Use existing showToast from app.js if available
                    if (typeof showToast === 'function') {
                        showToast(n.Message, n.NotificationType || 'info');
                    }
                });
            }

            // NEW: User wants "save in Notifications" (list) but "only show quick alerts" (Toast + No Badge)
            // So we re-enable the list population but keep the badge hidden, and use Toasts for new stuff.
            renderNotificationList(notifications);

            // Keep Badge Hidden as per previous strict request "không hiện biểu tượng vàng"
            const badge = document.getElementById('notificationBadge');
            if (badge) badge.style.display = 'none';

            // Check if there are unread notifications to update bell icon state
            const hasUnread = notifications.some(n => !n.IsRead);
            const bellIcon = document.getElementById('notificationBellIcon');
            if (bellIcon) {
                if (hasUnread) {
                    bellIcon.style.opacity = '1';
                    bellIcon.style.color = '#94A3B8'; // Normal
                } else {
                    bellIcon.style.opacity = '0.5'; // Dimmed
                    // bellIcon.style.color = '#475569'; // Optional darker color
                }
            }

            // Optional: Check for Promo (only if function exists - dashboard only)
            if (typeof checkForPromo === 'function') {
                checkForPromo(notifications);
            }
        }
    } catch (error) {
        // Suppress error logging to avoid flooding console during network issues
        // console.error('Error loading notifications:', error);
    }
}

function updateNotificationBadge(notifications) {
    // Disabled (User requested no badge)
}

function renderNotificationList(notifications) {
    const list = document.getElementById('notificationList');
    if (!list) return;

    if (notifications.length === 0) {
        list.innerHTML = `
            <div class="notification-empty">
                <i class="bi bi-bell-slash" style="font-size: 32px; color: #94A3B8;"></i>
            </div>`;
        return;
    }

    // Sort: Newest first
    notifications.sort((a, b) => new Date(b.CreatedAt) - new Date(a.CreatedAt));

    list.innerHTML = notifications.map(n => `
        <div class="notification-item ${n.IsRead ? '' : 'unread'}" onclick="handleNotificationClick(${n.NotificationId}, '${n.ActionUrl || ''}')">
            <div class="notification-content-wrapper">
                <div class="notification-icon-wrapper">
                    ${getNotificationIcon(n.NotificationType)}
                </div>
                <div style="flex: 1;">
                    <div class="notification-title">${n.Title}</div>
                    <div class="notification-message">${n.Message}</div>
                    <span class="notification-time">${getTimeAgo(new Date(n.CreatedAt))}</span>
                </div>
            </div>
        </div>
    `).join('');
}

function getNotificationIcon(type) {
    if (type === 'success') return '<i class="bi bi-check-circle-fill" style="color: #34D399;"></i>';
    if (type === 'warning') return '<i class="bi bi-exclamation-triangle-fill" style="color: #FBBF24;"></i>';
    if (type === 'error') return '<i class="bi bi-x-circle-fill" style="color: #EF4444;"></i>';
    return '<i class="bi bi-info-circle-fill" style="color: #60A5FA;"></i>';
}

function getTimeAgo(date) {
    const seconds = Math.floor((new Date() - date) / 1000);
    let interval = seconds / 31536000;
    if (interval > 1) return Math.floor(interval) + " năm trước";
    interval = seconds / 2592000;
    if (interval > 1) return Math.floor(interval) + " tháng trước";
    interval = seconds / 86400;
    if (interval > 1) return Math.floor(interval) + " ngày trước";
    interval = seconds / 3600;
    if (interval > 1) return Math.floor(interval) + " giờ trước";
    interval = seconds / 60;
    if (interval > 1) return Math.floor(interval) + " phút trước";
    return "Vừa xong";
}

let notificationTimer;

function toggleNotificationPanel(event) {
    if (event) event.stopPropagation(); // Prevent immediate close
    const panel = document.getElementById('notificationPanel');
    if (!panel) return;

    // Clear any existing timer
    if (notificationTimer) {
        clearTimeout(notificationTimer);
        notificationTimer = null;
    }

    const isVisible = panel.style.display === 'block';

    if (isVisible) {
        panel.style.display = 'none';
    } else {
        panel.style.display = 'block';
        // Auto-hide after 10 seconds
        notificationTimer = setTimeout(() => {
            const currentPanel = document.getElementById('notificationPanel');
            if (currentPanel) currentPanel.style.display = 'none';
        }, 10000);
    }
}

// Close panel when clicking outside
document.addEventListener('click', function (event) {
    const panel = document.getElementById('notificationPanel');
    if (!panel) return;

    if (panel.style.display === 'block' && !panel.contains(event.target)) {
        panel.style.display = 'none';
    }
});

function handleNotificationClick(id, url) {
    // Mark as read
    fetch(`/api/notifications/${id}/read`, { method: 'PUT' });

    // Visually mark as read
    const item = document.querySelector(`.notification-item[onclick*="${id}"]`);
    if (item) item.classList.remove('unread');

    // Update badge count locally
    const badge = document.getElementById('notificationBadge');
    if (badge && badge.style.display !== 'none') {
        let count = parseInt(badge.textContent);
        if (count > 0) {
            count--;
            badge.textContent = count;
            if (count === 0) badge.style.display = 'none';
        }
    }

    // Trigger immediate refresh to update dimmed status
    fetchNotifications();

    // Open URL if exists
    if (url && url !== 'null' && url !== 'undefined') {
        window.open(url, '_blank');
    }
}

async function markAllAsRead() {
    const userId = getUserId();
    if (!userId) return;

    try {
        await fetch(`/api/notifications/read-all/${userId}`, { method: 'PUT' });

        const badge = document.getElementById('notificationBadge');
        if (badge) badge.style.display = 'none';

        // Remove unread styles
        document.querySelectorAll('.notification-item.unread').forEach(el => el.classList.remove('unread'));

        // Immediately update bell icon state to dimmed
        const bellIcon = document.getElementById('notificationBellIcon');
        if (bellIcon) bellIcon.style.opacity = '0.5';

    } catch (error) {
        console.error('Error marking all as read:', error);
    }
}
