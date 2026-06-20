const API_BASE = '';

async function loadPlans() {
    try {
        const res = await fetch(`${API_BASE}/api/license/plans`);
        const data = await res.json();
        const container = document.getElementById('plans');
        if (!container || !data.plans) return;

        container.innerHTML = data.plans.map(p => `
            <div class="col-md-4">
                <div class="glass-panel p-3 h-100" style="text-align:center;">
                    <h3 style="color:#fff;font-size:18px;">${p.name}</h3>
                    <p style="color:#10B981;font-size:24px;font-weight:700;">$${p.price}</p>
                    <p style="color:#94A3B8;font-size:12px;">${p.days} days / ngày</p>
                    <p style="color:#64748B;font-size:11px;">Thanh toán → Admin cấp key<br>Pay → receive key by email</p>
                </div>
            </div>
        `).join('');
    } catch (e) {
        console.error(e);
    }
}

document.getElementById('activateBtn')?.addEventListener('click', async () => {
    const userJson = localStorage.getItem('avasec_user');
    if (!userJson) {
        alert('Vui lòng đăng nhập trước / Please login first');
        window.location.href = '/index.html';
        return;
    }
    const user = JSON.parse(userJson);
    const licenseKey = document.getElementById('licenseKey').value.trim();
    if (!licenseKey) {
        alert('Nhập license key / Enter license key');
        return;
    }

    const btn = document.getElementById('activateBtn');
    btn.disabled = true;
    try {
        const res = await fetch(`${API_BASE}/api/license/activate`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ licenseKey, userId: user.userId })
        });
        const data = await res.json();
        alert(data.message || (data.success ? 'OK' : 'Failed'));
        if (data.success) {
            window.location.href = '/dashboard.html';
        }
    } catch (e) {
        alert('Connection error / Lỗi kết nối');
    } finally {
        btn.disabled = false;
    }
});

loadPlans();
