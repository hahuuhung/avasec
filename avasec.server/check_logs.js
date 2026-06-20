const fs = require('fs');
const path = require('path');

const ERROR_LOG = path.join(__dirname, 'startup_error.txt');

console.log('--- SysAnti Log Monitor ---');
console.log(`Checking: ${ERROR_LOG}`);

if (fs.existsSync(ERROR_LOG)) {
    const stats = fs.statSync(ERROR_LOG);
    console.log(`Last Modified: ${stats.mtime}`);
    console.log(`Size: ${stats.size} bytes`);

    if (stats.size > 0) {
        console.log('\n--- Recent Errors ---');
        // Read last 2000 characters
        const buffer = Buffer.alloc(2000);
        const fd = fs.openSync(ERROR_LOG, 'r');
        fs.readSync(fd, buffer, 0, 2000, Math.max(0, stats.size - 2000));
        console.log(buffer.toString());
        fs.closeSync(fd);
    } else {
        console.log('No errors logged.');
    }
} else {
    console.log('Log file not found.');
}
