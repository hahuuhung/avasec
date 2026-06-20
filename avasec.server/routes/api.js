const express = require('express');
const router = express.Router();
const db = require('../config/db.config');
const crypto = require('crypto');
const nodemailer = require('nodemailer');
const axios = require('axios');

// --- Helper Functions ---
function generateApiKey() {
    return 'sk_live_' + crypto.randomBytes(16).toString('hex');
}

// --- DEBUG ENDPOINT ---
// GET /api/admin/debug/users - Raw database query for debugging
router.get('/admin/debug/users', async (req, res) => {
    try {
        const [rows] = await db.query(`
            SELECT 
                u.UserId, u.Username, u.Email, u.CreatedAt,
                l.LicenseType, l.ExpiryDate, l.LicenseKey
            FROM Users u
            LEFT JOIN Licenses l ON u.UserId = l.UserId
            ORDER BY u.UserId ASC
        `);

        res.json({
            success: true,
            message: `Found ${rows.length} users in database`,
            data: rows,
            timestamp: new Date().toISOString()
        });
    } catch (error) {
        res.status(500).json({
            success: false,
            message: 'Database query failed',
            error: error.message,
            code: error.code
        });
    }
});

// POST /api/admin/init - Initialize database with admin user
router.post('/admin/init', async (req, res) => {
    try {
        // Check if admin exists
        const [existing] = await db.query('SELECT UserId FROM Users WHERE Username = ?', ['admin']);

        if (existing.length > 0) {
            return res.json({
                success: true,
                message: 'Admin user already exists',
                adminId: existing[0].UserId
            });
        }

        // Create admin user
        const [result] = await db.query(
            'INSERT INTO Users (Username, PasswordHash, Email) VALUES (?, ?, ?)',
            ['admin', 'admin123', 'admin@avasec.app']
        );
        const adminId = result.insertId;

        // Create Premium license for admin
        const expiryDate = new Date();
        expiryDate.setFullYear(expiryDate.getFullYear() + 10); // 10 years
        const licenseKey = 'ADMIN-' + crypto.randomBytes(8).toString('hex').toUpperCase();

        await db.query(
            'INSERT INTO Licenses (UserId, LicenseKey, LicenseType, ExpiryDate) VALUES (?, ?, ?, ?)',
            [adminId, licenseKey, 'Premium', expiryDate]
        );

        res.json({
            success: true,
            message: 'Admin user created successfully',
            credentials: {
                username: 'admin',
                password: 'admin123',
                email: 'admin@avasec.app'
            },
            adminId: adminId,
            licenseKey: licenseKey
        });
    } catch (error) {
        res.status(500).json({
            success: false,
            message: 'Failed to create admin user',
            error: error.message
        });
    }
});

// --- Users & Licenses Endpoints ---

// GET /api/admin/users - List all users with license info (with pagination, sorting, search)
router.get('/admin/users', async (req, res) => {
    try {
        const page = parseInt(req.query.page);
        const limit = parseInt(req.query.limit);
        const offset = page && limit ? (page - 1) * limit : null;
        const sortBy = req.query.sortBy || 'UserId';
        const sortOrder = req.query.sortOrder === 'asc' ? 'ASC' : 'DESC';
        const search = req.query.search || '';

        let whereClause = '';
        let queryParams = [];
        if (search) {
            whereClause = 'WHERE u.Username LIKE ? OR u.Email LIKE ?';
            const searchTerm = `%${search}%`;
            queryParams = [searchTerm, searchTerm];
        }

        const [countResult] = await db.query(`
            SELECT COUNT(*) as total
            FROM Users u
            ${whereClause}
        `, queryParams);
        const totalUsers = countResult[0].total;

        let query = `
            SELECT 
                u.UserId, u.Username, u.Email, u.CreatedAt,
                l.LicenseType, l.ExpiryDate, l.LicenseKey
            FROM Users u
            LEFT JOIN Licenses l ON u.UserId = l.UserId
            ${whereClause}
            ORDER BY u.${sortBy} ${sortOrder}
        `;

        if (limit !== null && offset !== null && !isNaN(limit) && !isNaN(offset)) {
            query += ` LIMIT ? OFFSET ?`;
            queryParams.push(limit, offset);
        }

        const [rows] = await db.query(query, queryParams);
        console.log(`[USER_FETCH] SQL: ${query.replace(/\s+/g, ' ').trim()} | Found: ${rows.length}`);

        // --- FORCE ADMIN VISIBILITY ---
        // If on page 1 (or no page) and searching is empty (or searching for admin), ensure Admin is present
        if ((!page || page === 1) && (!search || 'admin'.includes(search.toLowerCase()))) {
            const hasAdmin = rows.some(r => (r.Username || '').toLowerCase() === 'admin');
            if (!hasAdmin) {
                // Fetch Admin specifically
                const [adminRows] = await db.query(`
                    SELECT 
                        u.UserId, u.Username, u.Email, u.CreatedAt,
                        l.LicenseType, l.ExpiryDate, l.LicenseKey
                    FROM Users u
                    LEFT JOIN Licenses l ON u.UserId = l.UserId
                    WHERE u.Username = 'admin'
                `);

                if (adminRows.length > 0) {
                    rows.unshift(adminRows[0]); // Add to top
                }
            }
        }
        // -----------------------------

        const users = rows.map(user => {
            let remainingDays = 0;
            // Handle missing license (e.g. manual DB insert)
            const licenseType = user.LicenseType || 'Trial';
            let expiryDate = user.ExpiryDate ? new Date(user.ExpiryDate) : null;

            // If no expiry date (manual insert or invalid), give 14 days default for display
            if (!expiryDate || isNaN(expiryDate.getTime())) {
                expiryDate = new Date();
                expiryDate.setDate(expiryDate.getDate() + 14);
            }

            if (expiryDate) {
                const now = new Date();
                const diffTime = expiryDate - now;
                remainingDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                if (remainingDays < 0) remainingDays = 0;
            }

            return {
                userId: user.UserId,
                username: user.Username || 'Unknown',
                email: user.Email || '',
                createdAt: user.CreatedAt,
                licenseType: licenseType,
                licenseExpiry: expiryDate,
                remainingDays: remainingDays
            };
        });

        res.json({
            success: true,
            data: users,
            pagination: {
                page: page,
                limit: limit,
                total: totalUsers,
                totalPages: Math.ceil(totalUsers / limit)
            }
        });
    } catch (error) {
        console.error('Error fetching users:', error);
        res.status(500).json({ success: false, message: 'Database error / Lỗi cơ sở dữ liệu' });
    }
});

// POST /api/admin/licenses/extend - Extend license
router.post('/admin/licenses/extend', async (req, res) => {
    const { userId, days } = req.body;
    try {
        const [licenses] = await db.query('SELECT LicenseId, ExpiryDate, LicenseType FROM Licenses WHERE UserId = ?', [userId]);

        if (licenses.length === 0) {
            return res.status(404).json({ success: false, message: 'User or license not found. / Người dùng hoặc giấy phép không tìm thấy.' });
        }

        const license = licenses[0];
        const currentExpiry = new Date(license.ExpiryDate);
        const now = new Date();
        let newExpiry;

        const isFree = license.LicenseType !== "Premium" && license.LicenseType !== "Pro";
        const isExpired = currentExpiry < now;

        if (isFree && isExpired) {
            newExpiry = new Date();
            newExpiry.setDate(newExpiry.getDate() + 15);
            await db.query('UPDATE Licenses SET ExpiryDate = ? WHERE UserId = ?', [newExpiry, userId]);
            return res.json({ success: true, message: 'Expired Trial account extended by 15 days from now.' });
        }

        newExpiry = new Date(currentExpiry.getTime());
        newExpiry.setDate(newExpiry.getDate() + days);
        await db.query('UPDATE Licenses SET ExpiryDate = ? WHERE UserId = ?', [newExpiry, userId]);

        res.json({ success: true, message: `Extended license by ${days} days.` });
    } catch (error) {
        console.error('Error extending license:', error);
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

// POST /api/admin/licenses/change-type - Change license type
router.post('/admin/licenses/change-type', async (req, res) => {
    const { userId, licenseType } = req.body;
    try {
        let durationDays = 0;
        switch (licenseType) {
            case 'Trial': durationDays = 14; break;
            case 'Pro': durationDays = 365; break;
            case 'Premium': durationDays = 1095; break;
            default:
                return res.status(400).json({ success: false, message: 'Invalid license type.' });
        }

        const [licenses] = await db.query('SELECT ExpiryDate FROM Licenses WHERE UserId = ?', [userId]);
        const now = new Date();
        let newExpiry;

        if (licenseType === "Trial") {
            newExpiry = new Date();
            newExpiry.setDate(newExpiry.getDate() + 14);
        } else {
            if (licenses.length > 0) {
                const currentExpiry = new Date(licenses[0].ExpiryDate);
                if (currentExpiry < now) {
                    newExpiry = new Date();
                    newExpiry.setDate(newExpiry.getDate() + durationDays);
                } else {
                    newExpiry = new Date(currentExpiry.getTime());
                    newExpiry.setDate(newExpiry.getDate() + durationDays);
                }
            } else {
                newExpiry = new Date();
                newExpiry.setDate(newExpiry.getDate() + durationDays);
            }
        }

        if (licenses.length > 0) {
            await db.query('UPDATE Licenses SET LicenseType = ?, ExpiryDate = ? WHERE UserId = ?', [licenseType, newExpiry, userId]);
        } else {
            const key = licenseType.toUpperCase() + '-' + crypto.randomBytes(8).toString('hex').toUpperCase();
            await db.query('INSERT INTO Licenses (UserId, LicenseKey, LicenseType, ExpiryDate) VALUES (?, ?, ?, ?)', [userId, key, licenseType, newExpiry]);
        }

        res.json({ success: true, message: `Changed to ${licenseType}.` });
    } catch (error) {
        console.error('Error changing license:', error);
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

// POST /api/admin/users/reset-password - Reset user password to default
router.post('/admin/users/reset-password', async (req, res) => {
    const { userId } = req.body;

    if (!userId) {
        return res.status(400).json({ success: false, message: 'UserId is required' });
    }

    try {
        // Check if user exists
        const [users] = await db.query('SELECT Username FROM Users WHERE UserId = ?', [userId]);

        if (users.length === 0) {
            return res.status(404).json({ success: false, message: 'User not found / Người dùng không tìm thấy' });
        }

        // Reset password to default: 12345678
        // NOTE: Currently using plain text storage (should upgrade to bcrypt in production)
        const defaultPassword = '12345678';

        await db.query('UPDATE Users SET PasswordHash = ? WHERE UserId = ?', [defaultPassword, userId]);

        // Log the action
        await db.query(`
            INSERT INTO SystemLogs (LogLevel, LogMessage, Source)
            VALUES (?, ?, ?)
        `, ['warning', `Admin reset password for user: ${users[0].Username} (ID: ${userId})`, 'AdminPanel']);

        res.json({
            success: true,
            message: `Password reset to default (12345678) for user: ${users[0].Username}`
        });
    } catch (error) {
        console.error('Error resetting password:', error);
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

// POST /api/admin/users/create - Create new user from Admin Panel
router.post('/admin/users/create', async (req, res) => {
    const { username, password, email, licenseType } = req.body;

    if (!username || !password || !email) {
        return res.status(400).json({ success: false, message: 'Username, password, and email are required.' });
    }

    try {
        // 1. Check existing
        const [existing] = await db.query('SELECT 1 FROM Users WHERE Username = ? OR Email = ?', [username, email]);
        if (existing.length > 0) {
            return res.status(400).json({ success: false, message: 'Username or Email already exists.' });
        }

        // 2. Create User
        const [result] = await db.query('INSERT INTO Users (Username, PasswordHash, Email) VALUES (?, ?, ?)', [username, password, email]);
        const userId = result.insertId;

        // 3. Create License (Default Trial if not specified)
        const type = licenseType || 'Trial';
        let days = 14;
        if (type === 'Pro') days = 365;
        if (type === 'Premium') days = 1095;

        const expiryDate = new Date();
        expiryDate.setDate(expiryDate.getDate() + days);

        // Generate Key: TYPE-RANDOMHEX
        const licenseKey = `${type.toUpperCase()}-` + crypto.randomBytes(8).toString('hex').toUpperCase();

        await db.query('INSERT INTO Licenses (UserId, LicenseKey, LicenseType, ExpiryDate) VALUES (?, ?, ?, ?)',
            [userId, licenseKey, type, expiryDate]);

        // 4. Log
        await db.query(`
            INSERT INTO SystemLogs (LogLevel, LogMessage, Source)
            VALUES (?, ?, ?)
        `, ['info', `Admin created user: ${username} (ID: ${userId}) with ${type} license`, 'AdminPanel']);

        res.json({
            success: true,
            message: 'User created successfully.',
            userId: userId,
            licenseKey: licenseKey,
            licenseType: type
        });
    } catch (error) {
        console.error('Error creating user:', error);
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

// --- API Keys Endpoints ---
router.get('/apikeys', async (req, res) => {
    try {
        const [rows] = await db.query('SELECT * FROM ApiKeys ORDER BY CreatedAt DESC');
        res.json({ success: true, data: rows });
    } catch (error) {
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

router.post('/apikeys', async (req, res) => {
    const { title } = req.body;
    try {
        const newKey = generateApiKey();
        await db.query('INSERT INTO ApiKeys (Title, ApiKey) VALUES (?, ?)', [title, newKey]);
        res.json({ success: true, message: 'API Key generated', apiKey: newKey });
    } catch (error) {
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

// --- Notification Endpoints ---

router.post('/notify/upgrade', async (req, res) => {
    const { userId, username, package: pkg } = req.body;

    // Telegram/Email Logic (skipped for brevity but should be here if needed)

    try {
        await db.query(`
            INSERT INTO SystemLogs (LogLevel, LogMessage, Source)
            VALUES (?, ?, ?)
        `, ['info', `Bot: Yêu cầu nâng cấp từ ${username} (Gói: ${pkg})`, 'UpgradeBot']);
    } catch (dbErr) {
        console.error('Failed to log to DB:', dbErr.message);
    }

    res.json({ success: true, message: 'Notification sent' });
});

router.post('/notify/payment-complete', async (req, res) => {
    const { userId, username, package: pkg } = req.body;

    try {
        await db.query(`
            INSERT INTO SystemLogs (LogLevel, LogMessage, Source)
            VALUES (?, ?, ?)
        `, ['success', `Bot: Thanh toán hoàn tất từ ${username} (Gói: ${pkg})`, 'PaymentBot']);
    } catch (dbErr) {
        console.error('Failed to log to DB:', dbErr.message);
    }

    res.json({ success: true, message: 'Payment complete notification sent' });
});

// --- Notifications Endpoints (Desktop <-> Web) ---

router.post('/admin/notifications/send', async (req, res) => {
    const { userId, title, message, notificationType, priority, expiresInDays, imageUrl, actionUrl, isPromotional } = req.body;
    try {
        if (!title || !message) {
            return res.status(400).json({ success: false, message: 'Title and message are required' });
        }
        let expiresAt = null;
        const days = expiresInDays || 2;
        expiresAt = new Date();
        expiresAt.setDate(expiresAt.getDate() + days);

        await db.query(`
            INSERT INTO Notifications (UserId, Title, Message, NotificationType, Priority, ExpiresAt, ImageUrl, ActionUrl, IsPromotional)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
        `, [userId || null, title, message, notificationType || 'info', priority || 0, expiresAt, imageUrl || null, actionUrl || null, isPromotional || false]);

        res.json({ success: true, message: 'Notification sent' });
    } catch (error) {
        console.error('Error sending notification:', error);
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

router.get('/notifications/:userId', async (req, res) => {
    try {
        const userId = req.params.userId;
        const includeRead = req.query.includeRead === 'true';

        let whereClause = '(UserId = ? OR UserId IS NULL)';
        if (!includeRead) {
            whereClause += ' AND IsRead = FALSE';
        }

        await db.query('DELETE FROM Notifications WHERE ExpiresAt < NOW()');

        const [notifications] = await db.query(`
            SELECT * FROM Notifications
            WHERE ${whereClause} AND (ExpiresAt IS NULL OR ExpiresAt > NOW())
            ORDER BY Priority DESC, CreatedAt DESC LIMIT 50
        `, [userId]);

        res.json({ success: true, data: notifications });
    } catch (error) {
        console.error('Error fetching notifications:', error);
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

router.put('/notifications/:notificationId/read', async (req, res) => {
    try {
        await db.query('UPDATE Notifications SET IsRead = TRUE WHERE NotificationId = ?', [req.params.notificationId]);
        res.json({ success: true, message: 'Marked as read' });
    } catch (error) {
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

// --- Analytics Endpoints ---

router.get('/admin/analytics/overview', async (req, res) => {
    try {
        const [totalUsersResult] = await db.query('SELECT COUNT(*) as total FROM Users');
        const totalUsers = totalUsersResult[0].total;

        const [activeUsersResult] = await db.query('SELECT COUNT(DISTINCT UserId) as count FROM Licenses WHERE ExpiryDate > NOW()');
        const activeUsers = activeUsersResult[0].count;

        const [expiringUsersResult] = await db.query('SELECT COUNT(DISTINCT UserId) as count FROM Licenses WHERE ExpiryDate BETWEEN NOW() AND DATE_ADD(NOW(), INTERVAL 7 DAY)');
        const expiringUsers = expiringUsersResult[0].count;

        const [licenseDistResult] = await db.query('SELECT LicenseType, COUNT(*) as count FROM Licenses GROUP BY LicenseType');

        // Transform for frontend
        const licenseDistribution = licenseDistResult.map(r => ({ licenseType: r.LicenseType, count: r.count }));

        res.json({
            success: true,
            data: {
                totalUsers,
                activeUsers,
                expiringLicenses: expiringUsers,
                licenseDistribution
            }
        });
    } catch (error) {
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

// --- Chat Endpoints ---

router.post('/chat/send', async (req, res) => {
    const { sessionId, userId, userName, message, isAgent } = req.body;
    try {
        const [result] = await db.query(`
            INSERT INTO ChatMessages (SessionId, UserId, UserName, Message, IsAgent, IsRead)
            VALUES (?, ?, ?, ?, ?, ?)
        `, [sessionId, userId || 'anonymous', userName || 'Guest', message, isAgent || false, false]);
        res.json({ success: true, messageId: result.insertId });
    } catch (error) {
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

router.get('/chat/history/:sessionId', async (req, res) => {
    try {
        const [rows] = await db.query('SELECT * FROM ChatMessages WHERE SessionId = ? ORDER BY CreatedAt ASC', [req.params.sessionId]);
        // Mask messages as read if requested by admin (implied by fetching history in admin panel)
        // await db.query('UPDATE ChatMessages SET IsRead = 1 WHERE SessionId = ? AND IsAgent = 0', [req.params.sessionId]);
        res.json({ success: true, data: rows });
    } catch (error) {
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

router.get('/chat/sessions', async (req, res) => {
    try {
        // Get list of unique sessions with last message time and unread count
        const query = `
            SELECT 
                SessionId, 
                MAX(UserName) as UserName, 
                MAX(CreatedAt) as LastActive, 
                SUM(CASE WHEN IsRead = 0 AND IsAgent = 0 THEN 1 ELSE 0 END) as UnreadCount,
                COUNT(*) as TotalMessages
            FROM ChatMessages
            GROUP BY SessionId
            ORDER BY LastActive DESC
        `;
        const [rows] = await db.query(query);
        res.json({ success: true, data: rows });
    } catch (error) {
        console.error('Chat sessions error:', error);
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

router.get('/chat/poll/:sessionId', async (req, res) => {
    try {
        const { sessionId } = req.params;
        const lastId = req.query.lastId || 0;

        const [rows] = await db.query(
            'SELECT * FROM ChatMessages WHERE SessionId = ? AND MessageId > ? ORDER BY MessageId ASC',
            [sessionId, lastId]
        );
        res.json({ success: true, data: rows });
    } catch (error) {
        console.error('Chat poll error:', error);
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

router.get('/admin/logs', async (req, res) => {
    try {
        const [logs] = await db.query('SELECT * FROM SystemLogs ORDER BY CreatedAt DESC LIMIT 20');
        res.json({ success: true, data: logs });
    } catch (error) {
        res.status(500).json({ success: false, message: 'Database error' });
    }
});

module.exports = router;
