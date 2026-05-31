/* ═══════════════════════════════════════════════════════════════
   USER DETAIL — Client-Side Module
   MinePress ERP — Tabbed View/Edit with AI Insights
   ═══════════════════════════════════════════════════════════════ */

const UdApp = (() => {
    const API = '/api/usermanagement';
    let _user = null;
    let _editing = false;
    let _lookups = null;
    let _roles = [];
    let _permissions = [];

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
    function esc(s) { const d = document.createElement('div'); d.textContent = s ?? ''; return d.innerHTML; }
    function fmtDate(d) { return d ? new Date(d).toLocaleDateString() : '—'; }
    function fmtDateTime(d) { return d ? new Date(d).toLocaleString() : '—'; }
    function relTime(d) {
        if (!d) return '—';
        const diff = Date.now() - new Date(d).getTime();
        const days = Math.floor(diff / 86400000);
        if (days < 1) return 'Today';
        if (days === 1) return 'Yesterday';
        if (days < 30) return `${days} days ago`;
        if (days < 365) return `${Math.floor(days / 30)} months ago`;
        return `${Math.floor(days / 365)} years ago`;
    }

    function getUserId() {
        return document.getElementById('udUserId')?.value;
    }

    // ── Init ──
    async function init() {
        const id = getUserId();
        if (!id) return;
        await Promise.all([loadUser(), loadLookups(), loadRoles(), loadPermissions()]);
        generateAiRecommendations();
    }

    // ══════════════════════════════════════
    // LOAD USER
    // ══════════════════════════════════════
    async function loadUser() {
        try {
            _user = await fetchJson(`${API}/users/${getUserId()}`);
            renderHero();
            renderProfile();
            renderSecurity();
            renderActivity();
            renderAiInsights();
            renderAiBar();
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        }
    }

    function renderHero() {
        const u = _user;
        document.getElementById('udAvatar').textContent = u.name?.charAt(0)?.toUpperCase() || '?';
        document.getElementById('udHeroName').textContent = u.name || '—';
        document.getElementById('udHeroCode').innerHTML = `<code>${esc(u.userCode)}</code>`;
        document.getElementById('udHeroUsername').textContent = u.username || '';

        let badge = '';
        if (u.isLocked) badge = '<span class="badge bg-warning-lt text-warning"><i class="bi bi-lock me-1"></i>Locked</span>';
        else if (u.isActive) badge = '<span class="badge bg-success-lt text-success"><i class="bi bi-check-circle me-1"></i>Active</span>';
        else badge = '<span class="badge bg-danger-lt text-danger"><i class="bi bi-x-circle me-1"></i>Inactive</span>';
        document.getElementById('udHeroStatus').innerHTML = badge;

        // Toggle button labels
        const toggleBtn = document.getElementById('udToggleActiveBtn');
        const toggleLabel = document.getElementById('udToggleActiveLabel');
        if (u.isActive) {
            toggleBtn.className = 'btn btn-sm btn-outline-warning';
            toggleLabel.textContent = 'Deactivate';
        } else {
            toggleBtn.className = 'btn btn-sm btn-outline-success';
            toggleLabel.textContent = 'Activate';
        }
        document.getElementById('udUnlockBtn').style.display = u.isLocked ? '' : 'none';
    }

    function renderProfile() {
        const u = _user;
        document.getElementById('vName').textContent = u.name || '—';
        document.getElementById('vUsername').textContent = u.username || '—';
        document.getElementById('vUserCode').textContent = u.userCode || '—';
        document.getElementById('vEmployeeCode').textContent = u.employeeCode || '—';
        document.getElementById('vEmail').textContent = u.email || '—';
        document.getElementById('vMobile').textContent = u.mobile || '—';

        document.getElementById('vUserType').innerHTML = u.userType ? `<span class="badge bg-primary-lt">${esc(u.userType)}</span>` : '—';
        document.getElementById('vDepartment').textContent = u.department || '—';
        document.getElementById('vDesignation').textContent = u.designation || '—';
        document.getElementById('vLocation').textContent = u.location || '—';
        document.getElementById('vReportingTo').textContent = u.reportingUser ? u.reportingUser.name : '—';
        document.getElementById('vUserCategory').textContent = u.userCategory || '—';

        document.getElementById('vJoiningDate').textContent = fmtDate(u.joiningDate);
        document.getElementById('vExitDate').textContent = fmtDate(u.exitDate);
        document.getElementById('vLastPasswordChange').textContent = fmtDate(u.lastPasswordChange);
    }

    function renderSecurity() {
        const u = _user;

        // Status cards
        const activeCard = document.getElementById('udStatusActive');
        const lockedCard = document.getElementById('udStatusLocked');

        activeCard.className = `ud-status-card ${u.isActive ? 'status-good' : 'status-bad'}`;
        document.getElementById('vIsActive').innerHTML = u.isActive
            ? '<span class="text-green"><i class="bi bi-check-circle me-1"></i>Yes</span>'
            : '<span class="text-red"><i class="bi bi-x-circle me-1"></i>No</span>';

        lockedCard.className = `ud-status-card ${u.isLocked ? 'status-warn' : 'status-good'}`;
        document.getElementById('vIsLocked').innerHTML = u.isLocked
            ? '<span class="text-yellow"><i class="bi bi-lock me-1"></i>Yes</span>'
            : '<span class="text-green"><i class="bi bi-unlock me-1"></i>No</span>';

        document.getElementById('vFailedLogins').textContent = u.failedLoginCount || 0;

        // Flags
        document.getElementById('fSystemAdmin').checked = u.isSystemAdmin;
        document.getElementById('fApprovalUser').checked = u.isApprovalUser;
        document.getElementById('fProductionUser').checked = u.isProductionUser;
        document.getElementById('fWebAccess').checked = u.isWebAccess;
        document.getElementById('fMobileAccess').checked = u.isMobileAccess;
    }

    function renderActivity() {
        const u = _user;

        document.getElementById('vLastLogin').textContent = relTime(u.lastLogin);
        document.getElementById('vTotalLogins').textContent = u.recentLogins?.length || 0;
        document.getElementById('vFailedCount').textContent = u.failedLoginCount || 0;
        document.getElementById('vAccountAge').textContent = relTime(u.joiningDate);

        const tbody = document.getElementById('udLoginTableBody');
        if (!u.recentLogins || u.recentLogins.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center text-secondary py-4"><i class="bi bi-inbox me-1"></i>No login records found</td></tr>';
            return;
        }

        tbody.innerHTML = u.recentLogins.map(l => {
            const loginAt = l.loginAt ? new Date(l.loginAt) : null;
            const logoutAt = l.logoutAt ? new Date(l.logoutAt) : null;
            let duration = '—';
            if (loginAt && logoutAt) {
                const mins = Math.round((logoutAt - loginAt) / 60000);
                duration = mins < 60 ? `${mins}m` : `${Math.floor(mins / 60)}h ${mins % 60}m`;
            }
            const channelClass = (l.channel || '').toLowerCase() === 'web' ? 'ud-login-web'
                : (l.channel || '').toLowerCase() === 'mobile' ? 'ud-login-mobile' : 'ud-login-api';
            return `
                <tr class="ud-fade-in">
                    <td>${fmtDateTime(l.loginAt)}</td>
                    <td>${fmtDateTime(l.logoutAt)}</td>
                    <td><code class="small">${esc(l.ip || '—')}</code></td>
                    <td><span class="ud-login-badge ${channelClass}">${esc(l.channel || '—')}</span></td>
                    <td>${duration}</td>
                </tr>`;
        }).join('');
    }

    function renderAiInsights() {
        const u = _user;
        const score = u.aiHealthScore;

        // Score
        const scoreEl = document.getElementById('vAiScore');
        if (score != null) {
            scoreEl.textContent = score + '%';
            scoreEl.className = `ud-ai-card-val ${score >= 70 ? 'text-green' : score >= 40 ? 'text-yellow' : 'text-red'}`;
        } else {
            scoreEl.textContent = '—';
        }

        // Progress bar
        const bar = document.getElementById('vAiScoreBar');
        bar.style.width = (score || 0) + '%';
        bar.className = `ud-ai-progress-bar ${score >= 70 ? 'score-good' : score >= 40 ? 'score-warn' : 'score-bad'}`;

        document.getElementById('vAiAlerts').textContent = u.aiAlertCount || 0;
        document.getElementById('vAiAutoConfig').innerHTML = u.aiAutoConfigured
            ? '<i class="bi bi-check-circle text-green me-1"></i>Yes'
            : '<i class="bi bi-x-circle text-secondary me-1"></i>No';
        document.getElementById('vAiLastReviewed').textContent = fmtDateTime(u.aiLastReviewed);
    }

    function renderAiBar() {
        const u = _user;
        const badges = [];
        const hints = [];

        if (u.aiHealthScore != null) {
            const cls = u.aiHealthScore >= 70 ? 'good' : u.aiHealthScore >= 40 ? 'warn' : 'bad';
            badges.push(`<span class="ud-ai-badge ${cls}"><i class="bi bi-heart-pulse me-1"></i>${u.aiHealthScore}% Health</span>`);
        }

        if (!u.lastLogin) {
            badges.push('<span class="ud-ai-badge warn"><i class="bi bi-exclamation-triangle me-1"></i>Never logged in</span>');
            hints.push('User has never logged in');
        } else {
            const daysSince = Math.floor((Date.now() - new Date(u.lastLogin).getTime()) / 86400000);
            if (daysSince > 90) {
                badges.push(`<span class="ud-ai-badge warn"><i class="bi bi-clock me-1"></i>Dormant (${daysSince}d)</span>`);
                hints.push(`Last login ${daysSince} days ago — consider reviewing`);
            }
        }

        if (u.isSystemAdmin) {
            badges.push('<span class="ud-ai-badge info"><i class="bi bi-shield me-1"></i>Admin</span>');
        }
        if (u.isLocked) {
            badges.push('<span class="ud-ai-badge bad"><i class="bi bi-lock me-1"></i>Locked</span>');
            hints.push('Account is locked — unlock if appropriate');
        }
        if (!u.isActive) {
            badges.push('<span class="ud-ai-badge bad"><i class="bi bi-x-circle me-1"></i>Inactive</span>');
        }
        if (u.failedLoginCount > 3) {
            badges.push(`<span class="ud-ai-badge warn"><i class="bi bi-exclamation me-1"></i>${u.failedLoginCount} failed</span>`);
        }

        if (hints.length === 0) hints.push('User profile looks healthy — no issues detected');

        document.getElementById('udAiBadges').innerHTML = badges.join('');
        document.getElementById('udAiHint').textContent = hints[0];
    }

    function generateAiRecommendations() {
        if (!_user) return;
        const u = _user;
        const recs = [];

        if (!u.email) {
            recs.push({ type: 'warning', icon: 'bi-envelope-exclamation', title: 'Missing Email', desc: 'No email address configured. Email is required for password resets and notifications.' });
        }
        if (!u.mobile) {
            recs.push({ type: 'suggest', icon: 'bi-phone', title: 'Add Mobile Number', desc: 'Adding a mobile number enables SMS notifications and 2FA.' });
        }
        if (!u.lastLogin) {
            recs.push({ type: 'warning', icon: 'bi-person-x', title: 'Never Logged In', desc: 'This user has never logged in. Consider sending a reminder or verifying the account.' });
        } else {
            const daysSince = Math.floor((Date.now() - new Date(u.lastLogin).getTime()) / 86400000);
            if (daysSince > 90) {
                recs.push({ type: 'warning', icon: 'bi-clock-history', title: 'Dormant Account', desc: `Last login was ${daysSince} days ago. Consider deactivating if no longer needed.` });
            }
            if (daysSince > 365) {
                recs.push({ type: 'danger', icon: 'bi-exclamation-octagon', title: 'Stale Account Risk', desc: 'Account inactive for over a year. Strongly recommend deactivation for security.' });
            }
        }

        if (u.isSystemAdmin && !u.isWebAccess) {
            recs.push({ type: 'danger', icon: 'bi-shield-exclamation', title: 'Admin Without Web Access', desc: 'System admin has no web access — this may be a misconfiguration.' });
        }
        if (u.failedLoginCount > 5) {
            recs.push({ type: 'danger', icon: 'bi-shield-x', title: 'High Failed Logins', desc: `${u.failedLoginCount} failed login attempts detected. Possible brute-force attempt.` });
        }
        if (u.isWebAccess && u.isMobileAccess) {
            recs.push({ type: 'success', icon: 'bi-check-circle', title: 'Full Access Enabled', desc: 'User has both web and mobile access configured — good multi-platform coverage.' });
        }
        if (!u.lastPasswordChange) {
            recs.push({ type: 'warning', icon: 'bi-key', title: 'Password Never Changed', desc: 'User may still be using the default password. Recommend enforcing a change.' });
        }
        if (u.aiHealthScore != null && u.aiHealthScore >= 80) {
            recs.push({ type: 'success', icon: 'bi-stars', title: 'Healthy Profile', desc: `AI health score is ${u.aiHealthScore}% — user profile is well-configured.` });
        }

        if (recs.length === 0) {
            recs.push({ type: 'success', icon: 'bi-check2-all', title: 'All Good', desc: 'No issues found. User profile meets all recommended standards.' });
        }

        const container = document.getElementById('udAiRecommendations');
        container.innerHTML = recs.map((r, i) => `
            <div class="ud-ai-rec ud-fade-in" style="animation-delay:${i * .08}s">
                <div class="ud-ai-rec-icon ${r.type}"><i class="bi ${r.icon}"></i></div>
                <div>
                    <div class="ud-ai-rec-title">${esc(r.title)}</div>
                    <div class="ud-ai-rec-desc">${esc(r.desc)}</div>
                </div>
            </div>
        `).join('');
    }

    // ══════════════════════════════════════
    // LOOKUPS & ROLES
    // ══════════════════════════════════════
    async function loadLookups() {
        try {
            _lookups = await fetchJson(`${API}/lookups`);
        } catch (e) {
            console.error('Lookups error', e);
        }
    }

    async function loadRoles() {
        try {
            const data = await fetchJson(`${API}/users/${getUserId()}/roles`);
            _roles = data.roles || [];
            renderRoles();
        } catch (e) {
            console.error('Roles error', e);
        }
    }

    function renderRoles() {
        const grid = document.getElementById('udRolesGrid');
        if (_roles.length === 0) {
            grid.innerHTML = '<div class="text-center text-secondary py-4">No roles available</div>';
            return;
        }

        grid.innerHTML = _roles.map(r => `
            <div class="ud-role-card ud-fade-in">
                <div class="ud-role-icon"><i class="bi bi-shield"></i></div>
                <div>
                    <div class="ud-role-name">${esc(r.roleName)}</div>
                    <div class="ud-role-code">${esc(r.roleCode)}</div>
                </div>
                <div class="ud-role-check">
                    <div class="form-check">
                        <input class="form-check-input ud-role-cb" type="checkbox" value="${r.roleId}" id="udRole_${r.roleId}" ${r.isAssigned ? 'checked' : ''} disabled />
                    </div>
                </div>
            </div>
        `).join('');
    }

    async function loadPermissions() {
        try {
            const data = await fetchJson(`${API}/users/${getUserId()}/permissions`);
            _permissions = data.permissions || [];
            renderPermissions();
        } catch (e) {
            console.error('Permissions error', e);
        }
    }

    function renderPermissions() {
        const grid = document.getElementById('udPermissionsGrid');
        if (!grid) return;
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

        grid.innerHTML = Object.entries(grouped).map(([mod, perms]) => `
            <div class="ud-perm-group">
                <div class="ud-perm-group-header">
                    <i class="bi bi-folder me-1"></i>${esc(mod)}
                    <span class="badge bg-secondary-lt ms-2">${perms.filter(p => p.isAssigned).length}/${perms.length}</span>
                </div>
                <div class="ud-perm-group-body">
                    ${perms.map(p => `
                        <div class="ud-perm-item">
                            <div class="form-check">
                                <input class="form-check-input ud-perm-cb" type="checkbox" value="${p.permissionId}" id="udPerm_${p.permissionId}" ${p.isAssigned ? 'checked' : ''} disabled />
                                <label class="form-check-label" for="udPerm_${p.permissionId}">
                                    <span class="ud-perm-name">${esc(p.permissionName)}</span>
                                    <span class="ud-perm-code text-secondary">${esc(p.permissionCode)}</span>
                                </label>
                            </div>
                        </div>
                    `).join('')}
                </div>
            </div>
        `).join('');
    }

    function populateEditFields() {
        if (!_user || !_lookups) return;
        const u = _user;

        // Fill edit inputs
        document.getElementById('eName').value = u.name || '';
        document.getElementById('eUsername').value = u.username || '';
        document.getElementById('eUserCode').value = u.userCode || '';
        document.getElementById('eEmployeeCode').value = u.employeeCode || '';
        document.getElementById('eEmail').value = u.email || '';
        document.getElementById('eMobile').value = u.mobile || '';
        document.getElementById('eUserType').value = u.userType || '';

        // Populate dropdowns
        const deptSel = document.getElementById('eDepartment');
        deptSel.innerHTML = '<option value="">Select…</option>' + _lookups.departments.map(x =>
            `<option value="${x.id}" ${x.id === u.departmentId ? 'selected' : ''}>${esc(x.name)}</option>`
        ).join('');

        const desigSel = document.getElementById('eDesignation');
        desigSel.innerHTML = '<option value="">Select…</option>' + _lookups.designations.map(x =>
            `<option value="${x.id}" ${x.id === u.designationId ? 'selected' : ''}>${esc(x.name)}</option>`
        ).join('');

        const locSel = document.getElementById('eLocation');
        locSel.innerHTML = '<option value="">Select…</option>' + _lookups.locations.map(x =>
            `<option value="${x.id}" ${x.id === u.locationId ? 'selected' : ''}>${esc(x.name)}</option>`
        ).join('');
    }

    // ══════════════════════════════════════
    // EDIT MODE
    // ══════════════════════════════════════
    function toggleEditMode() {
        if (_editing) {
            cancelEdit();
        } else {
            enterEditMode();
        }
    }

    function enterEditMode() {
        _editing = true;
        populateEditFields();

        // Toggle field visibility
        document.querySelectorAll('.ud-view-val').forEach(el => el.classList.add('d-none'));
        document.querySelectorAll('.ud-edit-field').forEach(el => el.classList.remove('d-none'));

        // Enable flag switches
        document.querySelectorAll('.ud-flag-switch').forEach(el => el.disabled = false);

        // Enable role checkboxes
        document.querySelectorAll('.ud-role-cb').forEach(el => el.disabled = false);

        // Enable permission checkboxes
        document.querySelectorAll('.ud-perm-cb').forEach(el => el.disabled = false);

        // Show save/cancel buttons
        document.getElementById('udSaveBtn').classList.remove('d-none');
        document.getElementById('udCancelBtn').classList.remove('d-none');

        // Update toggle button
        document.getElementById('udEditLabel').textContent = 'Editing…';
        document.getElementById('udEditToggle').className = 'btn btn-light btn-sm';

        // Add editing class for CSS overrides
        document.querySelector('.tab-content')?.classList.add('ud-editing');
    }

    function cancelEdit() {
        _editing = false;

        // Toggle field visibility
        document.querySelectorAll('.ud-view-val').forEach(el => el.classList.remove('d-none'));
        document.querySelectorAll('.ud-edit-field').forEach(el => el.classList.add('d-none'));

        // Disable flag switches
        document.querySelectorAll('.ud-flag-switch').forEach(el => el.disabled = true);

        // Disable role checkboxes
        document.querySelectorAll('.ud-role-cb').forEach(el => el.disabled = true);

        // Disable permission checkboxes
        document.querySelectorAll('.ud-perm-cb').forEach(el => el.disabled = true);

        // Hide save/cancel buttons
        document.getElementById('udSaveBtn').classList.add('d-none');
        document.getElementById('udCancelBtn').classList.add('d-none');

        // Restore toggle button
        document.getElementById('udEditLabel').textContent = 'Edit';
        document.getElementById('udEditToggle').className = 'btn btn-outline-secondary btn-sm';

        document.querySelector('.tab-content')?.classList.remove('ud-editing');

        // Re-render with original data
        renderProfile();
        renderSecurity();
    }

    // ══════════════════════════════════════
    // SAVE
    // ══════════════════════════════════════
    async function saveUser() {
        const payload = {
            userCode: document.getElementById('eUserCode').value,
            username: document.getElementById('eUsername').value,
            name: document.getElementById('eName').value,
            email: document.getElementById('eEmail').value || null,
            mobile: document.getElementById('eMobile').value || null,
            userType: document.getElementById('eUserType').value,
            departmentId: parseInt(document.getElementById('eDepartment').value),
            designationId: parseInt(document.getElementById('eDesignation').value),
            locationId: parseInt(document.getElementById('eLocation').value),
            employeeCode: document.getElementById('eEmployeeCode').value || null,
            isSystemAdmin: document.getElementById('fSystemAdmin').checked,
            isApprovalUser: document.getElementById('fApprovalUser').checked,
            isProductionUser: document.getElementById('fProductionUser').checked,
            isWebAccess: document.getElementById('fWebAccess').checked,
            isMobileAccess: document.getElementById('fMobileAccess').checked,
            roleIds: Array.from(document.querySelectorAll('.ud-role-cb:checked')).map(c => parseInt(c.value))
        };

        if (!payload.userCode || !payload.username || !payload.name) {
            Swal.fire({ icon: 'warning', title: 'Validation', text: 'User code, username, and name are required.' });
            return;
        }
        if (!payload.departmentId || !payload.designationId || !payload.locationId) {
            Swal.fire({ icon: 'warning', title: 'Validation', text: 'Department, designation, and location are required.' });
            return;
        }

        try {
            const userId = getUserId();
            await putJson(`${API}/users/${userId}`, payload);

            // Save roles separately
            const roleIds = Array.from(document.querySelectorAll('.ud-role-cb:checked')).map(c => parseInt(c.value));
            await postJson(`${API}/users/${userId}/roles`, { roleIds });

            // Save permissions separately
            const permissionIds = Array.from(document.querySelectorAll('.ud-perm-cb:checked')).map(c => parseInt(c.value));
            await postJson(`${API}/users/${userId}/permissions`, { permissionIds });

            Swal.fire({ icon: 'success', title: 'Saved', text: 'User, roles, and permissions updated successfully', timer: 1500 });
            document.getElementById('udLastSaved').textContent = `Saved ${new Date().toLocaleTimeString()}`;
            cancelEdit();
            await Promise.all([loadUser(), loadRoles(), loadPermissions()]);
            generateAiRecommendations();
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Save Failed', text: e.message });
        }
    }

    // ══════════════════════════════════════
    // ACTIONS
    // ══════════════════════════════════════
    async function resetPassword() {
        const confirm = await Swal.fire({
            icon: 'warning',
            title: 'Reset Password',
            text: 'This will reset the password to default (Welcome@123). Continue?',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            confirmButtonText: 'Reset'
        });
        if (!confirm.isConfirmed) return;

        try {
            const res = await postJson(`${API}/users/${getUserId()}/reset-password`, {});
            Swal.fire({ icon: 'success', title: 'Done', text: res.message, timer: 2000 });
            await loadUser();
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        }
    }

    async function toggleActive() {
        const action = _user?.isActive ? 'deactivate' : 'activate';
        const confirm = await Swal.fire({
            icon: 'question',
            title: 'Confirm',
            text: `Are you sure you want to ${action} this user?`,
            showCancelButton: true
        });
        if (!confirm.isConfirmed) return;

        try {
            const res = await postJson(`${API}/users/${getUserId()}/toggle-status`, { field: 'active' });
            Swal.fire({ icon: 'success', title: 'Done', text: res.message, timer: 1500 });
            await loadUser();
            generateAiRecommendations();
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        }
    }

    async function unlockUser() {
        const confirm = await Swal.fire({
            icon: 'question',
            title: 'Unlock User',
            text: 'This will unlock the user and reset failed login count. Continue?',
            showCancelButton: true
        });
        if (!confirm.isConfirmed) return;

        try {
            const res = await postJson(`${API}/users/${getUserId()}/toggle-status`, { field: 'locked' });
            Swal.fire({ icon: 'success', title: 'Done', text: res.message, timer: 1500 });
            await loadUser();
            generateAiRecommendations();
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        }
    }

    async function runAiAnalysis() {
        const btn = event?.target?.closest('button');
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<i class="bi bi-hourglass-split me-1"></i>Analyzing…';
        }

        // Simulate AI analysis delay
        await new Promise(r => setTimeout(r, 1200));
        generateAiRecommendations();
        renderAiBar();

        if (btn) {
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-arrow-clockwise me-1"></i>Re-run AI Analysis';
        }

        Swal.fire({ icon: 'success', title: 'AI Analysis Complete', text: 'Recommendations have been refreshed.', timer: 1500 });
    }

    // ── Public API ──
    return {
        init,
        toggleEditMode,
        cancelEdit,
        saveUser,
        resetPassword,
        toggleActive,
        unlockUser,
        runAiAnalysis
    };
})();

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', () => UdApp.init());
