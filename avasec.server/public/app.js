// API Base URL - Để trống nếu web và API chạy cùng host
// API Base URL - Leave empty if web and API run on the same host
const API_BASE = '';

const USER_STORAGE_KEY = 'avasec_user';
(function migrateLegacyUserKey() {
    const legacy = localStorage.getItem('sysanti_user');
    if (legacy && !localStorage.getItem(USER_STORAGE_KEY)) {
        localStorage.setItem(USER_STORAGE_KEY, legacy);
        localStorage.removeItem('sysanti_user');
    }
})();

// DOM Elements / Các phần tử DOM
const tabBtns = document.querySelectorAll('.tab-btn');
const tabContents = document.querySelectorAll('.tab-content');
const loginForm = document.getElementById('loginForm');
const registerForm = document.getElementById('registerForm');
const toast = document.getElementById('toast');

// Tab Switching / Chuyển đổi tab
if (tabBtns.length > 0) {
    tabBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const tabId = btn.dataset.tab;

            // Update buttons
            tabBtns.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');

            // Update content
            tabContents.forEach(content => {
                content.classList.remove('active');
                if (content.id === tabId) {
                    content.classList.add('active');
                }
            });
        });
    });
}

// Show Toast / Hiển thị thông báo - ENHANCED VERSION
function showToast(message, type = 'success', duration = 4000) {
    if (!message) message = (type === 'error') ? 'Unknown Error / Lỗi không xác định' : 'Operation completed';

    // Create toast container if it doesn't exist
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        document.body.appendChild(toastContainer);
    }

    // Icons for different types
    const icons = {
        'success': '<svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor"><path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"/></svg>',
        'error': '<svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor"><path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/></svg>',
        'warning': '<svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor"><path d="M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z"/></svg>',
        'info': '<svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor"><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z"/></svg>'
    };

    // Create individual toast element
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    const icon = icons[type] || icons['info'];

    toast.innerHTML = `
        <div class="toast-content">
            <span class="toast-icon">${icon}</span>
            <span class="toast-message">${message}</span>
        </div>
        <button class="toast-close" onclick="this.parentElement.remove()">&times;</button>
    `;

    toastContainer.appendChild(toast);

    // Trigger animation
    requestAnimationFrame(() => {
        toast.classList.add('show');
    });

    // Auto remove
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => {
            if (toast.parentElement) toast.remove();
        }, 300); // Wait for transition to finish
    }, duration);
}

// Global Logout Function
function logout() {
    localStorage.removeItem(USER_STORAGE_KEY);
    window.location.href = '/index.html';
}
window.logout = logout;

// Mobile Menu Toggle
function toggleMobileMenu() {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');
    if (sidebar && overlay) {
        sidebar.classList.toggle('open');
        overlay.classList.toggle('active');
    }
}
window.toggleMobileMenu = toggleMobileMenu;

// Login Handler / Xử lý đăng nhập
if (loginForm) {
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const username = document.getElementById('loginUsername').value;
        const password = document.getElementById('loginPassword').value;
        const submitBtn = loginForm.querySelector('button[type="submit"]');

        submitBtn.disabled = true;
        submitBtn.textContent = '⏳ Đang xử lý...';

        try {
            const response = await fetch(`${API_BASE}/api/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });

            const data = await response.json();

            if (data.success) {
                showToast(data.message, 'success');
                localStorage.setItem(USER_STORAGE_KEY, JSON.stringify({
                    userId: data.userId,
                    username: username
                }));
                setTimeout(() => {
                    showToast('Đang chuyển hướng... / Redirecting...', 'success');
                    window.location.href = '/dashboard.html';
                }, 1000);
            } else {
                showToast(data.message, 'error');
            }
        } catch (error) {
            showToast('Lỗi kết nối Server / Connection Error', 'error');
            console.error('Login error:', error);
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = '🔓 Đăng nhập (Login)';
        }
    });
}

// Register Handler / Xử lý đăng ký
if (registerForm) {
    registerForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const username = document.getElementById('regUsername').value;
        const email = document.getElementById('regEmail').value;
        const password = document.getElementById('regPassword').value;
        const submitBtn = registerForm.querySelector('button[type="submit"]');

        submitBtn.disabled = true;
        submitBtn.textContent = '⏳ Đang xử lý...';

        try {
            const response = await fetch(`${API_BASE}/api/auth/register`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password, email })
            });

            const data = await response.json();

            if (data.success) {
                showToast(data.message, 'success');
                setTimeout(() => {
                    // Switch to login tab if available
                    if (tabBtns.length > 0) {
                        tabBtns[0].click();
                        const loginUser = document.getElementById('loginUsername');
                        if (loginUser) loginUser.value = username;
                    }
                }, 1500);
            } else {
                showToast(data.message, 'error');
            }
        } catch (error) {
            showToast('Lỗi kết nối Server / Connection Error', 'error');
            console.error('Register error:', error);
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = '✨ Đăng ký (Register)';
        }
    });
}

// Check if already logged in (Redirect logic)
document.addEventListener('DOMContentLoaded', () => {
    // Only check redirect if we are on login page
    if (window.location.pathname === '/' || window.location.pathname === '/index.html') {
        const user = localStorage.getItem(USER_STORAGE_KEY);
        if (user) {
            window.location.href = '/dashboard.html';
        }
    }
});

/* =========================================
   Web Chat Logic (New Feature)
   ========================================= */

document.addEventListener('DOMContentLoaded', () => {
    // Only initialize chat on dashboard/authenticated pages, NOT on login/register pages
    // Dashboard has its own chat-widget.js, so exclude it too
    const isLoginPage = window.location.pathname === '/' ||
        window.location.pathname === '/index.html' ||
        window.location.pathname === '/donate.html' ||
        window.location.pathname === '/dashboard.html';

    if (isLoginPage) {
        return; // Don't initialize chat on login/register/donate/dashboard pages
    }

    // Check if we are logged in to initialize chat
    const userStr = localStorage.getItem(USER_STORAGE_KEY);
    if (userStr && typeof io !== 'undefined') {
        initChat(JSON.parse(userStr));
    }
});

function initChat(user) {
    const socket = io('/', {
        query: {
            userId: user.userId,
            username: user.username,
            role: 'user' // Default to user role
        }
    });

    // Create Chat Elements
    const chatWidget = document.createElement('div');
    chatWidget.className = 'chat-widget';
    chatWidget.innerHTML = `
        <div class="chat-header">
            <div class="chat-status">
                <span class="chat-status-indicator"></span>
                <span>Admin Support</span>
            </div>
            <div class="chat-controls">
                <button id="refresh-chat" title="Refresh Messages">↻</button>
                <button id="minimize-chat">_</button>
            </div>
        </div>
        <div class="chat-body" id="chat-messages">
            <div class="chat-message admin">
                Hello ${user.username}! How can we help you today? / Xin chào, chúng tôi có thể giúp gì cho bạn?
                <span class="chat-message-time">${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
            </div>
        </div>
        <div class="chat-input-area">
            <input type="text" id="chat-input" class="chat-input" placeholder="Type message...">
            <button id="send-chat" class="chat-send-btn">➤</button>
        </div>
    `;
    document.body.appendChild(chatWidget);

    // Create Floating Toggle Button
    const toggleBtn = document.createElement('button');
    toggleBtn.className = 'chat-toggle-btn';
    toggleBtn.innerHTML = '💬';
    document.body.appendChild(toggleBtn);

    // Elements
    const messagesContainer = document.getElementById('chat-messages');
    const input = document.getElementById('chat-input');
    const sendBtn = document.getElementById('send-chat');
    const minBtn = document.getElementById('minimize-chat');
    const refreshBtn = document.getElementById('refresh-chat');

    // Toggle Visibility
    toggleBtn.addEventListener('click', () => {
        chatWidget.classList.add('active');
        toggleBtn.classList.add('hidden');
        // Remove badge if exists
        const badge = toggleBtn.querySelector('.chat-badge');
        if (badge) badge.remove();
        input.focus();
        loadHistory(); // Auto-load on open
    });

    minBtn.addEventListener('click', () => {
        chatWidget.classList.remove('active');
        toggleBtn.classList.remove('hidden');
    });

    // Refresh History
    refreshBtn.addEventListener('click', () => {
        loadHistory();
    });

    async function loadHistory() {
        refreshBtn.classList.add('spinning'); // Add CSS class for rotation if desired
        try {
            const sessionId = `session_${user.userId}`;
            const response = await fetch(`${API_BASE}/api/chat/history/${sessionId}`);
            const res = await response.json();

            if (res.success && res.data && res.data.length > 0) {
                messagesContainer.innerHTML = ''; // Clear default
                res.data.forEach(msg => {
                    addMessage(msg.Message, msg.IsAgent ? 'admin' : 'user', msg.CreatedAt);
                });
                messagesContainer.scrollTop = messagesContainer.scrollHeight;
            }
        } catch (error) {
            console.error('Failed to load chat history', error);
        } finally {
            refreshBtn.classList.remove('spinning');
        }
    }

    // Send Message
    function sendMessage() {
        const content = input.value.trim();
        if (!content) return;

        // Emit to server
        socket.emit('send_message', { content });

        // Add to UI
        addMessage(content, 'user', new Date());
        input.value = '';
    }

    sendBtn.addEventListener('click', sendMessage);
    input.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') sendMessage();
    });

    // Sound Notification
    function playNotificationSound() {
        try {
            const ctx = new (window.AudioContext || window.webkitAudioContext)();
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            osc.connect(gain);
            gain.connect(ctx.destination);

            osc.type = 'sine';
            osc.frequency.setValueAtTime(880, ctx.currentTime);
            osc.frequency.exponentialRampToValueAtTime(440, ctx.currentTime + 0.1);

            gain.gain.setValueAtTime(0.3, ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.3);

            osc.start();
            osc.stop(ctx.currentTime + 0.3);
        } catch (e) { console.error('Audio error', e); }
    }

    // Receive Message
    socket.on('receive_message', (data) => {
        // Play Sound
        playNotificationSound();

        addMessage(data.content, 'admin');

        // If widget is closed, show badge
        if (!chatWidget.classList.contains('active')) {
            let badge = toggleBtn.querySelector('.chat-badge');
            if (!badge) {
                badge = document.createElement('span');
                badge.className = 'chat-badge';
                badge.innerText = '1';
                toggleBtn.appendChild(badge);
            } else {
                badge.innerText = parseInt(badge.innerText) + 1;
            }
        }
    });

    function addMessage(text, type, timestamp = new Date()) {
        const timeObj = new Date(timestamp);
        const timeStr = timeObj.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        const div = document.createElement('div');
        div.className = `chat-message ${type}`;
        div.innerHTML = `
            ${text}
            <span class="chat-message-time">${timeStr}</span>
        `;
        messagesContainer.appendChild(div);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }
}
