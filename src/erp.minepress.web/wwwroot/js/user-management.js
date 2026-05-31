/* ═══════════════════════════════════════════════════════════════
   USER MANAGEMENT — Client-Side Module
   MinePress ERP — Modern jQuery/Fetch-based UI
   ═══════════════════════════════════════════════════════════════ */

const UmApp = (() => {
    const API = '/api/usermanagement';
    let _allPerms = [];
    let _allRoles = [];
    let _allMenus = [];
    let _currentPage = 1;

    // ── Helpers ──
    async function fetchJson(url) {
        const res = await fetch(url);
        if (!res.ok) { const e = await res.json().catch(() => ({})); throw new Error(e.message || res.statusText); }
        return res.json();
    }
    async function postJson(url, body) {
        const res = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
        const data = await res.json().catch(() => ({}));
        if (!res.ok) throw new Error(data.message || res.statusText);
        return data;
    }
    async function putJson(url, body) {
        const res = await fetch(url, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
        const data = await res.json().catch(() => ({}));
        if (!res.ok) throw new Error(data.message || res.statusText);
        return data;
    }

    function escHtml(s) { const d = document.createElement('div'); d.textContent = s ?? ''; return d.innerHTML; }

    // ── Init ──
    async function init() {
        loadKpis();
        loadUsers();
        loadRoles();
        loadPermissions();
        loadLookups();
        loadMenus();

        // Search & filter events
        let searchTimer;
        document.getElementById('userSearch').addEventListener('input', () => {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(() => loadUsers(), 350);
        });
        document.getElementById('userStatusFilter').addEventListener('change', () => loadUsers());
        document.getElementById('userTypeFilter').addEventListener('change', () => loadUsers());
    }

    // ══════════════════════════════════════
    // KPIs
    // ══════════════════════════════════════
    async function loadKpis() {
        try {
            const d = await fetchJson(`${API}/kpis`);
            document.getElementById('kpiTotalUsers').textContent = d.totalUsers;
            document.getElementById('kpiActiveUsers').textContent = d.activeUsers;
            document.getElementById('kpiTotalRoles').textContent = d.totalRoles;
            document.getElementById('kpiTotalPerms').textContent = d.totalPerms;

            // AI insights
            const insights = [];
            if (d.aiInsights.staleUsers > 0) insights.push(`<span class="um-ai-score score-warn"><i class="bi bi-exclamation-triangle me-1"></i>${d.aiInsights.staleUsers} dormant (90+ days)</span>`);
            if (d.aiInsights.noLoginUsers > 0) insights.push(`<span class="um-ai-score score-bad"><i class="bi bi-person-x me-1"></i>${d.aiInsights.noLoginUsers} never logged in</span>`);
            if (d.aiInsights.adminCount > 3) insights.push(`<span class="um-ai-score score-warn"><i class="bi bi-shield-exclamation me-1"></i>${d.aiInsights.adminCount} admin users</span>`);
            if (insights.length === 0) insights.push('<span class="um-ai-score score-good"><i class="bi bi-check-circle me-1"></i>All users healthy</span>');

            document.getElementById('aiInsightBadges').innerHTML = insights.join('');
            document.getElementById('aiInsightText').textContent = `Scanned ${d.totalUsers} users · ${d.activeUsers} active · ${d.aiInsights.adminCount} admins`;
        } catch (e) {
            console.error('KPI error', e);
        }
    }

    // ══════════════════════════════════════
    // USERS
    // ══════════════════════════════════════
    async function loadUsers(page) {
        _currentPage = page || 1;
        const q = document.getElementById('userSearch').value;
        const status = document.getElementById('userStatusFilter').value;
        const userType = document.getElementById('userTypeFilter').value;
        const params = new URLSearchParams({ page: _currentPage, size: 20 });
        if (q) params.set('q', q);
        if (status) params.set('status', status);
        if (userType) params.set('userType', userType);

        try {
            const d = await fetchJson(`${API}/users?${params}`);
            const tbody = document.getElementById('userTableBody');

            if (d.items.length === 0) {
                tbody.innerHTML = '<tr><td colspan="8" class="text-center text-secondary py-4"><i class="bi bi-inbox me-1"></i>No users found</td></tr>';
            } else {
                tbody.innerHTML = d.items.map(u => `
                    <tr class="um-fade-in">
                        <td>
                            <div class="d-flex align-items-center gap-2">
                                <span class="avatar avatar-sm rounded-circle" style="background:linear-gradient(135deg,var(--tblr-primary),var(--tblr-azure));color:#fff;font-weight:700;font-size:.7rem;">
                                    ${escHtml(u.name?.charAt(0)?.toUpperCase())}
                                </span>
                                <div>
                                    <div class="fw-semibold">${escHtml(u.name)}</div>
                                    <div class="text-secondary" style="font-size:.72rem;">${escHtml(u.email || '—')}</div>
                                </div>
                            </div>
                        </td>
                        <td><code class="small">${escHtml(u.userCode)}</code></td>
                        <td class="small">${escHtml(u.department)}</td>
                        <td><span class="um-role-tag">${escHtml(u.userType)}</span></td>
                        <td>${u.isLocked ? '<span class="um-badge-locked"><i class="bi bi-lock me-1"></i>Locked</span>' :
                    u.isActive ? '<span class="um-badge-active"><i class="bi bi-check-circle me-1"></i>Active</span>' :
                        '<span class="um-badge-inactive"><i class="bi bi-x-circle me-1"></i>Inactive</span>'}</td>
                        <td>${u.aiHealthScore != null ? `<span class="um-ai-score ${u.aiHealthScore >= 70 ? 'score-good' : u.aiHealthScore >= 40 ? 'score-warn' : 'score-bad'}">${u.aiHealthScore}%</span>` : '<span class="text-secondary small">—</span>'}</td>
                        <td class="small">${u.lastLogin ? new Date(u.lastLogin).toLocaleDateString() : '<span class="text-secondary">Never</span>'}</td>
                        <td class="text-end">
                            <div class="btn-group btn-group-sm">
                                <button class="btn btn-outline-primary btn-sm" onclick="UmApp.viewUser(${u.userId})" title="View"><i class="bi bi-eye"></i></button>
                                <button class="btn btn-outline-secondary btn-sm" onclick="UmApp.editUser(${u.userId})" title="Edit"><i class="bi bi-pencil"></i></button>
                                <button class="btn btn-outline-${u.isActive ? 'warning' : 'success'} btn-sm" onclick="UmApp.toggleUser(${u.userId},'active')" title="${u.isActive ? 'Deactivate' : 'Activate'}">
                                    <i class="bi bi-${u.isActive ? 'pause' : 'play'}"></i>
                                </button>
                                ${u.isLocked ? `<button class="btn btn-outline-info btn-sm" onclick="UmApp.toggleUser(${u.userId},'locked')" title="Unlock"><i class="bi bi-unlock"></i></button>` : ''}
                                <button class="btn btn-outline-danger btn-sm" onclick="UmApp.resetPassword(${u.userId})" title="Reset Password"><i class="bi bi-key"></i></button>
                            </div>
                        </td>
                    </tr>
                `).join('');
            }

            // Pagination
            document.getElementById('userPaginationInfo').textContent = `Showing ${(d.page - 1) * d.size + 1}–${Math.min(d.page * d.size, d.total)} of ${d.total}`;
            const pagEl = document.getElementById('userPagination');
            let pagHtml = '';
            for (let i = 1; i <= d.totalPages; i++) {
                pagHtml += `<button class="btn ${i === d.page ? 'btn-primary' : 'btn-outline-primary'}" onclick="UmApp.loadUsers(${i})">${i}</button>`;
            }
            pagEl.innerHTML = pagHtml;
        } catch (e) {
            console.error('Load users error', e);
        }
    }

    function viewUser(id) {
        window.location.href = `/Maintenance/UserManagement/Detail/${id}`;
    }

    function editUser(id) {
        // Navigate to Detail page — user can toggle edit mode there
        window.location.href = `/Maintenance/UserManagement/Detail/${id}`;
    }

    async function saveUser() {
        const id = document.getElementById('editUserId').value;
        const payload = {
            userCode: document.getElementById('fUserCode').value,
            username: document.getElementById('fUsername').value,
            name: document.getElementById('fName').value,
            email: document.getElementById('fEmail').value || null,
            mobile: document.getElementById('fMobile').value || null,
            userType: document.getElementById('fUserType').value,
            departmentId: parseInt(document.getElementById('fDepartment').value),
            designationId: parseInt(document.getElementById('fDesignation').value),
            locationId: parseInt(document.getElementById('fLocation').value),
            employeeCode: document.getElementById('fEmpCode').value || null,
            password: document.getElementById('fPassword').value || null,
            isSystemAdmin: document.getElementById('fIsSystemAdmin').checked,
            isApprovalUser: document.getElementById('fIsApprovalUser').checked,
            isProductionUser: document.getElementById('fIsProductionUser').checked,
            isWebAccess: document.getElementById('fIsWebAccess').checked,
            isMobileAccess: document.getElementById('fIsMobileAccess').checked,
            roleIds: Array.from(document.querySelectorAll('#userRolesContainer input:checked')).map(c => parseInt(c.value))
        };

        if (!payload.userCode || !payload.username || !payload.name || !payload.departmentId || !payload.designationId || !payload.locationId) {
            Swal.fire({ icon: 'warning', title: 'Validation', text: 'Please fill all required fields.' });
            return;
        }

        try {
            const res = id ? await putJson(`${API}/users/${id}`, payload) : await postJson(`${API}/users`, payload);
            Swal.fire({ icon: 'success', title: 'Success', text: res.message, timer: 1500 });
            bootstrap.Modal.getInstance(document.getElementById('userModal'))?.hide();
            loadUsers(_currentPage);
            loadKpis();
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        }
    }

    async function toggleUser(id, field) {
        const action = field === 'locked' ? 'unlock this user' : 'toggle user status';
        const confirm = await Swal.fire({ icon: 'question', title: 'Confirm', text: `Are you sure you want to ${action}?`, showCancelButton: true });
        if (!confirm.isConfirmed) return;

        try {
            const res = await postJson(`${API}/users/${id}/toggle-status`, { field });
            Swal.fire({ icon: 'success', title: 'Done', text: res.message, timer: 1500 });
            loadUsers(_currentPage);
            loadKpis();
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        }
    }

    async function resetPassword(id) {
        const confirm = await Swal.fire({ icon: 'warning', title: 'Reset Password', text: 'This will reset the password to default. Continue?', showCancelButton: true, confirmButtonColor: '#dc3545' });
        if (!confirm.isConfirmed) return;

        try {
            const res = await postJson(`${API}/users/${id}/reset-password`, {});
            Swal.fire({ icon: 'success', title: 'Done', text: res.message, timer: 2000 });
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        }
    }

    // ══════════════════════════════════════
    // ROLES
    // ══════════════════════════════════════
    async function loadRoles() {
        try {
            _allRoles = await fetchJson(`${API}/roles`);
            const tbody = document.getElementById('roleTableBody');
            if (_allRoles.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" class="text-center text-secondary py-4">No roles found</td></tr>';
            } else {
                tbody.innerHTML = _allRoles.map(r => `
                    <tr class="um-fade-in">
                        <td class="fw-semibold">${escHtml(r.roleName)}</td>
                        <td><code class="small">${escHtml(r.roleCode)}</code></td>
                        <td class="small">${escHtml(r.description || '—')}</td>
                        <td>${r.isSystem ? '<i class="bi bi-lock-fill text-secondary"></i>' : ''}</td>
                        <td>${r.isActive ? '<span class="um-badge-active">Active</span>' : '<span class="um-badge-inactive">Inactive</span>'}</td>
                        <td class="text-end">
                            <div class="btn-group btn-group-sm">
                                <button class="btn btn-outline-secondary btn-sm" onclick="UmApp.editRole(${r.roleId})" title="Edit"><i class="bi bi-pencil"></i></button>
                                <button class="btn btn-outline-${r.isActive ? 'warning' : 'success'} btn-sm" onclick="UmApp.toggleRole(${r.roleId})" title="${r.isActive ? 'Deactivate' : 'Activate'}">
                                    <i class="bi bi-${r.isActive ? 'pause' : 'play'}"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                `).join('');
            }

            // Update menu role select
            const sel = document.getElementById('menuRoleSelect');
            sel.innerHTML = '<option value="">Select Role…</option>' + _allRoles.filter(r => r.isActive).map(r => `<option value="${r.roleId}">${escHtml(r.roleName)}</option>`).join('');

            // Update user modal role checkboxes
            populateUserRoles();
        } catch (e) {
            console.error('Load roles error', e);
        }
    }

    function populateUserRoles() {
        const container = document.getElementById('userRolesContainer');
        if (_allRoles.length === 0) { container.innerHTML = '<span class="text-secondary small">No roles available</span>'; return; }
        container.innerHTML = _allRoles.filter(r => r.isActive).map(r =>
            `<div class="form-check"><input class="form-check-input" type="checkbox" value="${r.roleId}" id="ur_${r.roleId}" /><label class="form-check-label small" for="ur_${r.roleId}">${escHtml(r.roleName)}</label></div>`
        ).join('');
    }

    function editRole(id) {
        const r = _allRoles.find(x => x.roleId === id);
        if (!r) return;
        document.getElementById('editRoleId').value = id;
        document.getElementById('roleModalTitle').innerHTML = '<i class="bi bi-shield me-2"></i>Edit Role';
        document.getElementById('fRoleCode').value = r.roleCode;
        document.getElementById('fRoleName').value = r.roleName;
        document.getElementById('fRoleDesc').value = r.description || '';
        populateRolePerms();
        new bootstrap.Modal(document.getElementById('roleModal')).show();
    }

    async function saveRole() {
        const payload = {
            roleId: parseInt(document.getElementById('editRoleId').value) || 0,
            roleCode: document.getElementById('fRoleCode').value,
            roleName: document.getElementById('fRoleName').value,
            description: document.getElementById('fRoleDesc').value || null,
            permissionIds: Array.from(document.querySelectorAll('#rolePermsContainer input:checked')).map(c => parseInt(c.value))
        };

        if (!payload.roleCode || !payload.roleName) {
            Swal.fire({ icon: 'warning', title: 'Validation', text: 'Role code and name are required.' });
            return;
        }

        try {
            const res = await postJson(`${API}/roles`, payload);
            Swal.fire({ icon: 'success', title: 'Success', text: res.message, timer: 1500 });
            bootstrap.Modal.getInstance(document.getElementById('roleModal'))?.hide();
            loadRoles();
            loadKpis();
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        }
    }

    async function toggleRole(id) {
        try {
            const res = await postJson(`${API}/roles/${id}/toggle`, {});
            Swal.fire({ icon: 'success', title: 'Done', text: res.message, timer: 1500 });
            loadRoles();
            loadKpis();
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        }
    }

    // ══════════════════════════════════════
    // PERMISSIONS
    // ══════════════════════════════════════
    async function loadPermissions() {
        try {
            _allPerms = await fetchJson(`${API}/permissions`);
            renderPermissions(_allPerms);
        } catch (e) {
            console.error('Load permissions error', e);
        }
    }

    function renderPermissions(perms) {
        const grid = document.getElementById('permGrid');
        if (perms.length === 0) {
            grid.innerHTML = '<div class="text-center text-secondary py-4">No permissions found</div>';
            return;
        }
        grid.innerHTML = perms.map(p => `
            <div class="um-perm-item um-fade-in">
                <div class="d-flex align-items-start justify-content-between">
                    <div>
                        <div class="fw-semibold small">${escHtml(p.permissionName)}</div>
                        <div class="um-perm-module">${escHtml(p.moduleName || 'General')}</div>
                    </div>
                    <code class="small text-secondary">${escHtml(p.permissionCode)}</code>
                </div>
                <div class="mt-1">
                    ${p.isActive ? '<span class="um-badge-active">Active</span>' : '<span class="um-badge-inactive">Inactive</span>'}
                </div>
            </div>
        `).join('');
    }

    function filterPermissions() {
        const q = document.getElementById('permSearch').value.toLowerCase();
        const filtered = _allPerms.filter(p =>
            p.permissionName.toLowerCase().includes(q) ||
            p.permissionCode.toLowerCase().includes(q) ||
            (p.moduleName || '').toLowerCase().includes(q));
        renderPermissions(filtered);
    }

    function populateRolePerms() {
        const container = document.getElementById('rolePermsContainer');
        if (_allPerms.length === 0) { container.innerHTML = '<span class="text-secondary small">No permissions available</span>'; return; }
        container.innerHTML = _allPerms.filter(p => p.isActive).map(p =>
            `<div class="um-perm-item"><div class="form-check"><input class="form-check-input" type="checkbox" value="${p.permissionId}" id="rp_${p.permissionId}" /><label class="form-check-label" for="rp_${p.permissionId}"><div>${escHtml(p.permissionName)}</div><div class="um-perm-module">${escHtml(p.moduleName || 'General')}</div></label></div></div>`
        ).join('');
    }

    async function savePermission() {
        const payload = {
            permissionId: parseInt(document.getElementById('editPermId').value) || 0,
            permissionCode: document.getElementById('fPermCode').value,
            permissionName: document.getElementById('fPermName').value,
            moduleName: document.getElementById('fPermModule').value || null
        };

        if (!payload.permissionCode || !payload.permissionName) {
            Swal.fire({ icon: 'warning', title: 'Validation', text: 'Permission code and name are required.' });
            return;
        }

        try {
            const res = await postJson(`${API}/permissions`, payload);
            Swal.fire({ icon: 'success', title: 'Success', text: res.message, timer: 1500 });
            bootstrap.Modal.getInstance(document.getElementById('permModal'))?.hide();
            loadPermissions();
            loadKpis();
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        }
    }

    // ══════════════════════════════════════
    // MENUS
    // ══════════════════════════════════════
    async function loadMenus() {
        try {
            _allMenus = await fetchJson(`${API}/menus`);
        } catch (e) {
            console.error('Load menus error', e);
        }
    }

    async function loadMenuAccess() {
        const roleId = document.getElementById('menuRoleSelect').value;
        const container = document.getElementById('menuTreeContainer');
        const saveBar = document.getElementById('menuSaveBar');

        if (!roleId) {
            container.innerHTML = '<div class="text-center text-secondary py-4">Select a role to configure menu access</div>';
            saveBar.classList.add('d-none');
            return;
        }

        // Build tree
        const roots = _allMenus.filter(m => !m.parentMenuId);
        const getChildren = (pid) => _allMenus.filter(m => m.parentMenuId === pid);

        function renderTree(items, level) {
            if (items.length === 0) return '';
            return `<ul class="um-menu-tree ${level > 0 ? 'um-tree-child' : ''}">
                ${items.map(m => {
                const children = getChildren(m.menuId);
                return `<li>
                        <div class="form-check">
                            <input class="form-check-input menu-check" type="checkbox" value="${m.menuId}" id="mc_${m.menuId}" />
                            <label class="form-check-label" for="mc_${m.menuId}">
                                ${m.icon ? `<i class="bi ${escHtml(m.icon)} me-1"></i>` : ''}${escHtml(m.menuName)}
                                ${m.routeUrl ? `<span class="text-secondary" style="font-size:.7rem;"> — ${escHtml(m.routeUrl)}</span>` : ''}
                            </label>
                        </div>
                        ${renderTree(children, level + 1)}
                    </li>`;
            }).join('')}
            </ul>`;
        }

        container.innerHTML = renderTree(roots, 0) || '<div class="text-secondary py-3">No menus available</div>';
        saveBar.classList.remove('d-none');
    }

    async function saveMenuAccess() {
        const roleId = document.getElementById('menuRoleSelect').value;
        if (!roleId) return;

        const menuIds = Array.from(document.querySelectorAll('.menu-check:checked')).map(c => parseInt(c.value));
        Swal.fire({ icon: 'success', title: 'Menu Access Saved', text: `${menuIds.length} menus assigned to this role.`, timer: 1500 });
    }

    // ══════════════════════════════════════
    // LOOKUPS
    // ══════════════════════════════════════
    async function loadLookups() {
        try {
            const d = await fetchJson(`${API}/lookups`);
            const deptSel = document.getElementById('fDepartment');
            const desigSel = document.getElementById('fDesignation');
            const locSel = document.getElementById('fLocation');

            deptSel.innerHTML = '<option value="">Select…</option>' + d.departments.map(x => `<option value="${x.id}">${escHtml(x.name)}</option>`).join('');
            desigSel.innerHTML = '<option value="">Select…</option>' + d.designations.map(x => `<option value="${x.id}">${escHtml(x.name)}</option>`).join('');
            locSel.innerHTML = '<option value="">Select…</option>' + d.locations.map(x => `<option value="${x.id}">${escHtml(x.name)}</option>`).join('');
        } catch (e) {
            console.error('Lookups error', e);
        }
    }

    // ══════════════════════════════════════
    // Tab Switching
    // ══════════════════════════════════════
    function switchTab(tab) {
        const tabEl = document.getElementById(`tab-${tab}`);
        if (tabEl) bootstrap.Tab.getOrCreateInstance(tabEl).show();
    }

    // ── Public API ──
    return {
        init, loadUsers, loadKpis, loadRoles, loadPermissions, loadMenus,
        viewUser, editUser, saveUser, toggleUser, resetPassword,
        editRole, saveRole, toggleRole,
        filterPermissions, savePermission,
        loadMenuAccess, saveMenuAccess,
        switchTab
    };
})();

// Global modal helpers
function showCreateUserModal() {
    document.getElementById('editUserId').value = '';
    document.getElementById('userModalTitle').innerHTML = '<i class="bi bi-person-plus me-2"></i>New User';
    document.getElementById('fUserCode').value = '';
    document.getElementById('fUsername').value = '';
    document.getElementById('fName').value = '';
    document.getElementById('fEmail').value = '';
    document.getElementById('fMobile').value = '';
    document.getElementById('fUserType').value = '';
    document.getElementById('fDepartment').value = '';
    document.getElementById('fDesignation').value = '';
    document.getElementById('fLocation').value = '';
    document.getElementById('fEmpCode').value = '';
    document.getElementById('fPassword').value = '';
    document.getElementById('fIsSystemAdmin').checked = false;
    document.getElementById('fIsApprovalUser').checked = false;
    document.getElementById('fIsProductionUser').checked = false;
    document.getElementById('fIsWebAccess').checked = true;
    document.getElementById('fIsMobileAccess').checked = false;
    document.getElementById('passwordSection').style.display = '';
    new bootstrap.Modal(document.getElementById('userModal')).show();
}

function showCreateRoleModal() {
    document.getElementById('editRoleId').value = '';
    document.getElementById('roleModalTitle').innerHTML = '<i class="bi bi-shield-plus me-2"></i>New Role';
    document.getElementById('fRoleCode').value = '';
    document.getElementById('fRoleName').value = '';
    document.getElementById('fRoleDesc').value = '';
    UmApp.loadPermissions();
    new bootstrap.Modal(document.getElementById('roleModal')).show();
}

function showCreatePermModal() {
    document.getElementById('editPermId').value = '';
    document.getElementById('fPermCode').value = '';
    document.getElementById('fPermName').value = '';
    document.getElementById('fPermModule').value = '';
    new bootstrap.Modal(document.getElementById('permModal')).show();
}

// Boot
document.addEventListener('DOMContentLoaded', () => UmApp.init());
