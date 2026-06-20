const db = require('./config/db.config');

async function run() {
    try {
        console.log('Adding columns to Notifications table...');

        // Add ImageUrl
        try {
            await db.query("ALTER TABLE Notifications ADD COLUMN ImageUrl VARCHAR(255) DEFAULT NULL AFTER Priority");
            console.log('Added ImageUrl');
        } catch (e) {
            if (e.code === 'ER_DUP_FIELDNAME') console.log('ImageUrl already exists');
            else throw e;
        }

        // Add ActionUrl
        try {
            await db.query("ALTER TABLE Notifications ADD COLUMN ActionUrl VARCHAR(255) DEFAULT NULL AFTER ImageUrl");
            console.log('Added ActionUrl');
        } catch (e) {
            if (e.code === 'ER_DUP_FIELDNAME') console.log('ActionUrl already exists');
            else throw e;
        }

        // Add IsPromotional
        try {
            await db.query("ALTER TABLE Notifications ADD COLUMN IsPromotional BOOLEAN DEFAULT FALSE AFTER ActionUrl");
            console.log('Added IsPromotional');
        } catch (e) {
            if (e.code === 'ER_DUP_FIELDNAME') console.log('IsPromotional already exists');
            else throw e;
        }

        console.log('Migration complete');
        process.exit(0);
    } catch (err) {
        console.error('Migration failed:', err);
        process.exit(1);
    }
}

run();
