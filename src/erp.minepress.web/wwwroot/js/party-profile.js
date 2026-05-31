/**
 * PartyProfile — Profile page logic for Party Portal
 */
const PartyProfile = (() => {
    let profileData = null;
    let isEditing = false;

    async function init() {
        await loadProfile();
    }

    async function loadProfile() {
        document.getElementById('profileLoading').style.display = '';
        document.getElementById('profileContent').style.display = 'none';
        try {
            const res = await fetch('/api/PartyPortal/profile');
            if (!res.ok) throw new Error('Failed to load profile');
            profileData = await res.json();
            renderProfile(profileData);
            document.getElementById('profileLoading').style.display = 'none';
            document.getElementById('profileContent').style.display = '';
        } catch (err) {
            document.getElementById('profileLoading').innerHTML =
                '<div class="text-danger"><i class="bi bi-exclamation-triangle me-1"></i>Failed to load profile</div>';
        }
    }

    function renderProfile(p) {
        // Identity card
        document.getElementById('profileInitial').textContent = (p.name || '?')[0].toUpperCase();
        document.getElementById('profileName').textContent = p.name || '—';
        document.getElementById('profileCode').textContent = p.code || '—';
        document.getElementById('profileStatus').innerHTML =
            `<i class="bi bi-circle-fill me-1" style="font-size:0.5rem;"></i>${p.isActive ? 'Active' : 'Inactive'}`;
        document.getElementById('profileStatus').className = p.isActive ? 'badge bg-green-lt' : 'badge bg-red-lt';
        document.getElementById('profileSince').innerHTML =
            `<i class="bi bi-calendar3 me-1"></i>Since ${p.createdOn}`;

        // Role badges
        const badgesHtml = (p.roles || []).map(r => {
            const color = r === 'Customer' ? 'bg-blue' : r === 'Supplier' ? 'bg-green' : r === 'Vendor' ? 'bg-orange' : 'bg-secondary';
            return `<span class="badge ${color} me-1">${r}</span>`;
        }).join('');
        document.getElementById('profileRoleBadges').innerHTML = badgesHtml;

        // Quick info
        document.getElementById('infoEmail').textContent = p.email || '—';
        document.getElementById('infoMobile').textContent = p.mobile || '—';
        document.getElementById('infoGst').textContent = p.gstNo || '—';
        document.getElementById('infoPan').textContent = p.panNo || '—';

        // Editable fields
        document.getElementById('editEmail').value = p.email || '';
        document.getElementById('editMobile').value = p.mobile || '';
        document.getElementById('editAddress1').value = p.address1 || '';
        document.getElementById('editAddress2').value = p.address2 || '';
        document.getElementById('editCity').value = p.cityName || '';
        document.getElementById('editPin').value = p.pin || '';

        // Addresses
        renderAddresses(p.addresses || []);

        // Contacts
        renderContacts(p.contacts || []);

        // Banks
        renderBanks(p.banks || []);
    }

    function renderAddresses(addresses) {
        const container = document.getElementById('addressesContainer');
        if (!addresses.length) {
            container.innerHTML = '<div class="pp-empty-state py-4"><i class="bi bi-geo"></i><p>No addresses found</p></div>';
            return;
        }
        container.innerHTML = addresses.map(a => {
            const lines = [a.addressLine1, a.addressLine2].filter(Boolean).join(', ');
            const location = [a.cityName, a.stateName, a.postalCode].filter(Boolean).join(', ');
            const gstLine = a.gstin ? `<span class="pp-addr-meta">GSTIN: ${esc(a.gstin)}</span>` : '';
            const contactLine = a.contactPersonName
                ? `<span class="pp-addr-meta"><i class="bi bi-person me-1"></i>${esc(a.contactPersonName)}${a.contactPhone ? ' · ' + esc(a.contactPhone) : ''}</span>`
                : '';
            const defaultBadge = a.isDefault ? '<span class="badge bg-green-lt ms-2">Default</span>' : '';
            return `
                <div class="pp-address-card">
                    <div class="d-flex align-items-center mb-1">
                        <span class="pp-addr-type">${esc(a.addressType || 'Address')}</span>
                        ${a.addressLabel ? `<span class="text-secondary small ms-2">(${esc(a.addressLabel)})</span>` : ''}
                        ${defaultBadge}
                    </div>
                    <div class="pp-addr-line">${esc(lines)}</div>
                    ${location ? `<div class="pp-addr-meta"><i class="bi bi-pin-map me-1"></i>${esc(location)}</div>` : ''}
                    ${gstLine}
                    ${contactLine}
                </div>`;
        }).join('');
    }

    function renderContacts(contacts) {
        const container = document.getElementById('contactsContainer');
        if (!contacts.length) {
            container.innerHTML = '<div class="pp-empty-state py-4"><i class="bi bi-person-lines-fill"></i><p>No contacts found</p></div>';
            return;
        }
        container.innerHTML = contacts.map(c => {
            const initial = (c.contactName || '?')[0].toUpperCase();
            const meta = [c.designation, c.email, c.mobile].filter(Boolean).join(' · ');
            return `
                <div class="pp-contact-row">
                    <div class="pp-contact-avatar">${initial}</div>
                    <div class="pp-contact-info">
                        <div class="pp-contact-name">${esc(c.contactName || '—')}</div>
                        <div class="pp-contact-meta">${esc(meta)}</div>
                    </div>
                </div>`;
        }).join('');
    }

    function renderBanks(banks) {
        const container = document.getElementById('banksContainer');
        if (!banks.length) {
            container.innerHTML = '<div class="pp-empty-state py-4"><i class="bi bi-safe"></i><p>No bank details found</p></div>';
            return;
        }
        container.innerHTML = banks.map(b => `
            <div class="pp-bank-row">
                <div>
                    <div class="pp-detail-label">Bank</div>
                    <div class="pp-detail-value">${esc(b.bankName || '—')}</div>
                </div>
                <div>
                    <div class="pp-detail-label">Branch</div>
                    <div class="pp-detail-value">${esc(b.branchName || '—')}</div>
                </div>
                <div>
                    <div class="pp-detail-label">Account No</div>
                    <div class="pp-detail-value">${maskAccount(b.accountNo)}</div>
                </div>
                <div>
                    <div class="pp-detail-label">IFSC</div>
                    <div class="pp-detail-value">${esc(b.ifscCode || '—')}</div>
                </div>
                <div>
                    <div class="pp-detail-label">MICR</div>
                    <div class="pp-detail-value">${esc(b.micrNo || '—')}</div>
                </div>
            </div>`).join('');
    }

    function toggleEdit() {
        if (!isEditing) {
            // Enter edit mode
            isEditing = true;
            setFieldsDisabled(false);
            document.getElementById('btnEditSave').innerHTML = '<i class="bi bi-check-lg me-1"></i>Save';
            document.getElementById('btnEditSave').className = 'btn btn-success btn-sm';
            document.getElementById('btnEditSave').setAttribute('onclick', 'PartyProfile.saveProfile()');
            document.getElementById('btnCancelEdit').style.display = '';
        }
    }

    function cancelEdit() {
        isEditing = false;
        setFieldsDisabled(true);
        document.getElementById('btnEditSave').innerHTML = '<i class="bi bi-pencil me-1"></i>Edit';
        document.getElementById('btnEditSave').className = 'btn btn-primary btn-sm';
        document.getElementById('btnEditSave').setAttribute('onclick', 'PartyProfile.toggleEdit()');
        document.getElementById('btnCancelEdit').style.display = 'none';
        // Restore original values
        if (profileData) {
            document.getElementById('editEmail').value = profileData.email || '';
            document.getElementById('editMobile').value = profileData.mobile || '';
            document.getElementById('editAddress1').value = profileData.address1 || '';
            document.getElementById('editAddress2').value = profileData.address2 || '';
            document.getElementById('editPin').value = profileData.pin || '';
        }
    }

    async function saveProfile() {
        const payload = {
            email: document.getElementById('editEmail').value.trim() || null,
            mobile: parseInt(document.getElementById('editMobile').value) || null,
            address1: document.getElementById('editAddress1').value.trim(),
            address2: document.getElementById('editAddress2').value.trim(),
            pin: document.getElementById('editPin').value.trim()
        };

        try {
            const res = await fetch('/api/PartyPortal/profile/update', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            const data = await res.json();
            if (!res.ok) {
                Swal.fire('Error', data.message || 'Failed to update profile', 'error');
                return;
            }
            Swal.fire({ icon: 'success', title: 'Updated', text: data.message, timer: 1800, showConfirmButton: false });
            cancelEdit();
            await loadProfile();
        } catch {
            Swal.fire('Error', 'An unexpected error occurred', 'error');
        }
    }

    async function changePassword() {
        const current = document.getElementById('currentPassword').value;
        const newPwd = document.getElementById('newPassword').value;
        const confirm = document.getElementById('confirmPassword').value;

        if (!current || !newPwd || !confirm) {
            Swal.fire('Missing Fields', 'Please fill in all password fields.', 'warning');
            return;
        }
        if (newPwd !== confirm) {
            Swal.fire('Mismatch', 'New password and confirmation do not match.', 'warning');
            return;
        }
        if (newPwd.length < 6) {
            Swal.fire('Too Short', 'Password must be at least 6 characters.', 'warning');
            return;
        }

        try {
            const res = await fetch('/api/PartyPortal/profile/change-password', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ currentPassword: current, newPassword: newPwd, confirmPassword: confirm })
            });
            const data = await res.json();
            if (!res.ok) {
                Swal.fire('Error', data.message || 'Failed to change password', 'error');
                return;
            }
            Swal.fire({ icon: 'success', title: 'Password Changed', text: data.message, timer: 2000, showConfirmButton: false });
            document.getElementById('currentPassword').value = '';
            document.getElementById('newPassword').value = '';
            document.getElementById('confirmPassword').value = '';
        } catch {
            Swal.fire('Error', 'An unexpected error occurred', 'error');
        }
    }

    function setFieldsDisabled(disabled) {
        ['editEmail', 'editMobile', 'editAddress1', 'editAddress2', 'editPin'].forEach(id => {
            document.getElementById(id).disabled = disabled;
        });
    }

    function maskAccount(acct) {
        if (!acct || acct.length <= 4) return acct || '—';
        return '••••' + acct.slice(-4);
    }

    function esc(str) {
        if (!str) return '';
        const d = document.createElement('div');
        d.textContent = str;
        return d.innerHTML;
    }

    // Auto-init
    document.addEventListener('DOMContentLoaded', init);

    return { loadProfile, toggleEdit, cancelEdit, saveProfile, changePassword };
})();
