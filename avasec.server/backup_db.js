const fs = require('fs');
const path = require('path');
const db = require('./config/db.config');

const BACKUP_DIR = path.join(__dirname, 'backups');

// Ensure backup directory exists
if (!fs.existsSync(BACKUP_DIR)) {
    fs.mkdirSync(BACKUP_DIR);
}

async function backup() {
    console.log('Starting backup...');
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    const backupPath = path.join(BACKUP_DIR, `backup_${timestamp}`);

    if (!fs.existsSync(backupPath)) {
        fs.mkdirSync(backupPath);
    }

    try {
        // 1. Backup Users
        console.log('Backing up Users...');
        const [users] = await db.query('SELECT * FROM Users');
        fs.writeFileSync(path.join(backupPath, 'users.json'), JSON.stringify(users, null, 2));

        // 2. Backup Licenses
        console.log('Backing up Licenses...');
        const [licenses] = await db.query('SELECT * FROM Licenses');
        fs.writeFileSync(path.join(backupPath, 'licenses.json'), JSON.stringify(licenses, null, 2));

        // 3. Backup ApiKeys
        console.log('Backing up ApiKeys...');
        const [apiKeys] = await db.query('SELECT * FROM ApiKeys');
        fs.writeFileSync(path.join(backupPath, 'apikeys.json'), JSON.stringify(apiKeys, null, 2));

        // 4. Backup Notifications (if exists)
        try {
            console.log('Backing up Notifications...');
            const [notifications] = await db.query('SELECT * FROM Notifications');
            fs.writeFileSync(path.join(backupPath, 'notifications.json'), JSON.stringify(notifications, null, 2));
        } catch (e) {
            console.log('Notifications table might not exist yet, skipping.');
        }

        console.log(`Backup completed successfully to: ${backupPath}`);
        process.exit(0);
    } catch (error) {
        console.error('Backup failed:', error);
        process.exit(1);
    }
}

backup();
