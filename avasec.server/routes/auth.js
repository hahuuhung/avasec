const express = require('express');
const router = express.Router();
const db = require('../config/db.config');
const crypto = require('crypto');
const bcrypt = require('bcrypt');

const BCRYPT_ROUNDS = 10;

function generateLicenseKey() {
    return 'TRIAL-' + crypto.randomBytes(8).toString('hex').toUpperCase();
}

async function verifyPassword(plain, storedHash) {
    if (!storedHash) {
        return false;
    }

    if (storedHash.startsWith('$2')) {
        return bcrypt.compare(plain, storedHash);
    }

    // Legacy plain-text passwords (migrate on successful login)
    return plain === storedHash;
}

async function hashPassword(plain) {
    return bcrypt.hash(plain, BCRYPT_ROUNDS);
}

function adminAuth(req, res, next) {
    const secret = req.headers['x-admin-secret'] || req.query.adminSecret || req.body?.adminSecret;
    if (!secret || secret !== process.env.ADMIN_SECRET) {
        return res.status(403).json({ success: false, message: 'Forbidden / Không có quyền admin' });
    }
    next();
}

// --- Admin: quản lý người dùng (cần x-admin-secret) ---

router.get('/admin/users', adminAuth, async (req, res) => {
    try {
        const [rows] = await db.query(`
            SELECT u.UserId, u.Username, u.Email, u.CreatedAt, u.IsActive,
                   l.LicenseId, l.LicenseKey, l.LicenseType, l.ExpiryDate, l.IsActive AS LicenseActive
            FROM Users u
            LEFT JOIN Licenses l ON u.UserId = l.UserId
            ORDER BY u.UserId DESC
            LIMIT 500
        `);
        const [stats] = await db.query(`
            SELECT COUNT(*) AS total,
                   SUM(CASE WHEN IsActive = TRUE THEN 1 ELSE 0 END) AS active
            FROM Users
        `);
        res.json({ success: true, users: rows, stats: stats[0] || { total: 0, active: 0 } });
    } catch (error) {
        console.error('Admin list users:', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

router.post('/admin/users', adminAuth, async (req, res) => {
    const { username, password, email, grantTrial = true } = req.body;
    if (!username || !password || !email) {
        return res.status(400).json({ success: false, message: 'username, password, email required' });
    }
    try {
        const [existing] = await db.query(
            'SELECT 1 FROM Users WHERE Username = ? OR Email = ?', [username, email]
        );
        if (existing.length > 0) {
            return res.status(400).json({ success: false, message: 'Username hoặc Email đã tồn tại' });
        }
        const passwordHash = await hashPassword(password);
        const [result] = await db.query(
            'INSERT INTO Users (Username, PasswordHash, Email, IsActive) VALUES (?, ?, ?, TRUE)',
            [username, passwordHash, email]
        );
        const userId = result.insertId;
        let licenseKey = null;
        if (grantTrial) {
            const expiryDate = new Date();
            expiryDate.setDate(expiryDate.getDate() + 14);
            licenseKey = generateLicenseKey();
            await db.query(
                'INSERT INTO Licenses (UserId, LicenseKey, LicenseType, ExpiryDate, IsActive) VALUES (?, ?, ?, ?, TRUE)',
                [userId, licenseKey, 'Trial', expiryDate]
            );
        }
        res.json({
            success: true,
            message: 'Đã tạo user',
            userId,
            licenseKey
        });
    } catch (error) {
        console.error('Admin create user:', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

router.patch('/admin/users/:id', adminAuth, async (req, res) => {
    const userId = parseInt(req.params.id, 10);
    const { email, password, isActive } = req.body;
    if (!userId) {
        return res.status(400).json({ success: false, message: 'Invalid user id' });
    }
    try {
        const [users] = await db.query('SELECT UserId FROM Users WHERE UserId = ?', [userId]);
        if (users.length === 0) {
            return res.status(404).json({ success: false, message: 'User not found' });
        }
        if (email) {
            await db.query('UPDATE Users SET Email = ? WHERE UserId = ?', [email, userId]);
        }
        if (typeof isActive === 'boolean') {
            await db.query('UPDATE Users SET IsActive = ? WHERE UserId = ?', [isActive, userId]);
        }
        if (password && password.length >= 4) {
            const hash = await hashPassword(password);
            await db.query('UPDATE Users SET PasswordHash = ? WHERE UserId = ?', [hash, userId]);
        }
        res.json({ success: true, message: 'Đã cập nhật user' });
    } catch (error) {
        console.error('Admin update user:', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

router.post('/admin/users/:id/grant-trial', adminAuth, async (req, res) => {
    const userId = parseInt(req.params.id, 10);
    const days = parseInt(req.body?.days, 10) || 14;
    try {
        const [existing] = await db.query('SELECT LicenseId FROM Licenses WHERE UserId = ?', [userId]);
        const expiryDate = new Date();
        expiryDate.setDate(expiryDate.getDate() + days);
        const licenseKey = generateLicenseKey();
        if (existing.length > 0) {
            await db.query(
                `UPDATE Licenses SET LicenseKey = ?, LicenseType = 'Trial', ExpiryDate = ?, IsActive = TRUE, IssueDate = NOW()
                 WHERE UserId = ?`,
                [licenseKey, expiryDate, userId]
            );
        } else {
            await db.query(
                'INSERT INTO Licenses (UserId, LicenseKey, LicenseType, ExpiryDate, IsActive) VALUES (?, ?, ?, ?, TRUE)',
                [userId, licenseKey, 'Trial', expiryDate]
            );
        }
        res.json({ success: true, message: 'Đã cấp Trial', licenseKey });
    } catch (error) {
        console.error('Admin grant trial:', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

router.post('/register', async (req, res) => {
    const { username, password, email } = req.body;

    if (!username || !password || !email) {
        return res.status(400).json({ success: false, message: 'Missing required fields / Thiếu thông tin bắt buộc' });
    }

    try {
        const [existing] = await db.query('SELECT 1 FROM Users WHERE Username = ? OR Email = ?', [username, email]);
        if (existing.length > 0) {
            return res.status(400).json({ success: false, message: 'Username or Email already exists / Tên đăng nhập hoặc Email đã tồn tại' });
        }

        const passwordHash = await hashPassword(password);

        const [result] = await db.query('INSERT INTO Users (Username, PasswordHash, Email) VALUES (?, ?, ?)', [username, passwordHash, email]);
        const userId = result.insertId;

        const expiryDate = new Date();
        expiryDate.setDate(expiryDate.getDate() + 14);
        const licenseKey = generateLicenseKey();

        await db.query('INSERT INTO Licenses (UserId, LicenseKey, LicenseType, ExpiryDate) VALUES (?, ?, ?, ?)',
            [userId, licenseKey, 'Trial', expiryDate]);

        res.json({
            success: true,
            message: 'Registration successful! Please login. / Đăng ký thành công! Vui lòng đăng nhập.',
            userId: userId
        });

    } catch (error) {
        console.error('Register Error:', error);
        res.status(500).json({ success: false, message: 'Server error during registration / Lỗi server khi đăng ký' });
    }
});

router.post('/login', async (req, res) => {
    const { username, password } = req.body;

    try {
        const [users] = await db.query('SELECT UserId, Username, PasswordHash FROM Users WHERE Username = ?', [username]);

        if (users.length === 0) {
            return res.status(401).json({ success: false, message: 'Invalid credentials / Thông tin đăng nhập không hợp lệ' });
        }

        const user = users[0];
        const match = await verifyPassword(password, user.PasswordHash);

        if (!match) {
            return res.status(401).json({ success: false, message: 'Invalid credentials / Thông tin đăng nhập không hợp lệ' });
        }

        if (!user.PasswordHash.startsWith('$2')) {
            const upgraded = await hashPassword(password);
            await db.query('UPDATE Users SET PasswordHash = ? WHERE UserId = ?', [upgraded, user.UserId]);
        }

        const [licRows] = await db.query(
            'SELECT LicenseKey, LicenseType, ExpiryDate, IsActive FROM Licenses WHERE UserId = ? LIMIT 1',
            [user.UserId]
        );
        const license = licRows.length > 0 ? {
            licenseKey: licRows[0].LicenseKey,
            licenseType: licRows[0].LicenseType,
            expiryDate: licRows[0].ExpiryDate,
            isActive: !!licRows[0].IsActive
        } : null;

        res.json({
            success: true,
            message: 'Login successful / Đăng nhập thành công',
            userId: user.UserId,
            username: user.Username,
            license
        });

    } catch (error) {
        console.error('Login Error:', error);
        res.status(500).json({ success: false, message: 'Server error / Lỗi server' });
    }
});

router.get('/user/:id', async (req, res) => {
    try {
        const userId = req.params.id;

        const [rows] = await db.query(`
            SELECT 
                u.UserId, u.Username, u.Email,
                l.LicenseId, l.LicenseKey, l.LicenseType, l.ExpiryDate
            FROM Users u
            LEFT JOIN Licenses l ON u.UserId = l.UserId
            WHERE u.UserId = ?
            LIMIT 1
        `, [userId]);

        if (rows.length === 0) {
            return res.status(404).json({ success: false, message: 'User not found' });
        }

        const row = rows[0];
        let license = null;

        if (row.LicenseId) {
            license = {
                LicenseId: row.LicenseId,
                LicenseKey: row.LicenseKey,
                LicenseType: row.LicenseType,
                ExpiryDate: row.ExpiryDate,
                UserId: row.UserId
            };
        }

        res.json({
            success: true,
            data: {
                UserId: row.UserId,
                Username: row.Username,
                Email: row.Email,
                PasswordHash: "",
                License: license
            }
        });

    } catch (error) {
        console.error('Get User Error:', error);
        res.status(500).json({ success: false, message: 'Server error' });
    }
});

module.exports = router;
