const STORAGE_KEY = 'avasec_admin_secret';

function getSecret() {
    return sessionStorage.getItem(STORAGE_KEY) || '';
}

function setSecret(value) {
    if (value) sessionStorage.setItem(STORAGE_KEY, value);
    else sessionStorage.removeItem(STORAGE_KEY);
}

function apiHeaders() {
    return {
        'Content-Type': 'application/json',
        'x-admin-secret': getSecret()
    };
}

async function adminFetch(url, options = {}) {
    const res = await fetch(url, {
        ...options,
        headers: { ...apiHeaders(), ...(options.headers || {}) }
    });
    const data = await res.json().catch(() => ({}));
    if (res.status === 403) {
        setSecret('');
        showAuthGate();
        throw new Error('Hết phiên admin / Invalid secret');
    }
    if (!res.ok && !data.success) {
        throw new Error(data.message || `HTTP ${res.status}`);
    }
    return data;
}

function showMsg(text, isError = false) {
    const el = document.getElementById('globalMsg');
    if (!el) return;
    el.textContent = text;
    el.className = 'msg ' + (isError ? 'err' : 'ok');
    el.style.display = 'block';
    setTimeout(() => { el.style.display = 'none'; }, 4000);
}

function showAuthGate() {
    document.getElementById('authGate').style.display = 'block';
    document.getElementById('adminApp').style.display = 'none';
}

function showAdminApp() {
    document.getElementById('authGate').style.display = 'none';
    document.getElementById('adminApp').style.display = 'block';
}

function copyText(text) {
    navigator.clipboard.writeText(text).then(() => showMsg('Đã copy key!'));
}

function formatDate(d) {
    if (!d) return '—';
    return new Date(d).toLocaleString('vi-VN');
}

function renderStats(poolStats, licStats, userStats) {
    const row = document.getElementById('statsRow');
    const p = poolStats || { available: 0, redeemed: 0, total: 0 };
    const l = licStats || { active: 0, inactive: 0, total: 0 };
    const u = userStats || { total: 0, active: 0 };
    row.innerHTML = `
        <div class="stat-card"><div class="num">${u.total ?? 0}</div><div class="lbl">Tổng user</div></div>
        <div class="stat-card"><div class="num">${u.active ?? 0}</div><div class="lbl">User đang bật</div></div>
        <div class="stat-card"><div class="num">${p.available ?? 0}</div><div class="lbl">Key chưa dùng</div></div>
        <div class="stat-card"><div class="num">${l.active ?? 0}</div><div class="lbl">License active</div></div>
    `;
}

async function loadUsers() {
    const container = document.getElementById('usersTable');
    if (!container) return null;
    container.innerHTML = '<p style="color:#94a3b8;font-size:13px;">Đang tải danh sách...</p>';

    try {
        const data = await adminFetch('/api/auth/admin/users');

        if (!data.users || data.users.length === 0) {
            container.innerHTML = '<p style="color:#64748b;font-size:13px;">Chưa có user. Tạo ở form trên hoặc đăng ký tại <a href="index.html">trang login</a>.</p>';
            return data.stats;
        }

        const now = new Date();
        container.innerHTML = `
        <table>
            <thead><tr>
                <th>ID</th><th>Username</th><th>Email</th><th>License</th><th>Trạng thái</th><th>Thao tác</th>
            </tr></thead>
            <tbody>
                ${data.users.map(u => {
                    const hasLic = !!u.LicenseKey;
                    const licOk = hasLic && u.LicenseActive && new Date(u.ExpiryDate) > now;
                    return `
                <tr>
                    <td>${u.UserId}</td>
                    <td><strong>${escapeHtml(u.Username)}</strong><br>
                        <span style="color:#64748b;font-size:11px;">${formatDate(u.CreatedAt)}</span></td>
                    <td>${escapeHtml(u.Email)}</td>
                    <td>${hasLic
                        ? `<span class="key-mono">${u.LicenseKey}</span><br>
                           <span style="font-size:11px;color:#94a3b8;">${u.LicenseType} · ${formatDate(u.ExpiryDate)}</span>`
                        : '<span class="badge badge-warn">Chưa có</span>'}</td>
                    <td>${u.IsActive
                        ? (licOk ? '<span class="badge badge-ok">OK</span>' : '<span class="badge badge-warn">No license</span>')
                        : '<span class="badge badge-off">Khóa</span>'}</td>
                    <td style="white-space:nowrap;">
                        <button class="btn btn-ghost btn-sm" data-reset-pw="${u.UserId}" title="Đổi mật khẩu">🔑</button>
                        <button class="btn btn-ghost btn-sm" data-grant="${u.UserId}" title="Cấp Trial 14 ngày">🎁</button>
                        <button class="btn btn-ghost btn-sm" data-toggle="${u.UserId}" data-active="${u.IsActive ? '1' : '0'}">
                            ${u.IsActive ? '🔒' : '✅'}</button>
                    </td>
                </tr>`;
                }).join('')}
            </tbody>
        </table>`;

        container.querySelectorAll('[data-reset-pw]').forEach(btn => {
            btn.addEventListener('click', async () => {
                const pw = prompt('Mật khẩu mới (tối thiểu 4 ký tự):');
                if (!pw || pw.length < 4) return;
                try {
                    await adminFetch(`/api/auth/admin/users/${btn.dataset.resetPw}`, {
                        method: 'PATCH',
                        body: JSON.stringify({ password: pw })
                    });
                    showMsg('Đã đổi mật khẩu');
                } catch (e) { showMsg(e.message, true); }
            });
        });

        container.querySelectorAll('[data-grant]').forEach(btn => {
            btn.addEventListener('click', async () => {
                try {
                    const r = await adminFetch(`/api/auth/admin/users/${btn.dataset.grant}/grant-trial`, {
                        method: 'POST',
                        body: JSON.stringify({ days: 14 })
                    });
                    showMsg('Trial: ' + r.licenseKey);
                    await refreshAll();
                } catch (e) { showMsg(e.message, true); }
            });
        });

        container.querySelectorAll('[data-toggle]').forEach(btn => {
            btn.addEventListener('click', async () => {
                const active = btn.dataset.active !== '1';
                const action = active ? 'Mở khóa' : 'Khóa';
                if (!confirm(`${action} user #${btn.dataset.toggle}?`)) return;
                try {
                    await adminFetch(`/api/auth/admin/users/${btn.dataset.toggle}`, {
                        method: 'PATCH',
                        body: JSON.stringify({ isActive: active })
                    });
                    showMsg('Đã cập nhật');
                    await refreshAll();
                } catch (e) { showMsg(e.message, true); }
            });
        });

        return data.stats;
    } catch (e) {
        container.innerHTML = `<p class="msg err" style="display:block;">Không tải được user: ${escapeHtml(e.message)}. Kiểm tra MySQL (XAMPP) và Admin Secret.</p>`;
        throw e;
    }
}

function escapeHtml(s) {
    return String(s || '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
}

async function loadPool() {
    const status = document.getElementById('poolFilter').value;
    const data = await adminFetch(`/api/license/admin/pool?status=${status}`);
    const container = document.getElementById('poolTable');

    if (!data.keys || data.keys.length === 0) {
        container.innerHTML = '<p style="color:#64748b;font-size:13px;">Không có key nào.</p>';
        return data.stats;
    }

    container.innerHTML = `
        <table>
            <thead><tr>
                <th>Key</th><th>Gói</th><th>Trạng thái</th><th>Tạo lúc</th><th></th>
            </tr></thead>
            <tbody>
                ${data.keys.map(k => `
                <tr>
                    <td class="key-mono">${k.LicenseKey}
                        <button class="btn btn-ghost btn-sm" onclick="copyText('${k.LicenseKey}')" title="Copy">📋</button>
                    </td>
                    <td>${k.LicenseType}</td>
                    <td>${k.IsRedeemed
                        ? `<span class="badge badge-warn">Đã dùng #${k.RedeemedByUserId || '?'}</span>`
                        : '<span class="badge badge-ok">Chưa dùng</span>'}</td>
                    <td>${formatDate(k.CreatedAt)}</td>
                    <td>${!k.IsRedeemed
                        ? `<button class="btn btn-danger btn-sm" data-delete-pool="${k.PoolId}">Xóa</button>`
                        : ''}</td>
                </tr>`).join('')}
            </tbody>
        </table>`;

    container.querySelectorAll('[data-delete-pool]').forEach(btn => {
        btn.addEventListener('click', async () => {
            if (!confirm('Xóa key chưa dùng này?')) return;
            try {
                await adminFetch(`/api/license/admin/pool/${btn.dataset.deletePool}`, { method: 'DELETE' });
                showMsg('Đã xóa key');
                await refreshAll();
            } catch (e) {
                showMsg(e.message, true);
            }
        });
    });

    return data.stats;
}

async function loadLicenses() {
    const data = await adminFetch('/api/license/admin/licenses');
    const container = document.getElementById('licenseTable');

    if (!data.licenses || data.licenses.length === 0) {
        container.innerHTML = '<p style="color:#64748b;font-size:13px;">Chưa có license nào.</p>';
        return data.stats;
    }

    const now = new Date();
    container.innerHTML = `
        <table>
            <thead><tr>
                <th>User</th><th>Key</th><th>Gói</th><th>Hết hạn</th><th>Trạng thái</th><th></th>
            </tr></thead>
            <tbody>
                ${data.licenses.map(l => {
                    const expired = new Date(l.ExpiryDate) <= now;
                    const active = l.IsActive && !expired;
                    return `
                <tr>
                    <td><strong>${l.Username}</strong><br><span style="color:#64748b;font-size:11px;">${l.Email}</span></td>
                    <td class="key-mono">${l.LicenseKey}
                        <button class="btn btn-ghost btn-sm" onclick="copyText('${l.LicenseKey}')">📋</button>
                    </td>
                    <td>${l.LicenseType}</td>
                    <td>${formatDate(l.ExpiryDate)}</td>
                    <td>${active
                        ? '<span class="badge badge-ok">Active</span>'
                        : '<span class="badge badge-off">Off / Hết hạn</span>'}</td>
                    <td>${active
                        ? `<button class="btn btn-danger btn-sm" data-revoke="${l.LicenseId}">Thu hồi</button>`
                        : ''}</td>
                </tr>`;
                }).join('')}
            </tbody>
        </table>`;

    container.querySelectorAll('[data-revoke]').forEach(btn => {
        btn.addEventListener('click', async () => {
            if (!confirm('Thu hồi license này? User sẽ mất quyền Pro.')) return;
            try {
                await adminFetch('/api/license/admin/revoke-license', {
                    method: 'POST',
                    body: JSON.stringify({ licenseId: parseInt(btn.dataset.revoke, 10) })
                });
                showMsg('Đã thu hồi license');
                await refreshAll();
            } catch (e) {
                showMsg(e.message, true);
            }
        });
    });

    return data.stats;
}

async function refreshAll() {
    let userStats = null;
    let poolStats = null;
    let licStats = null;
    try {
        userStats = await loadUsers();
    } catch (e) {
        showMsg('Lỗi tải user: ' + e.message, true);
    }
    try {
        poolStats = await loadPool();
    } catch (e) {
        showMsg('Lỗi tải kho key: ' + e.message, true);
    }
    try {
        licStats = await loadLicenses();
    } catch (e) {
        showMsg('Lỗi tải license: ' + e.message, true);
    }
    renderStats(poolStats, licStats, userStats);
}

// Tabs
document.querySelectorAll('.tab').forEach(tab => {
    tab.addEventListener('click', () => {
        document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
        document.querySelectorAll('.panel').forEach(p => p.classList.remove('active'));
        tab.classList.add('active');
        document.getElementById('panel-' + tab.dataset.tab).classList.add('active');
    });
});

document.getElementById('createBtn')?.addEventListener('click', async () => {
    const planId = document.getElementById('planId').value;
    const count = parseInt(document.getElementById('count').value, 10) || 1;
    const out = document.getElementById('createResult');
    out.innerHTML = '<p style="color:#94a3b8;">Đang tạo...</p>';
    try {
        const data = await adminFetch('/api/license/admin/create-keys', {
            method: 'POST',
            body: JSON.stringify({ planId, count })
        });
        out.innerHTML = `
            <p class="msg ok">Đã tạo ${data.keys.length} key (${data.plan})</p>
            <ul style="list-style:none;padding:0;margin:8px 0;">
                ${data.keys.map(k => `<li class="key-mono" style="margin:6px 0;">${k}
                    <button class="btn btn-ghost btn-sm" onclick="copyText('${k}')">📋 Copy</button></li>`).join('')}
            </ul>`;
        await refreshAll();
    } catch (e) {
        out.innerHTML = `<p class="msg err">${e.message}</p>`;
    }
});

document.getElementById('refreshPool')?.addEventListener('click', () => loadPool().then(s => renderStats(s, null, null)).catch(e => showMsg(e.message, true)));
document.getElementById('refreshLicenses')?.addEventListener('click', () => loadLicenses().then(s => renderStats(null, s, null)).catch(e => showMsg(e.message, true)));
document.getElementById('refreshUsers')?.addEventListener('click', () => loadUsers().then(s => renderStats(null, null, s)).catch(e => showMsg(e.message, true)));
document.getElementById('poolFilter')?.addEventListener('change', () => loadPool().catch(e => showMsg(e.message, true)));

document.getElementById('createUserBtn')?.addEventListener('click', async () => {
    const username = document.getElementById('newUsername').value.trim();
    const email = document.getElementById('newEmail').value.trim();
    const password = document.getElementById('newPassword').value;
    if (!username || !email || !password) {
        showMsg('Nhập đủ username, email, password', true);
        return;
    }
    try {
        const r = await adminFetch('/api/auth/admin/users', {
            method: 'POST',
            body: JSON.stringify({ username, email, password, grantTrial: true })
        });
        showMsg(`Đã tạo ${username}` + (r.licenseKey ? ` · Trial: ${r.licenseKey}` : ''));
        document.getElementById('newUsername').value = '';
        document.getElementById('newEmail').value = '';
        document.getElementById('newPassword').value = '';
        await refreshAll();
    } catch (e) { showMsg(e.message, true); }
});

// Init — hỗ trợ ?secret=dev-secret trên URL (dev)
const urlSecret = new URLSearchParams(window.location.search).get('secret');
if (urlSecret) {
    document.getElementById('authSecret').value = urlSecret;
}

async function tryAdminLogin() {
    if (!getSecret()) return showAuthGate();
    try {
        await adminFetch('/api/license/admin/pool?status=available');
        showAdminApp();
        await refreshAll();
    } catch {
        setSecret('');
        showAuthGate();
    }
}

document.getElementById('authBtn')?.addEventListener('click', async () => {
    const secret = document.getElementById('authSecret').value.trim();
    const errEl = document.getElementById('authError');
    if (!secret) {
        errEl.textContent = 'Nhập ADMIN_SECRET';
        errEl.style.display = 'block';
        return;
    }
    setSecret(secret);
    try {
        await adminFetch('/api/license/admin/pool?status=available');
        showAdminApp();
        await refreshAll();
    } catch (e) {
        setSecret('');
        errEl.textContent = e.message || 'Sai mật khẩu admin';
        errEl.style.display = 'block';
    }
});

document.getElementById('logoutBtn')?.addEventListener('click', () => {
    setSecret('');
    showAuthGate();
});

// Init
if (urlSecret) {
    setSecret(urlSecret);
    tryAdminLogin();
} else if (getSecret()) {
    tryAdminLogin();
} else {
    showAuthGate();
}

window.copyText = copyText;
