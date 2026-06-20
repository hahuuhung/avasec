const mysql = require('mysql2/promise');
require('dotenv').config();

// Create connection configuration
const dbConfig = {
    host: process.env.DB_HOST || 'localhost',
    user: process.env.DB_USER || 'root',
    password: process.env.DB_PASSWORD || '',
    database: process.env.DB_NAME || 'avasec_db',
    waitForConnections: true,
    connectionLimit: 10, // Giảm limit xuống
    queueLimit: 0,
    connectTimeout: 10000 // Thêm timeout
};

const pool = mysql.createPool(dbConfig);

// Test connection softly
(async () => {
    try {
        const connection = await pool.getConnection();
        console.log('✅ MySQL Database connected successfully!');
        connection.release();
    } catch (err) {
        console.error('❌ MySQL Connection Failed:', err.code);
        console.error('⚠️  Server will start but API features needing DB will fail.');
        if (err.code === 'ECONNREFUSED') {
            console.log('👉 Tip: Check if MySQL connects at localhost:3306');
        }
    }
})();

module.exports = pool;
