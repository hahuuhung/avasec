const express = require('express');
const router = express.Router();
const db = require('../config/db.config');
const crypto = require('crypto');

const PLANS = [
    { id: 'trial', name: 'Trial / Dùng thử', price: 0, days: 14, type: 'Trial' },
    { id: 'ultra', name: 'Ultra / Năm', price: 19, days: 365, type: 'Ultra' },
    { id: 'lifetime', name: 'Lifetime / Trọn đời', price: 99, days: 36500, type: 'Lifetime' }
];

function generateKey(prefix = 'AVA') {
    const part = () => crypto.randomBytes(2).toString('hex').toUpperCase();
    return `${prefix}-${part()}-${part()}-${part()}-${part()}`;
}

async function ensurePoolTable() {
    await db.query(`
        CREATE TABLE IF NOT EXISTS LicenseKeyPool (
            PoolId INT AUTO_INCREMENT PRIMARY KEY,
            LicenseKey VARCHAR(100) NOT NULL UNIQUE,
            LicenseType VARCHAR(20) NOT NULL,
            DurationDays INT NOT NULL,
            IsRedeemed BOOLEAN DEFAULT FALSE,
            RedeemedByUserId INT NULL,
            RedeemedAt DATETIME NULL,
            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
        )
    `);
}

ensurePoolTable().catch(err => console.error('License pool init:', err.message));

// GET /api/license/plans — public pricing (no payment gateway needed for MVP)
router.get('/plans', (req, res) => {
    res.json({ success: true, plans: PLANS });
});

// GET /api/license/validate/:key
router.get('/validate/:key', async (req, res) => {
    const key = req.params.key;
    try {
        const license = await findLicenseByKey(key);
        if (!license) {
            return res.status(404).json({ success: false, message: 'Key not found / Không tìm thấy key' });
        }
        const valid = license.IsActive && new Date(license.ExpiryDate) > new Date();
        res.json({
            success: valid,
            message: valid ? 'Valid / Hợp lệ' : 'Expired or inactive / Hết hạn hoặc vô hiệu',
            license: formatLicense(license)
        });
    } catch (error) {
        console.error('Validate error:', error);
        res.status(500).json({ success: false, message: 'Server error' });
    }
});

// POST /api/license/activate { licenseKey, userId }
router.post('/activate', async (req, res) => {
    const { licenseKey, userId } = req.body;
    if (!licenseKey || !userId) {
        return res.status(400).json({ success: false, message: 'licenseKey and userId required' });
    }

    try {
        const [users] = await db.query('SELECT UserId, Username FROM Users WHERE UserId = ?', [userId]);
        if (users.length === 0) {
            return res.status(404).json({ success: false, message: 'User not found' });
        }

        // 1) Key already assigned to this user
        const [existing] = await db.query(
            'SELECT * FROM Licenses WHERE LicenseKey = ? AND UserId = ?',
            [licenseKey, userId]
        );
        if (existing.length > 0) {
            const lic = existing[0];
            return res.json({
                success: true,
                message: 'Already activated / Đã kích hoạt',
                license: formatLicense(lic)
            });
        }

        // 2) Redeem from pool
        const [pool] = await db.query(
            'SELECT * FROM LicenseKeyPool WHERE LicenseKey = ? AND IsRedeemed = FALSE',
            [licenseKey]
        );

        if (pool.length > 0) {
            const item = pool[0];
            const expiry = new Date();
            expiry.setDate(expiry.getDate() + item.DurationDays);

            await db.query(
                'UPDATE LicenseKeyPool SET IsRedeemed = TRUE, RedeemedByUserId = ?, RedeemedAt = NOW() WHERE PoolId = ?',
                [userId, item.PoolId]
            );

            const license = await upsertUserLicense(userId, licenseKey, item.LicenseType, expiry);
            return res.json({
                success: true,
                message: 'Activation successful! / Kích hoạt thành công!',
                license: formatLicense(license)
            });
        }

        // 3) Key exists but belongs to another user
        const [other] = await db.query('SELECT UserId FROM Licenses WHERE LicenseKey = ?', [licenseKey]);
        if (other.length > 0) {
            return res.status(400).json({ success: false, message: 'Key already used / Key đã được sử dụng' });
        }

        return res.status(404).json({ success: false, message: 'Invalid license key / Key không hợp lệ' });
    } catch (error) {
        console.error('Activate error:', error);
        res.status(500).json({ success: false, message: 'Server error' });
    }
});

function adminAuth(req, res, next) {
    const secret = req.headers['x-admin-secret'] || req.query.adminSecret || req.body?.adminSecret;
    if (!secret || secret !== process.env.ADMIN_SECRET) {
        return res.status(403).json({ success: false, message: 'Forbidden / Không có quyền admin' });
    }
    next();
}

// POST /api/license/admin/create-keys
router.post('/admin/create-keys', adminAuth, async (req, res) => {
    const { planId = 'ultra', count = 1 } = req.body;
    const plan = PLANS.find(p => p.id === planId) || PLANS[1];
    const keys = [];

    try {
        for (let i = 0; i < Math.min(count, 100); i++) {
            const key = generateKey('AVA');
            await db.query(
                'INSERT INTO LicenseKeyPool (LicenseKey, LicenseType, DurationDays) VALUES (?, ?, ?)',
                [key, plan.type, plan.days]
            );
            keys.push(key);
        }
        res.json({ success: true, keys, plan: plan.name });
    } catch (error) {
        console.error('Create keys error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

// GET /api/license/admin/pool?status=available|redeemed|all
router.get('/admin/pool', adminAuth, async (req, res) => {
    const status = req.query.status || 'all';
    try {
        let where = '';
        if (status === 'available') where = 'WHERE IsRedeemed = FALSE';
        else if (status === 'redeemed') where = 'WHERE IsRedeemed = TRUE';

        const [rows] = await db.query(`
            SELECT PoolId, LicenseKey, LicenseType, DurationDays, IsRedeemed,
                   RedeemedByUserId, RedeemedAt, CreatedAt
            FROM LicenseKeyPool
            ${where}
            ORDER BY CreatedAt DESC
            LIMIT 500
        `);

        const [stats] = await db.query(`
            SELECT
                SUM(CASE WHEN IsRedeemed = FALSE THEN 1 ELSE 0 END) AS available,
                SUM(CASE WHEN IsRedeemed = TRUE THEN 1 ELSE 0 END) AS redeemed,
                COUNT(*) AS total
            FROM LicenseKeyPool
        `);

        res.json({ success: true, keys: rows, stats: stats[0] || { available: 0, redeemed: 0, total: 0 } });
    } catch (error) {
        console.error('Pool list error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

// GET /api/license/admin/licenses
router.get('/admin/licenses', adminAuth, async (req, res) => {
    try {
        const [rows] = await db.query(`
            SELECT l.LicenseId, l.LicenseKey, l.LicenseType, l.IssueDate, l.ExpiryDate, l.IsActive,
                   u.UserId, u.Username, u.Email
            FROM Licenses l
            INNER JOIN Users u ON l.UserId = u.UserId
            ORDER BY l.IssueDate DESC
            LIMIT 500
        `);

        const [stats] = await db.query(`
            SELECT
                COUNT(*) AS total,
                SUM(CASE WHEN IsActive = TRUE AND ExpiryDate > NOW() THEN 1 ELSE 0 END) AS active,
                SUM(CASE WHEN IsActive = FALSE OR ExpiryDate <= NOW() THEN 1 ELSE 0 END) AS inactive
            FROM Licenses
        `);

        res.json({ success: true, licenses: rows, stats: stats[0] || { total: 0, active: 0, inactive: 0 } });
    } catch (error) {
        console.error('License list error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

// POST /api/license/admin/revoke-license { licenseId }
router.post('/admin/revoke-license', adminAuth, async (req, res) => {
    const { licenseId } = req.body;
    if (!licenseId) {
        return res.status(400).json({ success: false, message: 'licenseId required' });
    }
    try {
        const [result] = await db.query(
            'UPDATE Licenses SET IsActive = FALSE WHERE LicenseId = ?',
            [licenseId]
        );
        if (result.affectedRows === 0) {
            return res.status(404).json({ success: false, message: 'License not found' });
        }
        res.json({ success: true, message: 'License revoked / Đã thu hồi license' });
    } catch (error) {
        console.error('Revoke license error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

// DELETE /api/license/admin/pool/:poolId — remove unredeemed key from pool
router.delete('/admin/pool/:poolId', adminAuth, async (req, res) => {
    try {
        const [result] = await db.query(
            'DELETE FROM LicenseKeyPool WHERE PoolId = ? AND IsRedeemed = FALSE',
            [req.params.poolId]
        );
        if (result.affectedRows === 0) {
            return res.status(404).json({
                success: false,
                message: 'Key not found or already redeemed / Key không tồn tại hoặc đã dùng'
            });
        }
        res.json({ success: true, message: 'Key deleted / Đã xóa key' });
    } catch (error) {
        console.error('Delete pool key error:', error);
        res.status(500).json({ success: false, message: error.message });
    }
});

async function findLicenseByKey(key) {
    const [rows] = await db.query('SELECT * FROM Licenses WHERE LicenseKey = ?', [key]);
    if (rows.length > 0) return rows[0];

    const [pool] = await db.query(
        'SELECT LicenseKey, LicenseType, DurationDays, IsRedeemed FROM LicenseKeyPool WHERE LicenseKey = ?',
        [key]
    );
    if (pool.length > 0 && !pool[0].IsRedeemed) {
        const expiry = new Date();
        expiry.setDate(expiry.getDate() + pool[0].DurationDays);
        return {
            LicenseKey: pool[0].LicenseKey,
            LicenseType: pool[0].LicenseType,
            ExpiryDate: expiry,
            IsActive: true,
            UserId: 0
        };
    }
    return null;
}

async function upsertUserLicense(userId, licenseKey, licenseType, expiryDate) {
    const [existing] = await db.query('SELECT LicenseId FROM Licenses WHERE UserId = ?', [userId]);
    if (existing.length > 0) {
        await db.query(
            `UPDATE Licenses SET LicenseKey = ?, LicenseType = ?, ExpiryDate = ?, IsActive = TRUE, IssueDate = NOW()
             WHERE UserId = ?`,
            [licenseKey, licenseType, expiryDate, userId]
        );
    } else {
        await db.query(
            'INSERT INTO Licenses (UserId, LicenseKey, LicenseType, ExpiryDate, IsActive) VALUES (?, ?, ?, ?, TRUE)',
            [userId, licenseKey, licenseType, expiryDate]
        );
    }
    const [rows] = await db.query('SELECT * FROM Licenses WHERE UserId = ?', [userId]);
    return rows[0];
}

function formatLicense(row) {
    return {
        licenseKey: row.LicenseKey,
        licenseType: row.LicenseType,
        expiryDate: row.ExpiryDate,
        isActive: !!row.IsActive,
        userId: row.UserId
    };
}

module.exports = router;
