/**
 * Script này tự động tạo 50 người dùng và giấy phép tương ứng vào MySQL.
 * Chạy bằng lệnh: node seed_test_data.js
 */
const pool = require('./config/db.config');

async function seedData() {
    console.log('🚀 Bắt đầu thêm 50 users dữ liệu kiểm thử...');

    try {
        for (let i = 1; i <= 50; i++) {
            const username = `testuser${i}`;
            const email = `user${i}@demo.sysanti.com`;
            const password = 'password123'; // Logic auth.js đang dùng text cho admin, nhưng ở đây dùng mockup

            // 1. Thêm User
            const [result] = await pool.execute(
                'INSERT IGNORE INTO Users (Username, PasswordHash, Email) VALUES (?, ?, ?)',
                [username, password, email]
            );

            if (result.insertId > 0) {
                const userId = result.insertId;

                // 2. Phân loại license
                let type = 'Trial';
                let days = 14;
                if (i % 3 === 0) { type = 'Premium'; days = 365; }
                else if (i % 3 === 1) { type = 'Pro'; days = 30; }

                const licenseKey = `SK-TEST-${Math.random().toString(36).substring(2, 10).toUpperCase()}`;
                const expiryDate = new Date();
                expiryDate.setDate(expiryDate.getDate() + days);

                // 3. Thêm License
                await pool.execute(
                    'INSERT INTO Licenses (UserId, LicenseKey, ExpiryDate, LicenseType) VALUES (?, ?, ?, ?)',
                    [userId, licenseKey, expiryDate, type]
                );

                if (i % 10 === 0) console.log(`... Đã xong ${i} users`);
            }
        }

        console.log('✅ Hoàn tất! Đã thêm 50 users và licenses.');
        process.exit(0);
    } catch (error) {
        console.error('❌ Lỗi khi seed dữ liệu:', error);
        process.exit(1);
    }
}

seedData();
