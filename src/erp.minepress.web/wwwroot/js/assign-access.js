/* ═══════════════════════════════════════════════════════════════
   ASSIGN ACCESS — Client-Side Module
   MinePress ERP — Role & Permission Assignment Page
   ═══════════════════════════════════════════════════════════════ */

const AaApp = (() => {
    const API = '/api/usermanagement';
    let _user = null;
    let _roles = [];
    let _permissions = [];
    let _originalRoleIds = new Set();
    let _originalPermIds = new Set();

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
    function esc(s) { const d = document.createElement('div'); d.textContent = s ?? ''; return d.innerHTML; }

    function getUserId() {
        return document.getElementById('aaUserId')?.value;
    }

    // ── Init ──
    async function init() {
        const id = getUserId();
        if (!id) return;
        try {
            await Promise.all([loadUser(), loadRoles(), loadPermissions()]);
            updateSummary();
            updateChangeCount();
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        }
    }

    // ══════════════════════════════════════
    // LOAD USER
    // ══════════════════════════════════════
    async function loadUser() {
        _user = await fetchJson(`${API}/users/${getUserId()}`);
        renderHero();
    }

    function renderHero() {
        const u = _user;
        document.getElementById('aaAvatar').textContent = u.name?.charAt(0)?.toUpperCase() || '?';
        document.getElementById('aaHeroName').textContent = u.name || '—';
        document.getElementById('aaHeroCode').innerHTML = `<code>${esc(u.userCode)}</code>`;
        document.getElementById('aaHeroUsername').textContent = u.username || '';

        let badge = '';
        if (u.isLocked) badge = '<span class="badge bg-warning-lt text-warning"><i class="bi bi-lock me-1"></i>Locked</span>';
        else if (u.isActive) badge = '<span class="badge bg-success-lt text-success"><i class="bi bi-check-circle me-1"></i>Active</span>';
        else badge = '<span class="badge bg-danger-lt text-danger"><i class="bi bi-x-circle me-1"></i>Inactive</span>';
        document.getElementById('aaHeroStatus').innerHTML = badge;
    }

    // ══════════════════════════════════════
    // ROLES
    // ══════════════════════════════════════
    async function loadRoles() {
        const data = await fetchJson(`${API}/users/${getUserId()}/roles`);
        _roles = data.roles || [];
        _originalRoleIds = new Set(_roles.filter(r => r.isAssigned).map(r => r.roleId));
        renderRoles();
    }

    function renderRoles() {
        const grid = document.getElementById('aaRoleGrid');
        const assigned = _roles.filter(r => r.isAssigned).length;
        document.getElementById('aaRoleCount').textContent = `${assigned} of ${_roles.length} assigned`;

        if (_roles.length === 0) {
            grid.innerHTML = '<div class="text-center text-secondary py-4">No roles available</div>';
            return;
        }

        grid.innerHTML = _roles.map(r => `
            <label class="aa-role-card aa-fade-in ${r.isAssigned ? 'assigned' : ''}" id="aaRoleCard_${r.roleId}" for="aaRole_${r.roleId}">
                <div class="aa-role-icon"><i class="bi bi-shield"></i></div>
                <div class="flex-fill">
                    <div class="aa-role-name">${esc(r.roleName)}</div>
                    <div class="aa-role-code">${esc(r.roleCode)}</div>
                    ${r.description ? `<div class="aa-role-desc">${esc(r.description)}</div>` : ''}
                </div>
                <div class="form-check">
                    <input class="form-check-input aa-role-cb" type="checkbox" value="${r.roleId}" id="aaRole_${r.roleId}" ${r.isAssigned ? 'checked' : ''} onchange="AaApp.onRoleChange(this)" />
                </div>
            </label>
        `).join('');
    }

    function onRoleChange(cb) {
        const card = cb.closest('.aa-role-card');
        if (cb.checked) {
            card.classList.add('assigned');
        } else {
            card.classList.remove('assigned');
        }
        updateChangeCount();
        updateRoleCount();
    }

    function updateRoleCount() {
        const assigned = document.querySelectorAll('.aa-role-cb:checked').length;
        document.getElementById('aaRoleCount').textContent = `${assigned} of ${_roles.length} assigned`;
    }

    function filterRoles() {
        const term = document.getElementById('aaRoleSearch').value.toLowerCase();
        _roles.forEach(r => {
            const card = document.getElementById(`aaRoleCard_${r.roleId}`);
            if (!card) return;
            const match = (r.roleName || '').toLowerCase().includes(term) ||
                          (r.roleCode || '').toLowerCase().includes(term) ||
                          (r.description || '').toLowerCase().includes(term);
            card.style.display = match ? '' : 'none';
        });
    }

    function selectAllRoles() {
        document.querySelectorAll('.aa-role-cb').forEach(cb => {
            if (cb.closest('.aa-role-card').style.display !== 'none') {
                cb.checked = true;
                cb.closest('.aa-role-card').classList.add('assigned');
            }
        });
        updateChangeCount();
        updateRoleCount();
    }

    function clearAllRoles() {
        document.querySelectorAll('.aa-role-cb').forEach(cb => {
            if (cb.closest('.aa-role-card').style.display !== 'none') {
                cb.checked = false;
                cb.closest('.aa-role-card').classList.remove('assigned');
            }
        });
        updateChangeCount();
        updateRoleCount();
    }

    // ══════════════════════════════════════
    // PERMISSIONS
    // ══════════════════════════════════════
    async function loadPermissions() {
        const data = await fetchJson(`${API}/users/${getUserId()}/permissions`);
        _permissions = data.permissions || [];
        _originalPermIds = new Set(_permissions.filter(p => p.isAssigned).map(p => p.permissionId));
        renderPermissions();
    }

    function renderPermissions() {
        const grid = document.getElementById('aaPermGrid');
        const assigned = _permissions.filter(p => p.isAssigned).length;
        document.getElementById('aaPermCount').textContent = `${assigned} of ${_permissions.length} assigned`;

        if (_permissions.length === 0) {
            grid.innerHTML = '<div class="text-center text-secondary py-4">No permissions available</div>';
            return;
        }

        const grouped = {};
        _permissions.forEach(p => {
            const mod = p.moduleName || 'General';
            if (!grouped[mod]) grouped[mod] = [];
            grouped[mod].push(p);
        });

        grid.innerHTML = Object.entries(grouped).map(([mod, perms]) => {
            const assignedCount = perms.filter(p => p.isAssigned).length;
            return `
                <div class="aa-perm-group aa-fade-in" id="aaPermGroup_${mod.replace(/\s+/g, '_')}">
                    <div class="aa-perm-group-header" onclick="AaApp.togglePermGroup(this)">
                        <div class="d-flex align-items-center gap-2 flex-fill">
                            <i class="bi bi-folder"></i>
                            <span>${esc(mod)}</span>
                            <span class="badge bg-secondary-lt ms-1 aa-perm-badge" data-module="${esc(mod)}">${assignedCount}/${perms.length}</span>
                        </div>
                        <div class="d-flex align-items-center gap-1">
                            <button class="btn btn-sm btn-link p-0 text-primary" onclick="event.stopPropagation(); AaApp.selectModulePerms('${esc(mod)}')" title="Select all in module">
                                <i class="bi bi-check-all" style="font-size:.85rem;"></i>
                            </button>
                            <button class="btn btn-sm btn-link p-0 text-secondary" onclick="event.stopPropagation(); AaApp.clearModulePerms('${esc(mod)}')" title="Clear all in module">
                                <i class="bi bi-x-lg" style="font-size:.7rem;"></i>
                            </button>
                            <i class="bi bi-chevron-down aa-perm-chevron ms-1"></i>
                        </div>
                    </div>
                    <div class="aa-perm-group-body">
                        ${perms.map(p => `
                            <label class="aa-perm-item ${p.isAssigned ? 'assigned' : ''}" id="aaPermItem_${p.permissionId}" for="aaPerm_${p.permissionId}">
                                <div class="form-check">
                                    <input class="form-check-input aa-perm-cb" type="checkbox" value="${p.permissionId}" id="aaPerm_${p.permissionId}" data-module="${esc(mod)}" ${p.isAssigned ? 'checked' : ''} onchange="AaApp.onPermChange(this)" />
                                    <span class="aa-perm-name">${esc(p.permissionName)}</span>
                                    <span class="aa-perm-code">${esc(p.permissionCode)}</span>
                                </div>
                            </label>
                        `).join('')}
                    </div>
                </div>
            `;
        }).join('');
    }

    function onPermChange(cb) {
        const item = cb.closest('.aa-perm-item');
        if (cb.checked) {
            item.classList.add('assigned');
        } else {
            item.classList.remove('assigned');
        }
        updateModuleBadge(cb.dataset.module);
        updateChangeCount();
        updatePermCount();
    }

    function updatePermCount() {
        const assigned = document.querySelectorAll('.aa-perm-cb:checked').length;
        document.getElementById('aaPermCount').textContent = `${assigned} of ${_permissions.length} assigned`;
    }

    function updateModuleBadge(mod) {
        const badge = document.querySelector(`.aa-perm-badge[data-module="${mod}"]`);
        if (!badge) return;
        const cbs = document.querySelectorAll(`.aa-perm-cb[data-module="${mod}"]`);
        const checked = Array.from(cbs).filter(c => c.checked).length;
        badge.textContent = `${checked}/${cbs.length}`;
    }

    function togglePermGroup(header) {
        const group = header.closest('.aa-perm-group');
        group.classList.toggle('collapsed');
    }

    function filterPerms() {
        const term = document.getElementById('aaPermSearch').value.toLowerCase();
        const groups = document.querySelectorAll('.aa-perm-group');
        groups.forEach(group => {
            const items = group.querySelectorAll('.aa-perm-item');
            let anyVisible = false;
            items.forEach(item => {
                const name = item.querySelector('.aa-perm-name')?.textContent?.toLowerCase() || '';
                const code = item.querySelector('.aa-perm-code')?.textContent?.toLowerCase() || '';
                const match = name.includes(term) || code.includes(term);
                item.style.display = match ? '' : 'none';
                if (match) anyVisible = true;
            });
            group.style.display = anyVisible ? '' : 'none';
        });
    }

    function selectAllPerms() {
        document.querySelectorAll('.aa-perm-item').forEach(item => {
            if (item.style.display !== 'none') {
                const cb = item.querySelector('.aa-perm-cb');
                cb.checked = true;
                item.classList.add('assigned');
            }
        });
        _permissions.forEach(p => {
            const mod = p.moduleName || 'General';
            updateModuleBadge(mod);
        });
        updateChangeCount();
        updatePermCount();
    }

    function clearAllPerms() {
        document.querySelectorAll('.aa-perm-item').forEach(item => {
            if (item.style.display !== 'none') {
                const cb = item.querySelector('.aa-perm-cb');
                cb.checked = false;
                item.classList.remove('assigned');
            }
        });
        _permissions.forEach(p => {
            const mod = p.moduleName || 'General';
            updateModuleBadge(mod);
        });
        updateChangeCount();
        updatePermCount();
    }

    function selectModulePerms(mod) {
        document.querySelectorAll(`.aa-perm-cb[data-module="${mod}"]`).forEach(cb => {
            cb.checked = true;
            cb.closest('.aa-perm-item').classList.add('assigned');
        });
        updateModuleBadge(mod);
        updateChangeCount();
        updatePermCount();
    }

    function clearModulePerms(mod) {
        document.querySelectorAll(`.aa-perm-cb[data-module="${mod}"]`).forEach(cb => {
            cb.checked = false;
            cb.closest('.aa-perm-item').classList.remove('assigned');
        });
        updateModuleBadge(mod);
        updateChangeCount();
        updatePermCount();
    }

    // ══════════════════════════════════════
    // CHANGE TRACKING
    // ══════════════════════════════════════
    function getSelectedRoleIds() {
        return new Set(Array.from(document.querySelectorAll('.aa-role-cb:checked')).map(c => parseInt(c.value)));
    }

    function getSelectedPermIds() {
        return new Set(Array.from(document.querySelectorAll('.aa-perm-cb:checked')).map(c => parseInt(c.value)));
    }

    function countChanges() {
        const curRoles = getSelectedRoleIds();
        const curPerms = getSelectedPermIds();
        let changes = 0;

        // Roles added
        curRoles.forEach(id => { if (!_originalRoleIds.has(id)) changes++; });
        // Roles removed
        _originalRoleIds.forEach(id => { if (!curRoles.has(id)) changes++; });
        // Perms added
        curPerms.forEach(id => { if (!_originalPermIds.has(id)) changes++; });
        // Perms removed
        _originalPermIds.forEach(id => { if (!curPerms.has(id)) changes++; });

        return changes;
    }

    function updateChangeCount() {
        const count = countChanges();
        const el = document.getElementById('aaChangeCount');
        if (count === 0) {
            el.textContent = 'No changes';
            el.className = 'small text-secondary';
        } else {
            el.textContent = `${count} change${count > 1 ? 's' : ''} pending`;
            el.className = 'small text-warning fw-semibold';
        }
    }

    function resetChanges() {
        // Restore original role selections
        document.querySelectorAll('.aa-role-cb').forEach(cb => {
            const id = parseInt(cb.value);
            cb.checked = _originalRoleIds.has(id);
            const card = cb.closest('.aa-role-card');
            if (_originalRoleIds.has(id)) {
                card.classList.add('assigned');
            } else {
                card.classList.remove('assigned');
            }
        });

        // Restore original permission selections
        document.querySelectorAll('.aa-perm-cb').forEach(cb => {
            const id = parseInt(cb.value);
            cb.checked = _originalPermIds.has(id);
            const item = cb.closest('.aa-perm-item');
            if (_originalPermIds.has(id)) {
                item.classList.add('assigned');
            } else {
                item.classList.remove('assigned');
            }
        });

        // Update badges
        _permissions.forEach(p => {
            const mod = p.moduleName || 'General';
            updateModuleBadge(mod);
        });
        updateRoleCount();
        updatePermCount();
        updateChangeCount();
    }

    // ══════════════════════════════════════
    // SUMMARY BAR
    // ══════════════════════════════════════
    function updateSummary() {
        const badges = [];
        const rolesAssigned = _roles.filter(r => r.isAssigned).length;
        const permsAssigned = _permissions.filter(p => p.isAssigned).length;

        badges.push(`<span class="aa-ai-badge info"><i class="bi bi-shield me-1"></i>${rolesAssigned} Role${rolesAssigned !== 1 ? 's' : ''}</span>`);
        badges.push(`<span class="aa-ai-badge info"><i class="bi bi-key me-1"></i>${permsAssigned} Permission${permsAssigned !== 1 ? 's' : ''}</span>`);

        if (rolesAssigned === 0) {
            badges.push('<span class="aa-ai-badge warn"><i class="bi bi-exclamation-triangle me-1"></i>No roles assigned</span>');
        }
        if (permsAssigned === 0) {
            badges.push('<span class="aa-ai-badge warn"><i class="bi bi-exclamation-triangle me-1"></i>No permissions assigned</span>');
        }
        if (_user?.isSystemAdmin) {
            badges.push('<span class="aa-ai-badge good"><i class="bi bi-shield-fill-check me-1"></i>System Admin</span>');
        }

        document.getElementById('aaAiBadges').innerHTML = badges.join('');

        let hint = 'Select roles and permissions for this user, then click Save.';
        if (rolesAssigned === 0 && permsAssigned === 0) {
            hint = 'No access configured yet — assign roles and permissions below.';
        } else if (rolesAssigned > 0 && permsAssigned > 0) {
            hint = `User has ${rolesAssigned} role${rolesAssigned > 1 ? 's' : ''} and ${permsAssigned} permission${permsAssigned > 1 ? 's' : ''} assigned.`;
        }
        document.getElementById('aaAiHint').textContent = hint;
    }

    // ══════════════════════════════════════
    // SAVE
    // ══════════════════════════════════════
    async function saveAll() {
        const changes = countChanges();
        if (changes === 0) {
            Swal.fire({ icon: 'info', title: 'No Changes', text: 'No changes to save.', timer: 1500 });
            return;
        }

        const confirm = await Swal.fire({
            icon: 'question',
            title: 'Save Changes',
            html: `Apply <strong>${changes}</strong> change${changes > 1 ? 's' : ''} to roles and permissions?`,
            showCancelButton: true,
            confirmButtonText: 'Save',
            confirmButtonColor: '#6366f1'
        });
        if (!confirm.isConfirmed) return;

        const btn = document.getElementById('aaSaveBtn');
        btn.disabled = true;
        btn.innerHTML = '<i class="bi bi-hourglass-split me-1"></i>Saving…';

        try {
            const userId = getUserId();
            const roleIds = Array.from(getSelectedRoleIds());
            const permissionIds = Array.from(getSelectedPermIds());

            await postJson(`${API}/users/${userId}/roles`, { roleIds });
            await postJson(`${API}/users/${userId}/permissions`, { permissionIds });

            Swal.fire({ icon: 'success', title: 'Saved', text: 'Roles and permissions updated successfully.', timer: 1500 });
            document.getElementById('aaLastSaved').textContent = `Saved ${new Date().toLocaleTimeString()}`;

            // Reload to refresh original state
            await Promise.all([loadRoles(), loadPermissions()]);
            updateSummary();
            updateChangeCount();
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Save Failed', text: e.message });
        } finally {
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-check-lg me-1"></i>Save Changes';
        }
    }

    // ── Public API ──
    return {
        init,
        filterRoles,
        filterPerms,
        selectAllRoles,
        clearAllRoles,
        selectAllPerms,
        clearAllPerms,
        selectModulePerms,
        clearModulePerms,
        togglePermGroup,
        onRoleChange,
        onPermChange,
        resetChanges,
        saveAll
    };
})();

document.addEventListener('DOMContentLoaded', () => AaApp.init());
