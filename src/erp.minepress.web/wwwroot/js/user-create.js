/* ═══════════════════════════════════════════════════════════════
   USER CREATE — Wizard Client Module
   MinePress ERP — Modern AI-Powered User Creation
   ═══════════════════════════════════════════════════════════════ */

const UcApp = (() => {
    const API = '/api/usermanagement';
    let _step = 1;
    const TOTAL_STEPS = 4;
    let _roles = [];
    let _lookups = { departments: [], designations: [], locations: [] };

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
    function $(id) { return document.getElementById(id); }
    function val(id) { return $(id)?.value?.trim() ?? ''; }

    // ══════════════════════════════════════
    // INIT
    // ══════════════════════════════════════
    async function init() {
        loadLookups();
        loadRoles();

        // Password strength live update
        $('ucPassword')?.addEventListener('input', updatePwdStrength);

        // AI hint on department change
        $('ucDepartment')?.addEventListener('change', updateAiHints);
        $('ucUserType')?.addEventListener('change', updateAiHints);

        updateNav();
    }

    // ══════════════════════════════════════
    // LOOKUPS
    // ══════════════════════════════════════
    async function loadLookups() {
        try {
            _lookups = await fetchJson(`${API}/lookups`);
            const dSel = $('ucDepartment');
            const dgSel = $('ucDesignation');
            const lSel = $('ucLocation');

            dSel.innerHTML = '<option value="">Select department…</option>' +
                _lookups.departments.map(x => `<option value="${x.id}">${esc(x.name)}</option>`).join('');
            dgSel.innerHTML = '<option value="">Select designation…</option>' +
                _lookups.designations.map(x => `<option value="${x.id}">${esc(x.name)}</option>`).join('');
            lSel.innerHTML = '<option value="">Select location…</option>' +
                _lookups.locations.map(x => `<option value="${x.id}">${esc(x.name)}</option>`).join('');
        } catch (e) {
            console.error('Lookups error', e);
        }
    }

    // ══════════════════════════════════════
    // ROLES
    // ══════════════════════════════════════
    async function loadRoles() {
        try {
            _roles = await fetchJson(`${API}/roles`);
            renderRoles();
        } catch (e) {
            console.error('Roles error', e);
        }
    }

    function renderRoles() {
        const grid = $('ucRoleGrid');
        if (!grid) return;
        const active = _roles.filter(r => r.isActive);
        if (active.length === 0) {
            grid.innerHTML = '<div class="text-center text-secondary py-3">No roles available</div>';
            return;
        }
        grid.innerHTML = active.map(r => `
            <label class="uc-role-card" data-role-id="${r.roleId}" data-role-code="${esc(r.roleCode)}">
                <div class="role-icon"><i class="bi bi-shield-check"></i></div>
                <div class="flex-fill">
                    <div class="role-name">${esc(r.roleName)}</div>
                    <div class="role-code">${esc(r.roleCode)}</div>
                </div>
                <div class="form-check">
                    <input class="form-check-input" type="checkbox" value="${r.roleId}" />
                </div>
            </label>
        `).join('');
    }

    // ══════════════════════════════════════
    // STEP NAVIGATION
    // ══════════════════════════════════════
    function goStep(n) {
        if (n < 1 || n > TOTAL_STEPS) return;
        // Validate before forward navigation
        if (n > _step) {
            for (let s = _step; s < n; s++) {
                if (!validateStep(s)) return;
            }
        }
        _step = n;
        renderStep();
    }

    function nextStep() {
        if (_step >= TOTAL_STEPS) return;
        if (!validateStep(_step)) return;
        _step++;
        if (_step === 3) highlightAiRoles();
        if (_step === 4) { buildReview(); runAiChecks(); }
        renderStep();
    }

    function prevStep() {
        if (_step <= 1) return;
        _step--;
        renderStep();
    }

    function renderStep() {
        // Panels
        document.querySelectorAll('.uc-step-panel').forEach(p => p.classList.remove('active'));
        const panel = $(`ucStep${_step}`);
        if (panel) panel.classList.add('active');

        // Step indicators
        document.querySelectorAll('.uc-step').forEach(s => {
            const sn = parseInt(s.dataset.step);
            s.classList.remove('active', 'completed');
            if (sn === _step) s.classList.add('active');
            else if (sn < _step) s.classList.add('completed');
        });

        // Step lines
        const lines = document.querySelectorAll('.uc-step-line');
        lines.forEach((line, i) => {
            if (i + 1 < _step) line.classList.add('completed');
            else line.classList.remove('completed');
        });

        updateNav();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    function updateNav() {
        const prev = $('ucBtnPrev');
        const next = $('ucBtnNext');
        const create = $('ucBtnCreate');

        if (prev) prev.style.display = _step > 1 ? '' : 'none';
        if (next) next.style.display = _step < TOTAL_STEPS ? '' : 'none';
        if (create) create.style.display = _step === TOTAL_STEPS ? '' : 'none';
    }

    // ══════════════════════════════════════
    // VALIDATION
    // ══════════════════════════════════════
    function validateStep(step) {
        if (step === 1) {
            const required = [
                { id: 'ucUserCode', label: 'User Code' },
                { id: 'ucUsername', label: 'Username' },
                { id: 'ucName', label: 'Full Name' },
                { id: 'ucUserType', label: 'User Type' },
                { id: 'ucDepartment', label: 'Department' },
                { id: 'ucDesignation', label: 'Designation' },
                { id: 'ucLocation', label: 'Location' }
            ];
            for (const f of required) {
                if (!val(f.id)) {
                    Swal.fire({ icon: 'warning', title: 'Required', text: `${f.label} is required.` });
                    $(f.id)?.focus();
                    return false;
                }
            }
            // Validate email format if provided
            const email = val('ucEmail');
            if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
                Swal.fire({ icon: 'warning', title: 'Invalid Email', text: 'Please enter a valid email address.' });
                $('ucEmail')?.focus();
                return false;
            }
        }
        if (step === 2) {
            const pwd = val('ucPassword');
            if (!pwd) {
                Swal.fire({ icon: 'warning', title: 'Required', text: 'Password is required.' });
                $('ucPassword')?.focus();
                return false;
            }
            if (pwd.length < 8) {
                Swal.fire({ icon: 'warning', title: 'Weak Password', text: 'Password must be at least 8 characters.' });
                $('ucPassword')?.focus();
                return false;
            }
        }
        return true;
    }

    // ══════════════════════════════════════
    // PASSWORD
    // ══════════════════════════════════════
    function togglePwdVisibility() {
        const inp = $('ucPassword');
        const icon = $('ucPwdEyeIcon');
        if (!inp) return;
        const show = inp.type === 'password';
        inp.type = show ? 'text' : 'password';
        if (icon) {
            icon.className = show ? 'bi bi-eye-slash' : 'bi bi-eye';
        }
    }

    function generatePassword() {
        const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%&*';
        let pwd = '';
        for (let i = 0; i < 14; i++) pwd += chars.charAt(Math.floor(Math.random() * chars.length));
        // Ensure at least one uppercase, lowercase, digit, special
        pwd = pwd.substring(0, 10) +
            'ABCDEFGHJKLMNPQRSTUVWXYZ'[Math.floor(Math.random() * 24)] +
            'abcdefghjkmnpqrstuvwxyz'[Math.floor(Math.random() * 23)] +
            '23456789'[Math.floor(Math.random() * 8)] +
            '!@#$%&*'[Math.floor(Math.random() * 7)];
        const inp = $('ucPassword');
        if (inp) {
            inp.type = 'text';
            inp.value = pwd;
            const icon = $('ucPwdEyeIcon');
            if (icon) icon.className = 'bi bi-eye-slash';
        }
        updatePwdStrength();
        Swal.fire({ icon: 'success', title: 'Generated!', text: 'Strong password generated. Make sure to copy it.', timer: 2000, showConfirmButton: false });
    }

    function updatePwdStrength() {
        const pwd = val('ucPassword');
        const fill = $('ucPwdFill');
        const label = $('ucPwdLabel');
        if (!fill || !label) return;

        let score = 0;
        if (pwd.length >= 8) score++;
        if (pwd.length >= 12) score++;
        if (/[A-Z]/.test(pwd)) score++;
        if (/[a-z]/.test(pwd)) score++;
        if (/[0-9]/.test(pwd)) score++;
        if (/[^A-Za-z0-9]/.test(pwd)) score++;

        fill.className = 'uc-pwd-fill';
        if (pwd.length === 0) {
            fill.style.width = '0%';
            label.textContent = 'Enter a password';
        } else if (score <= 2) {
            fill.style.width = '25%';
            fill.classList.add('weak');
            label.textContent = 'Weak';
        } else if (score <= 3) {
            fill.style.width = '50%';
            fill.classList.add('fair');
            label.textContent = 'Fair';
        } else if (score <= 4) {
            fill.style.width = '75%';
            fill.classList.add('good');
            label.textContent = 'Good';
        } else {
            fill.style.width = '100%';
            fill.classList.add('strong');
            label.textContent = 'Strong';
        }
    }

    // ══════════════════════════════════════
    // AI HINTS & ROLE SUGGESTIONS
    // ══════════════════════════════════════
    function updateAiHints() {
        const dept = $('ucDepartment')?.selectedOptions[0]?.text || '';
        const type = val('ucUserType');
        const hint = $('ucAiHint');
        const badges = $('ucAiBadges');
        if (!hint) return;

        const tips = [];
        if (type === 'ADMIN') tips.push('<span class="badge bg-purple-lt text-purple">Admin — consider limiting to essential personnel</span>');
        if (type === 'EMPLOYEE' && dept) tips.push(`<span class="badge bg-azure-lt text-azure">${esc(dept)} dept detected</span>`);
        if (type === 'VENDOR' || type === 'CUSTOMER') tips.push('<span class="badge bg-yellow-lt text-yellow">External user — restrict access</span>');

        if (tips.length > 0 && badges) badges.innerHTML = tips.join('');
        if (dept && type) {
            hint.textContent = `AI ready — will suggest roles for ${type.toLowerCase()} in ${dept} department.`;
        }
    }

    function highlightAiRoles() {
        const type = val('ucUserType');
        const deptName = $('ucDepartment')?.selectedOptions[0]?.text?.toLowerCase() || '';
        const suggestion = $('ucAiRoleSuggestion');
        const suggText = $('ucAiRoleText');

        // Simple AI heuristic: suggest roles based on user type and department
        const suggestions = [];
        _roles.filter(r => r.isActive).forEach(r => {
            const code = r.roleCode.toUpperCase();
            const name = r.roleName.toLowerCase();
            if (type === 'ADMIN' && (code.includes('ADMIN') || name.includes('admin'))) suggestions.push(r.roleId);
            if (type === 'EMPLOYEE' && (code.includes('USER') || name.includes('user') || name.includes('employee'))) suggestions.push(r.roleId);
            if (deptName.includes('produc') && (code.includes('PROD') || name.includes('produc'))) suggestions.push(r.roleId);
            if (deptName.includes('account') && (code.includes('ACC') || name.includes('account') || name.includes('finance'))) suggestions.push(r.roleId);
            if (deptName.includes('hr') && (code.includes('HR') || name.includes('hr') || name.includes('human'))) suggestions.push(r.roleId);
        });

        // Highlight suggested role cards
        document.querySelectorAll('.uc-role-card').forEach(card => {
            const rid = parseInt(card.dataset.roleId);
            if (suggestions.includes(rid)) {
                card.classList.add('ai-suggested');
            } else {
                card.classList.remove('ai-suggested');
            }
        });

        if (suggestions.length > 0 && suggestion && suggText) {
            suggText.textContent = `Based on ${val('ucUserType')} type and department, we suggest ${suggestions.length} role(s). AI-suggested roles are highlighted with ★.`;
            suggestion.classList.remove('d-none');
        }
    }

    // ══════════════════════════════════════
    // REVIEW (Step 4)
    // ══════════════════════════════════════
    function buildReview() {
        const grid = $('ucReviewGrid');
        if (!grid) return;

        const deptText = $('ucDepartment')?.selectedOptions[0]?.text || '—';
        const desigText = $('ucDesignation')?.selectedOptions[0]?.text || '—';
        const locText = $('ucLocation')?.selectedOptions[0]?.text || '—';
        const selectedRoles = Array.from(document.querySelectorAll('#ucRoleGrid input:checked')).map(c => {
            const card = c.closest('.uc-role-card');
            return card?.querySelector('.role-name')?.textContent || '';
        });

        const flags = [];
        if ($('ucIsSystemAdmin')?.checked) flags.push('System Admin');
        if ($('ucIsApprovalUser')?.checked) flags.push('Approval');
        if ($('ucIsProductionUser')?.checked) flags.push('Production');
        if ($('ucIsWebAccess')?.checked) flags.push('Web Access');
        if ($('ucIsMobileAccess')?.checked) flags.push('Mobile Access');

        const items = [
            { label: 'User Code', value: val('ucUserCode') },
            { label: 'Username', value: val('ucUsername') },
            { label: 'Full Name', value: val('ucName') },
            { label: 'Email', value: val('ucEmail') || '—' },
            { label: 'Mobile', value: val('ucMobile') || '—' },
            { label: 'Employee Code', value: val('ucEmpCode') || '—' },
            { label: 'User Type', value: val('ucUserType') },
            { label: 'Department', value: deptText },
            { label: 'Designation', value: desigText },
            { label: 'Location', value: locText },
            { label: 'Roles', value: selectedRoles.length > 0 ? selectedRoles.join(', ') : '—' },
            { label: 'Access Flags', value: flags.length > 0 ? flags.join(', ') : 'None' }
        ];

        grid.innerHTML = items.map(i => `
            <div class="uc-review-item">
                <div class="review-label">${esc(i.label)}</div>
                <div class="review-value ${i.value === '—' ? 'empty' : ''}">${esc(i.value)}</div>
            </div>
        `).join('');
    }

    function runAiChecks() {
        const checks = $('ucAiChecks');
        if (!checks) return;

        const results = [];
        const email = val('ucEmail');
        const pwd = val('ucPassword');
        const type = val('ucUserType');
        const isAdmin = $('ucIsSystemAdmin')?.checked;
        const rolesSelected = document.querySelectorAll('#ucRoleGrid input:checked').length;

        // Check: email provided
        if (email) {
            results.push({ cls: 'pass', icon: 'bi-check-circle-fill', text: 'Email provided — welcome credentials will be sent' });
        } else {
            results.push({ cls: 'warn', icon: 'bi-exclamation-triangle-fill', text: 'No email — user won\'t receive login credentials via email' });
        }

        // Check: password strength
        let pwdScore = 0;
        if (pwd.length >= 8) pwdScore++;
        if (pwd.length >= 12) pwdScore++;
        if (/[A-Z]/.test(pwd) && /[a-z]/.test(pwd)) pwdScore++;
        if (/[0-9]/.test(pwd) && /[^A-Za-z0-9]/.test(pwd)) pwdScore++;
        if (pwdScore >= 3) results.push({ cls: 'pass', icon: 'bi-shield-fill-check', text: 'Strong password configured' });
        else results.push({ cls: 'warn', icon: 'bi-shield-fill-exclamation', text: 'Consider using a stronger password' });

        // Check: admin flag
        if (isAdmin) {
            results.push({ cls: 'warn', icon: 'bi-exclamation-diamond-fill', text: 'System Admin flag enabled — ensure this is intentional' });
        } else {
            results.push({ cls: 'pass', icon: 'bi-check-circle-fill', text: 'Standard access level — no admin privileges' });
        }

        // Check: roles
        if (rolesSelected > 0) {
            results.push({ cls: 'pass', icon: 'bi-check-circle-fill', text: `${rolesSelected} role(s) assigned` });
        } else {
            results.push({ cls: 'info', icon: 'bi-info-circle-fill', text: 'No roles assigned — user will have minimal access' });
        }

        // Check: external user with web access
        if ((type === 'VENDOR' || type === 'CUSTOMER') && $('ucIsWebAccess')?.checked) {
            results.push({ cls: 'warn', icon: 'bi-exclamation-triangle-fill', text: `${type} with web access — review access scope` });
        }

        // HR notification
        results.push({ cls: 'info', icon: 'bi-envelope-fill', text: 'HR department will be notified about the new user' });

        // Activity logging
        results.push({ cls: 'pass', icon: 'bi-journal-check', text: 'User creation will be logged in activity trail' });

        checks.innerHTML = results.map(r => `
            <div class="uc-ai-check ${r.cls}">
                <i class="bi ${r.icon}"></i>
                <span>${r.text}</span>
            </div>
        `).join('');
    }

    // ══════════════════════════════════════
    // CREATE USER
    // ══════════════════════════════════════
    async function createUser() {
        // Final validation
        for (let s = 1; s <= 3; s++) {
            if (!validateStep(s)) {
                goStep(s);
                return;
            }
        }

        const payload = {
            userCode: val('ucUserCode'),
            username: val('ucUsername'),
            name: val('ucName'),
            email: val('ucEmail') || null,
            mobile: val('ucMobile') || null,
            userType: val('ucUserType'),
            departmentId: parseInt(val('ucDepartment')),
            designationId: parseInt(val('ucDesignation')),
            locationId: parseInt(val('ucLocation')),
            employeeCode: val('ucEmpCode') || null,
            password: val('ucPassword'),
            isSystemAdmin: $('ucIsSystemAdmin')?.checked ?? false,
            isApprovalUser: $('ucIsApprovalUser')?.checked ?? false,
            isProductionUser: $('ucIsProductionUser')?.checked ?? false,
            isWebAccess: $('ucIsWebAccess')?.checked ?? true,
            isMobileAccess: $('ucIsMobileAccess')?.checked ?? false,
            roleIds: Array.from(document.querySelectorAll('#ucRoleGrid input:checked')).map(c => parseInt(c.value))
        };

        // Disable button to prevent double-submit
        const btn = $('ucBtnCreate');
        if (btn) { btn.disabled = true; btn.innerHTML = '<i class="bi bi-hourglass-split me-1"></i>Creating…'; }

        try {
            const res = await postJson(`${API}/users`, payload);

            await Swal.fire({
                icon: 'success',
                title: 'User Created Successfully!',
                html: `
                    <div class="text-start" style="font-size:.9rem;">
                        <div class="mb-2"><strong>${esc(payload.name)}</strong> has been created.</div>
                        <div class="p-3 rounded" style="background:#f1f5f9;">
                            <div class="mb-1"><i class="bi bi-person me-1"></i><strong>Username:</strong> ${esc(payload.username)}</div>
                            <div class="mb-1"><i class="bi bi-hash me-1"></i><strong>User Code:</strong> ${esc(payload.userCode)}</div>
                            ${payload.email ? `<div class="mb-1"><i class="bi bi-envelope-check me-1 text-success"></i>Welcome email sent to <strong>${esc(payload.email)}</strong></div>` : ''}
                            <div><i class="bi bi-bell me-1 text-info"></i>HR has been notified</div>
                        </div>
                    </div>
                `,
                confirmButtonText: 'Go to User Management',
                allowOutsideClick: false
            });

            window.location.href = '/Maintenance/UserManagement';
        } catch (e) {
            Swal.fire({ icon: 'error', title: 'Error', text: e.message });
        } finally {
            if (btn) { btn.disabled = false; btn.innerHTML = '<i class="bi bi-person-check me-1"></i>Create User & Send Credentials'; }
        }
    }

    // ── Public API ──
    return {
        init,
        goStep,
        nextStep,
        prevStep,
        togglePwdVisibility,
        generatePassword,
        createUser
    };
})();

document.addEventListener('DOMContentLoaded', () => UcApp.init());
