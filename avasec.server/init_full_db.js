const fs = require('fs');
const path = require('path');
const db = require('./config/db.config');

async function run() {
    try {
        console.log('Starting full database initialization...');

        // 1. Read and Execute database.sql
        console.log('Executing database.sql...');
        const sql1 = fs.readFileSync(path.join(__dirname, 'database.sql'), 'utf8');
        const statements1 = sql1.split(';').filter(s => s.trim());
        for (const stmt of statements1) {
            if (stmt.trim()) {
                try {
                    await db.query(stmt);
                } catch (e) {
                    if (e.code !== 'ER_DUP_ENTRY' && e.code !== 'ER_TABLE_EXISTS_ERROR') {
                        console.warn('Warning in database.sql:', e.message);
                    }
                }
            }
        }

        // 2. Read and Execute database_optimizations.sql
        console.log('Executing database_optimizations.sql...');
        const sql2 = fs.readFileSync(path.join(__dirname, 'database_optimizations.sql'), 'utf8');
        const statements2 = sql2.split(';').filter(s => s.trim());
        for (const stmt of statements2) {
            if (stmt.trim()) {
                try {
                    await db.query(stmt);
                } catch (e) {
                     if (e.code !== 'ER_DUP_ENTRY' && e.code !== 'ER_TABLE_EXISTS_ERROR' && e.code !== 'ER_DUP_KEYNAME') {
                        console.warn('Warning in database_optimizations.sql:', e.message);
                    }
                }
            }
        }

        // 3. Create ChatMessages Table (Inferred)
        console.log('Creating ChatMessages table...');
        const chatTableSql = `
            CREATE TABLE IF NOT EXISTS ChatMessages (
                MessageId INT AUTO_INCREMENT PRIMARY KEY,
                SessionId VARCHAR(100) NOT NULL,
                UserId VARCHAR(100) NULL, -- Can be 'anonymous' or user ID
                UserName VARCHAR(100) DEFAULT 'Guest',
                Message TEXT,
                IsAgent BOOLEAN DEFAULT FALSE,
                IsRead BOOLEAN DEFAULT FALSE,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        `;
        await db.query(chatTableSql);
        
        // Index for ChatMessages
        await db.query("CREATE INDEX IF NOT EXISTS idx_chat_session ON ChatMessages(SessionId)");
        await db.query("CREATE INDEX IF NOT EXISTS idx_chat_created ON ChatMessages(CreatedAt)");

        console.log('Database initialization complete!');
        process.exit(0);
    } catch (err) {
        console.error('Initialization failed:', err);
        process.exit(1);
    }
}

run();
