// ===== MinePress — SweetAlert2 Helper Functions =====
// Shared across all ERP modules for consistent notifications & confirmations.

const Swal2 = {

    // ── Toast (auto-dismiss, top-end) ──
    toast(message, icon = 'success', timer = 3000) {
        const Toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer,
            timerProgressBar: true,
            didOpen: (el) => {
                el.onmouseenter = Swal.stopTimer;
                el.onmouseleave = Swal.resumeTimer;
            }
        });
        return Toast.fire({ icon, title: message });
    },

    success(message, timer) { return this.toast(message, 'success', timer); },
    error(message, timer)   { return this.toast(message, 'error', timer || 4000); },
    warning(message, timer) { return this.toast(message, 'warning', timer || 4000); },
    info(message, timer)    { return this.toast(message, 'info', timer); },

    // ── Alert (centered modal — for important messages) ──
    alert(title, message, icon = 'info') {
        return Swal.fire({
            title,
            html: message,
            icon,
            confirmButtonText: 'OK',
            customClass: { confirmButton: 'btn btn-primary px-4' },
            buttonsStyling: false
        });
    },

    // ── Confirm (returns Promise<boolean>) ──
    async confirm(title, message, { icon = 'warning', confirmText = 'Yes', cancelText = 'Cancel', confirmClass = 'btn btn-danger', dangerMode = false } = {}) {
        const result = await Swal.fire({
            title,
            html: message,
            icon,
            showCancelButton: true,
            confirmButtonText: confirmText,
            cancelButtonText: cancelText,
            customClass: {
                confirmButton: confirmClass + ' px-4 me-2',
                cancelButton: 'btn btn-secondary px-4'
            },
            buttonsStyling: false,
            reverseButtons: true,
            focusCancel: dangerMode
        });
        return result.isConfirmed;
    },

    // ── Delete Confirm (red themed) ──
    confirmDelete(itemName) {
        return this.confirm(
            'Delete?',
            `This will permanently delete <strong>${itemName}</strong>.<br>This action cannot be undone.`,
            { icon: 'warning', confirmText: '<i class="bi bi-trash me-1"></i>Delete', confirmClass: 'btn btn-danger', dangerMode: true }
        );
    },

    // ── Status Change Confirm ──
    confirmStatus(itemName, newStatus) {
        const iconMap = { 'CANCELLED': 'warning', 'CLOSED': 'question', 'SUBMITTED': 'question', 'APPROVED': 'question' };
        const colorMap = { 'CANCELLED': 'btn btn-warning', 'CLOSED': 'btn btn-secondary', 'SUBMITTED': 'btn btn-info', 'APPROVED': 'btn btn-success' };
        return this.confirm(
            `Change Status?`,
            `Change <strong>${itemName}</strong> status to <strong>${newStatus}</strong>?`,
            { icon: iconMap[newStatus] || 'question', confirmText: `Yes, ${newStatus}`, confirmClass: colorMap[newStatus] || 'btn btn-primary' }
        );
    },

    // ── Save Success (with redirect option) ──
    async saveSuccess(title, message, redirectUrl) {
        await Swal.fire({
            icon: 'success',
            title,
            html: message,
            showConfirmButton: true,
            confirmButtonText: redirectUrl ? 'View' : 'OK',
            showCancelButton: !!redirectUrl,
            cancelButtonText: 'Stay Here',
            customClass: {
                confirmButton: 'btn btn-success px-4 me-2',
                cancelButton: 'btn btn-secondary px-4'
            },
            buttonsStyling: false,
            timer: redirectUrl ? 3000 : undefined,
            timerProgressBar: !!redirectUrl
        }).then((result) => {
            if (redirectUrl && (result.isConfirmed || result.dismiss === Swal.DismissReason.timer)) {
                window.location.href = redirectUrl;
            }
        });
    },

    // ── Loading state ──
    showLoading(title = 'Processing...') {
        Swal.fire({
            title,
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: () => Swal.showLoading()
        });
    },

    hideLoading() {
        Swal.close();
    }
};
