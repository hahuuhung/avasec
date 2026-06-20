const fs = require('fs');
const path = require('path');
const mysql = require('mysql2/promise');
require('dotenv').config();

const dbConfig = {
    host: process.env.DB_HOST || 'localhost',
    user: process.env.DB_USER || 'root',
    password: process.env.DB_PASSWORD || '',
    database: process.env.DB_NAME || 'avasec_db',
    multipleStatements: true
};

async function runSql() {
    const filePath = path.join(__dirname, 'database_optimizations.sql');
    if (!fs.existsSync(filePath)) {
        console.error('File not found:', filePath);
        process.exit(1);
    }

    const sql = fs.readFileSync(filePath, 'utf8');
    const connection = await mysql.createConnection(dbConfig);

    try {
        console.log('Applying database optimizations...');
        await connection.query(sql);
        console.log('✅ Database optimizations applied successfully!');

        // Seed 50 users if the file exists
        const seedPath = path.join(__dirname, 'seed_50_users.sql');
        if (fs.existsSync(seedPath)) {
            console.log('Seeding 50 sample users...');
            const seedSql = fs.readFileSync(seedPath, 'utf8');
            await connection.query(seedSql);
            console.log('✅ Seed data added successfully!');
        }

    } catch (error) {
        console.error('❌ Error applying SQL:', error.message);
    } finally {
        await connection.end();
        process.exit(0);
    }
}

runSql();
