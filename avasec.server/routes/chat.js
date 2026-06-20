const express = require('express');
const router = express.Router();

// In-memory storage for demo purposes
// Structure: { sessionId: { SessionId, UserId, UserName, LastActive, UnreadCount, Messages: [] } }
const chatSessions = {};

// Helper to get or create session
const getSession = (userId, userName) => {
    const sessionId = `session_${userId}`;
    if (!chatSessions[sessionId]) {
        chatSessions[sessionId] = {
            SessionId: sessionId,
            UserId: userId,
            UserName: userName || `User ${userId}`,
            LastActive: new Date(),
            UnreadCount: 0,
            Messages: []
        };
    }
    return chatSessions[sessionId];
};

// Initialize some demo data
const demoSessionId = 'session_1';
chatSessions[demoSessionId] = {
    SessionId: demoSessionId,
    UserId: '1',
    UserName: 'DemoUser',
    LastActive: new Date(Date.now() - 1000 * 60 * 5),
    UnreadCount: 2,
    Messages: [
        { Message: 'Hello, I need help with my antivirus.', IsAgent: false, CreatedAt: new Date(Date.now() - 1000 * 60 * 60) },
        { Message: 'Sure, what seems to be the problem?', IsAgent: true, CreatedAt: new Date(Date.now() - 1000 * 60 * 55) },
        { Message: 'It says my license is expired but I just bought it.', IsAgent: false, CreatedAt: new Date(Date.now() - 1000 * 60 * 10) },
        { Message: 'Can you check specifically for user ID 1?', IsAgent: false, CreatedAt: new Date(Date.now() - 1000 * 60 * 5) }
    ]
};

// Get all chat sessions (for admin panel)
router.get('/sessions', async (req, res) => {
    try {
        const sessions = Object.values(chatSessions).sort((a, b) => new Date(b.LastActive) - new Date(a.LastActive));

        // Return simplified list for sidebar
        const sessionList = sessions.map(s => ({
            SessionId: s.SessionId,
            UserName: s.UserName,
            LastActive: s.LastActive,
            UnreadCount: s.UnreadCount,
            TotalMessages: s.Messages.length
        }));

        res.json({
            success: true,
            data: sessionList
        });
    } catch (error) {
        console.error('Error fetching chat sessions:', error);
        res.status(500).json({ success: false, message: 'Failed to fetch chat sessions' });
    }
});

// Get chat history for a session
router.get('/history/:sessionId', async (req, res) => {
    try {
        const { sessionId } = req.params;
        const session = chatSessions[sessionId];

        if (!session) {
            return res.json({ success: false, message: 'Session not found', data: [] });
        }

        res.json({
            success: true,
            data: session.Messages
        });
    } catch (error) {
        console.error('Error fetching chat history:', error);
        res.status(500).json({ success: false, message: 'Failed to fetch chat history' });
    }
});

// Send a message
router.post('/send', async (req, res) => {
    try {
        const { sessionId, userId, userName, message, isAgent } = req.body;

        // If coming from user, we might need to create session if not exists
        // But for this demo, we assume admin is replying to existing session or user creates one 
        // For admin replay, sessionId is required.

        // If coming from admin replying
        let targetSessionId = sessionId;

        if (!targetSessionId && userId) {
            targetSessionId = `session_${userId}`;
        }

        const session = getSession(userId, userName);

        const newMessage = {
            Message: message,
            IsAgent: isAgent || false,
            CreatedAt: new Date()
        };

        session.Messages.push(newMessage);
        session.LastActive = new Date();

        // If user sent it, increment unread count for admin
        if (!isAgent) {
            session.UnreadCount += 1;
        }

        // --- Real-time Notification Logic ---
        const io = req.app.get('socketio');
        if (io) {
            if (isAgent) {
                // Admin sent message -> Notify User
                // Target room: user_{userId}
                io.to(`user_${session.UserId}`).emit('receive_message', {
                    from: 'Admin',
                    content: message,
                    timestamp: new Date()
                });
            } else {
                // User sent message -> Notify All Admins
                // Target room: admins
                io.to('admins').emit('receive_message', {
                    fromUserId: session.UserId,
                    fromUsername: session.UserName,
                    content: message,
                    timestamp: new Date()
                });
            }
        }

        res.json({
            success: true,
            message: 'Message sent successfully',
            data: newMessage
        });
    } catch (error) {
        console.error('Error sending message:', error);
        res.status(500).json({ success: false, message: 'Failed to send message' });
    }
});

// Mark session as read
router.post('/mark-read', async (req, res) => {
    try {
        const { sessionId } = req.body;
        if (chatSessions[sessionId]) {
            chatSessions[sessionId].UnreadCount = 0;
            res.json({ success: true, message: 'Marked as read' });
        } else {
            res.status(404).json({ success: false, message: 'Session not found' });
        }
    } catch (error) {
        console.error('Error marking read:', error);
        res.status(500).json({ success: false });
    }
});

module.exports = router;
