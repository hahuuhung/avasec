const db = require('./config/db.config');

async function checkAdmin() {
    try {
        const [users] = await db.query('SELECT * FROM Users WHERE Username = ?', ['admin']);
        if (users.length === 0) {
            console.log('User admin not found');
            return;
        }
        const user = users[0];
        console.log('User:', user);

        const [licenses] = await db.query('SELECT * FROM Licenses WHERE UserId = ?', [user.UserId]);
        console.log('Licenses:', licenses);
    } catch (err) {
        console.error(err);
    }
    process.exit();
}

checkAdmin();
