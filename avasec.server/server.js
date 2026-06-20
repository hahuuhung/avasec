const path = require('path');
const fs = require('fs');
require('dotenv').config();
const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const morgan = require('morgan');
const compression = require('compression');
const rateLimit = require('express-rate-limit');

const app = express();
const PORT = process.env.PORT || 3001;

// Security Middleware / Middleware Bảo mật
app.use(helmet({
    contentSecurityPolicy: false, // Allow inline scripts for demo UI
}));

// Rate Limiting / Giới hạn tốc độ
const limiter = rateLimit({
    windowMs: 15 * 60 * 1000, // 15 minutes
    max: 5000, // limit each IP to 5000 requests per windowMs (Increased for polling)
    message: {
        success: false,
        message: "Too many requests, please try again later. / Quá nhiều yêu cầu, vui lòng thử lại sau."
    }
});
app.use('/api', limiter);

// Performance Middleware / Middleware Hiệu năng
app.use(compression()); // Gzip compression
app.use(cors());
app.use(morgan('dev'));
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Admin UI — không cache (tránh trình duyệt giữ bản cũ không có tab Người dùng)
app.use((req, res, next) => {
    if (req.path === '/admin.html' || req.path === '/admin.js') {
        res.set('Cache-Control', 'no-store, no-cache, must-revalidate');
    }
    next();
});

app.use(express.static(path.join(__dirname, 'public'), {
    maxAge: '1d' // Cache static files for 1 day
}));

// Database Connection check
const db = require('./config/db.config');

// Import Routes
app.use('/api', require('./routes/api'));
app.use('/api/license', require('./routes/license'));
app.use('/api/auth', require('./routes/auth'));
app.use('/api/StatusReporting', require('./routes/status'));
app.use('/api/chat', require('./routes/chat'));

// Health Check / Kiểm tra trạng thái
app.get('/api/health', (req, res) => {
    res.json({
        status: 'UP',
        timestamp: new Date().toISOString(),
        system: 'AVA Security Server',
        version: '1.0.0'
    });
});

// SPA Fallback / Hỗ trợ SPA
app.get('*', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// 404 Handler / Xử lý lỗi 404
app.use((req, res, next) => {
    res.status(404).json({
        success: false,
        message: 'Endpoint not found / Không tìm thấy đường dẫn'
    });
});

// Global Error Handler / Xử lý lỗi toàn cục
app.use((err, req, res, next) => {
    console.error('Server Error:', err);
    // Log to file
    const logMessage = `[${new Date().toISOString()}] ${err.stack || err.message}\n`;
    fs.appendFile(path.join(__dirname, 'startup_error.txt'), logMessage, (e) => {
        if (e) console.error('Failed to write to log file:', e);
    });

    res.status(500).json({
        success: false,
        message: 'Internal Server Error / Lỗi máy chủ nội bộ',
        error: process.env.NODE_ENV === 'development' ? err.message : undefined
    });
});

// Socket.IO Setup
const http = require('http');
const { Server } = require("socket.io");
const server = http.createServer(app);
const io = new Server(server, {
    cors: {
        origin: "*", // Allow all origins for development
        methods: ["GET", "POST"]
    }
});

// Make io accessible to our router
app.set('socketio', io);

// Initialize Chat Service
try {
    const chatService = require('./services/ChatService');
    if (typeof chatService === 'function') {
        chatService(io);
    }
} catch (e) {
    console.error('Failed to load ChatService:', e.message);
}

server.listen(PORT, () => {
    console.log(`\n--------------------------------------------------------------`);
    console.log(`🚀 AVA Security Server running at http://localhost:${PORT}`);
    console.log(`📡 API Interface: http://localhost:${PORT}/api/health`);
    console.log(`💬 Socket.IO: Enabled`);
    console.log(`🔒 Security: Helmet, RateLimit, CORS Enabled in ${process.env.NODE_ENV || 'development'} mode`);
    console.log(`--------------------------------------------------------------\n`);
});

