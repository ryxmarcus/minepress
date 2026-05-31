// ===== MinePress Login Page — Interactions =====

document.addEventListener('DOMContentLoaded', function () {
    // ── Password visibility toggle ──
    const toggleBtn = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('passwordInput');
    const eyeIcon = document.getElementById('eyeIcon');

    if (toggleBtn && passwordInput && eyeIcon) {
        toggleBtn.addEventListener('click', function (e) {
            e.preventDefault();
            const isPassword = passwordInput.type === 'password';
            passwordInput.type = isPassword ? 'text' : 'password';
            eyeIcon.classList.toggle('bi-eye', !isPassword);
            eyeIcon.classList.toggle('bi-eye-slash', isPassword);
        });
    }

    // ── Button loading state on form submit ──
    const form = document.querySelector('.login-card form');
    const btnLogin = document.getElementById('btnLogin');

    if (form && btnLogin) {
        form.addEventListener('submit', function () {
            if (form.checkValidity()) {
                btnLogin.disabled = true;
                btnLogin.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Signing in\u2026';
            }
        });
    }

    // ── Clear validation on input ──
    document.querySelectorAll('.login-card .form-control').forEach(function (input) {
        input.addEventListener('input', function () {
            this.classList.remove('is-invalid');
            var span = this.closest('.login-input-wrap')?.querySelector('.text-danger');
            if (span) span.textContent = '';
        });
    });
});
