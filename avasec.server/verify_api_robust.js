const API_BASE = 'http://localhost:3000/api';

async function runTests() {
    console.log('--- STARTING API VERIFICATION TESTS (Fixed Data Access) ---');

    try {
        // 1. Health Check
        const healthRes = await fetch(`${API_BASE}/health`);
        const health = await healthRes.json();
        console.log('✅ Health Check Status:', health.status);

        // 2. Admin Login
        console.log('Attempting Admin Login...');
        const loginRes = await fetch(`${API_BASE}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                username: 'admin',
                password: 'admin123'
            })
        });
        const login = await loginRes.json();

        if (login.success) {
            console.log('✅ Admin Login: SUCCESS');
            const userId = login.userId;
            console.log(`Logined User ID: ${userId}`);

            // 3. Send Notification Test
            console.log('Sending Test Notification...');
            const notifyRes = await fetch(`${API_BASE}/admin/notifications/send`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    title: 'Test từ AI (v3)',
                    message: 'Thông báo này được gửi tự động và xác nhận qua script.',
                    type: 'info',
                    target: 'all'
                })
            });
            const notify = await notifyRes.json();

            if (notify.success) {
                console.log('✅ Send Notification: SUCCESS');
            } else {
                console.log('❌ Send Notification: FAILED', notify.message);
            }

            // 4. Check Notifications for current user
            console.log(`Checking notifications for User ID: ${userId}...`);
            const myNotifyRes = await fetch(`${API_BASE}/notifications/${userId}?includeRead=true`);
            const myNotifyResult = await myNotifyRes.json();

            if (myNotifyResult.success && Array.isArray(myNotifyResult.data)) {
                console.log(`✅ Received Notifications count: ${myNotifyResult.data.length}`);
                if (myNotifyResult.data.length > 0) {
                    console.log('Latest notification title:', myNotifyResult.data[0].Title);
                }
            } else {
                console.log('❌ Failed to fetch notifications data structure correctly');
            }

        } else {
            console.log('❌ Admin Login: FAILED', login.message);
        }

    } catch (error) {
        console.error('❌ Test failed with error:', error.message);
    }

    console.log('--- TESTS COMPLETED ---');
}

runTests();
