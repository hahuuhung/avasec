
// AVA Security Web Chat Widget
// Nhúng vào bất kỳ trang nào để bật tính năng chat với Admin
// Sử dụng Browser ID (localStorage) làm định danh

(function () {
    // 1. Session Management
    const STORAGE_KEY = 'avasec_chat_session_id';

    function getOrCreateSessionId() {
        let sessionId = localStorage.getItem(STORAGE_KEY);
        if (!sessionId) {
            sessionId = 'web_' + Math.random().toString(36).substr(2, 9) + '_' + Date.now().toString(36);
            localStorage.setItem(STORAGE_KEY, sessionId);
        }
        return sessionId;
    }

    const sessionId = getOrCreateSessionId();
    const userId = sessionId; // Use session ID as User ID for guests
    const userName = 'Guest Web User';
    let isChatOpen = false;
    let pollingInterval = null;
    let lastMessageId = 0;

    // 2. Inject Styles
    const style = document.createElement('style');
    style.innerHTML = `
        .avasec-chat-widget {
            position: fixed;
            bottom: 20px;
            right: 20px;
            z-index: 9999;
            font-family: 'Inter', sans-serif;
        }
        
        /* Toggle Button */
        .avasec-chat-btn {
            width: 60px;
            height: 60px;
            border-radius: 50%;
            background: linear-gradient(135deg, #6366f1 0%, #a855f7 100%);
            box-shadow: 0 4px 14px rgba(99, 102, 241, 0.4);
            border: none;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: transform 0.2s;
        }
        .avasec-chat-btn:hover { transform: scale(1.05); }
        .avasec-chat-btn svg { width: 30px; height: 30px; fill: white; }
        
        /* Chat Window */
        .avasec-chat-window {
            position: absolute;
            bottom: 80px;
            right: 0;
            width: 350px;
            height: 500px;
            background: #0f172a;
            border: 1px solid rgba(255,255,255,0.1);
            border-radius: 16px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.5);
            display: none;
            flex-direction: column;
            overflow: hidden;
            opacity: 0;
            transform: translateY(20px);
            transition: opacity 0.3s, transform 0.3s;
        }
        .avasec-chat-window.open {
            display: flex;
            opacity: 1;
            transform: translateY(0);
        }
        
        /* Header */
        .avasec-chat-header {
            background: linear-gradient(135deg, #6366f1 0%, #a855f7 100%);
            padding: 16px;
            color: white;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        .avasec-chat-header h4 { margin: 0; font-size: 16px; font-weight: 600; }
        .avasec-chat-header p { margin: 2px 0 0; font-size: 12px; opacity: 0.8; }
        .avasec-close-btn { background: none; border: none; color: white; cursor: pointer; font-size: 18px; }
        
        /* Messages Body */
        .avasec-chat-body {
            flex: 1;
            padding: 16px;
            overflow-y: auto;
            background: #1e293b;
            display: flex;
            flex-direction: column;
            gap: 12px;
        }
        
        /* Message Bubbles */
        .avasec-msg {
            max-width: 80%;
            padding: 10px 14px;
            border-radius: 12px;
            font-size: 14px;
            line-height: 1.4;
            position: relative;
            word-wrap: break-word;
        }
        .avasec-msg.user {
            align-self: flex-end;
            background: #3b82f6;
            color: white;
            border-bottom-right-radius: 2px;
        }
        .avasec-msg.agent {
            align-self: flex-start;
            background: #334155;
            color: #e2e8f0;
            border-bottom-left-radius: 2px;
        }
        .avasec-msg.msg-pending {
            opacity: 0.7;
        }
        .avasec-msg-time {
            font-size: 10px;
            opacity: 0.6;
            margin-top: 4px;
            display: block;
            text-align: right;
        }
        
        /* Footer Input */
        .avasec-chat-footer {
            padding: 12px;
            border-top: 1px solid rgba(255,255,255,0.1);
            background: #0f172a;
            display: flex;
            gap: 8px;
        }
        .avasec-chat-input {
            flex: 1;
            background: #1e293b;
            border: 1px solid rgba(255,255,255,0.1);
            border-radius: 20px;
            padding: 10px 16px;
            color: white;
            outline: none;
            font-size: 14px;
        }
        .avasec-chat-input:focus { border-color: #6366f1; }
        .avasec-send-btn {
            background: #6366f1;
            color: white;
            border: none;
            width: 40px;
            height: 40px;
            border-radius: 50%;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .avasec-send-btn:hover { background: #4f46e5; }
        
        /* Badge */
        .avasec-chat-badge {
            position: absolute;
            top: -5px;
            right: -5px;
            background: #ef4444;
            color: white;
            border-radius: 50%;
            width: 20px;
            height: 20px;
            font-size: 11px;
            display: flex;
            align-items: center;
            justify-content: center;
            border: 2px solid #0f172a;
            border: 2px solid #0f172a;
            display: none;
        }

        @keyframes spin { 100% { transform: rotate(360deg); } }
    `;
    document.head.appendChild(style);

    // 3. Create Widget HTML
    const widget = document.createElement('div');
    widget.className = 'avasec-chat-widget';
    widget.innerHTML = `
        <div class="avasec-chat-window" id="avasecChatWindow">
            <div class="avasec-chat-header">
                <div>
                    <h4>AVA Security Support</h4>
                    <p>Hỗ trợ trực tuyến / Live chat</p>
                </div>
                <div style="display:flex; gap:8px;">
                     <button class="avasec-close-btn" id="avasecRefreshBtn" title="Làm mới tin nhắn" style="font-size: 14px;">↻</button>
                     <button class="avasec-close-btn" id="avasecCloseBtn">✕</button>
                </div>
            </div>
            <div class="avasec-chat-body" id="avasecChatBody">
                <div style="text-align: center; color: #64748b; font-size: 12px; margin-top: 20px;">
                    Bắt đầu cuộc trò chuyện...
                </div>
            </div>
            <div class="avasec-chat-footer">
                <input type="text" class="avasec-chat-input" id="avasecChatInput" placeholder="Nhập tin nhắn..." autocomplete="off">
                <button class="avasec-send-btn" id="avasecSendBtn">
                    <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="22" y1="2" x2="11" y2="13"></line><polygon points="22 2 15 22 11 13 2 9 22 2"></polygon></svg>
                </button>
            </div>
        </div>
        <button class="avasec-chat-btn" id="avasecChatBtn">
            <svg viewBox="0 0 24 24"><path d="M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2z"></path></svg>
            <div class="avasec-chat-badge" id="avasecChatBadge">0</div>
        </button>
    `;
    document.body.appendChild(widget);

    // 4. Logic Implementation
    const chatBtn = document.getElementById('avasecChatBtn');
    const chatWindow = document.getElementById('avasecChatWindow');
    const closeBtn = document.getElementById('avasecCloseBtn');
    const refreshBtn = document.getElementById('avasecRefreshBtn');
    const chatBody = document.getElementById('avasecChatBody');
    const input = document.getElementById('avasecChatInput');
    const sendBtn = document.getElementById('avasecSendBtn');
    const badge = document.getElementById('avasecChatBadge');

    function toggleChat() {
        isChatOpen = !isChatOpen;
        if (isChatOpen) {
            chatWindow.classList.add('open');
            chatBtn.style.display = 'none'; // Optional: hide btn when open
            badge.style.display = 'none';
            scrollToBottom();
            // Force reload on open to ensure fresh data
            loadHistory();
            if (!pollingInterval) startPolling();
        } else {
            chatWindow.classList.remove('open');
            chatBtn.style.display = 'flex';
        }
    }

    // Refresh Logic
    refreshBtn.onclick = () => {
        refreshBtn.style.animation = 'spin 1s linear infinite';
        loadHistory().then(() => {
            refreshBtn.style.animation = '';
        });
    };

    // Close Logic
    closeBtn.onclick = () => {
        isChatOpen = false;
        chatWindow.classList.remove('open');
        chatBtn.style.display = 'flex';
    };

    chatBtn.onclick = toggleChat;

    async function sendMessage() {
        const msgText = input.value.trim();
        if (!msgText) return;

        // UI Optimistic Update
        const tempId = 'temp-' + Date.now();

        // Only append if it's not already in the list (very unlikely for new message, but safety first)
        // Actually, for duplication, we need to ensure polling doesn't add it again if we have it as pending.

        appendMessage({
            Message: msgText,
            IsAgent: false,
            CreatedAt: new Date().toISOString(),
            _tempId: tempId
        }, true); // true = isPending

        input.value = '';
        scrollToBottom();

        try {
            const res = await fetch('/api/chat/send', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    sessionId: sessionId,
                    userId: userId,
                    userName: userName,
                    message: msgText,
                    isAgent: false
                })
            });
            const data = await res.json();

            if (data.success && data.messageId) {
                // Determine if we need to update lastMessageId
                if (data.messageId > lastMessageId) {
                    lastMessageId = data.messageId;
                }

                // Promote the pending message to a real one immediately
                // This prevents race condition if polling happens right now
                promoteMessage(tempId, data.messageId);
            }
        } catch (e) {
            console.error('Failed to send message:', e);
            // Optionally mark message as failed in UI
            const pendingEl = document.getElementById(tempId);
            if (pendingEl) pendingEl.style.opacity = '0.5';
        }
    }

    sendBtn.onclick = sendMessage;
    input.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') sendMessage();
    });

    function promoteMessage(tempId, realId) {
        const el = document.getElementById(tempId);
        if (el) {
            el.id = 'msg-' + realId;
            el.dataset.messageId = realId;
            el.classList.remove('msg-pending');
            // Remove spinner or pending style if any
        }
    }

    function appendMessage(msg, isPending = false) {
        // De-duplication check using DOM
        // If we already have a message with this real ID, skip
        if (msg.MessageId && document.getElementById('msg-' + msg.MessageId)) {
            return;
        }

        // If this is an incoming real message (from Agent or Self via Polling)
        // We should check if we have a pending message that matches it to promote it
        if (!isPending && !msg.IsAgent && msg.MessageId) {
            // Find a pending message with the same content
            // Using reverse search to find the most recent one
            const pendingParams = Array.from(chatBody.querySelectorAll('.msg-pending'));
            const pending = pendingParams.reverse().find(el => el.innerText.includes(msg.Message));

            if (pending) {
                pending.id = 'msg-' + msg.MessageId;
                pending.dataset.messageId = msg.MessageId;
                pending.classList.remove('msg-pending');
                // Also update the time if needed, but usually fine

                // Update ID tracker
                if (msg.MessageId > lastMessageId) lastMessageId = msg.MessageId;
                return; // Converted pending to real, done. STOP HERE.
            }
        }

        // Update Tracker
        if (msg.MessageId && msg.MessageId > lastMessageId) {
            lastMessageId = msg.MessageId;
        }

        const div = document.createElement('div');
        div.className = `avasec-msg ${msg.IsAgent ? 'agent' : 'user'} ${isPending ? 'msg-pending' : ''}`;
        if (isPending && msg._tempId) div.id = msg._tempId;
        if (msg.MessageId) {
            div.id = 'msg-' + msg.MessageId;
            div.dataset.messageId = msg.MessageId;
        }

        const time = new Date(msg.CreatedAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        div.innerHTML = `
            ${msg.Message}
            <span class="avasec-msg-time">${time}</span>
        `;
        chatBody.appendChild(div);
    }

    function scrollToBottom() {
        chatBody.scrollTop = chatBody.scrollHeight;
    }

    async function loadHistory() {
        try {
            const res = await fetch(`/api/chat/history/${sessionId}`);
            const result = await res.json();
            if (result.success) {
                chatBody.innerHTML = ''; // Clear initial placeholder
                // Add Welcome Message if empty
                if (result.data.length === 0) {
                    // Manual append, no ID
                    const welcomeDiv = document.createElement('div');
                    welcomeDiv.className = 'avasec-msg agent';
                    welcomeDiv.innerHTML = `Xin chào! Tôi có thể giúp gì cho bạn hôm nay?<span class="avasec-msg-time">${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>`;
                    chatBody.appendChild(welcomeDiv);
                } else {
                    result.data.forEach(m => appendMessage(m));
                    // Update lastId
                    const maxId = result.data.reduce((max, m) => Math.max(max, m.MessageId), 0);
                    lastMessageId = maxId;
                }
                scrollToBottom();
            }
        } catch (e) { console.error(e); }
    }

    function startPolling() {
        if (pollingInterval) clearInterval(pollingInterval);
        pollingInterval = setInterval(async () => {
            try {
                const res = await fetch(`/api/chat/poll/${sessionId}?lastId=${lastMessageId}`);
                const result = await res.json();
                if (result.success && result.data.length > 0) {
                    let hasNewMessage = false;
                    result.data.forEach(msg => {
                        appendMessage(msg); // Will handle dedupe/promotion internally
                        if (msg.IsAgent) hasNewMessage = true;
                    });
                    scrollToBottom();

                    // Show notification badge if chat is closed
                    if (!isChatOpen && hasNewMessage) {
                        badge.style.display = 'flex';
                        badge.textContent = '1'; // Or count unread
                    }
                }
            } catch (e) { console.error(e); }
        }, 3000); // Poll every 3s
    }

    // Initialize
    loadHistory();
    startPolling();

})();
