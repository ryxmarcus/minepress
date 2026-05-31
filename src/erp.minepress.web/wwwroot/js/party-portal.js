/**
 * MinePress Party Portal — Modern AI Dashboard JS
 */
const PartyPortal = (() => {
    const API = '/api/PartyPortal';
    let _config = { partyId: null, roles: [], partyName: '' };

    // ── Color map for insight icons ──
    const colorMap = {
        blue: 'pp-ici-blue', purple: 'pp-ici-purple', green: 'pp-ici-green',
        orange: 'pp-ici-orange', teal: 'pp-ici-teal', pink: 'pp-ici-pink',
        amber: 'pp-ici-amber', indigo: 'pp-ici-indigo', gold: 'pp-ici-gold',
        red: 'pp-ici-red', cyan: 'pp-ici-blue'
    };

    // ── Bar color map for activity chart ──
    const barColors = {
        ENQUIRY: '#206bc4', QUOTATION: '#9333ea', JOB: '#2563eb',
        CHALLAN: '#06b6d4', INVOICE: '#ea580c', RECEIPT: '#16a34a',
        PAYMENT: '#059669', PURCHASE: '#14b8a6', OUTSOURCE: '#f59e0b',
        PRODUCTION: '#6366f1', APPROVAL: '#10b981', DOCUMENT: '#6b7280',
        COMMUNICATION: '#3b82f6', VISIT: '#ec4899', COMPLAINT: '#ef4444'
    };

    function init(config) {
        _config = config || _config;
        setGreeting();
        loadStats();
        loadSectionData();
    }

    // ── Time-based greeting ──
    function setGreeting() {
        const el = document.getElementById('pp-greeting');
        if (!el) return;
        const h = new Date().getHours();
        const prefix = h < 12 ? 'Good Morning' : h < 17 ? 'Good Afternoon' : 'Good Evening';
        el.textContent = `${prefix}, ${_config.partyName || 'Welcome'}`;
    }

    // ── Animated counter ──
    function animateCounter(el, target, duration) {
        if (!el || target == null) return;
        const isCurrency = el.classList.contains('pp-stat-currency');
        const start = 0;
        const end = typeof target === 'number' ? target : parseFloat(target) || 0;
        if (end === 0) { el.textContent = isCurrency ? '0' : '0'; return; }

        const startTime = performance.now();
        const dur = duration || 800;

        function tick(now) {
            const elapsed = now - startTime;
            const progress = Math.min(elapsed / dur, 1);
            const ease = 1 - Math.pow(1 - progress, 3); // ease-out cubic
            const current = Math.round(start + (end - start) * ease);
            el.textContent = isCurrency ? current.toLocaleString('en-IN') : current.toString();
            if (progress < 1) requestAnimationFrame(tick);
        }
        requestAnimationFrame(tick);
    }

    function setStatAnimated(id, value) {
        const el = document.getElementById(id);
        if (el && value !== undefined && value !== null) {
            el.dataset.target = value;
            animateCounter(el, value);
        }
    }

    function setTrend(id, value) {
        const el = document.getElementById(id);
        if (!el) return;
        if (value > 0) {
            el.className = 'pp-stat-trend up';
            el.innerHTML = `<i class="bi bi-arrow-up-short"></i>${value}%`;
        } else if (value < 0) {
            el.className = 'pp-stat-trend down';
            el.innerHTML = `<i class="bi bi-arrow-down-short"></i>${Math.abs(value)}%`;
        } else {
            el.className = 'pp-stat-trend neutral';
            el.innerHTML = `<i class="bi bi-dash"></i>0%`;
        }
    }

    // ── Load Dashboard Stats ──
    async function loadStats() {
        try {
            const res = await fetch(`${API}/stats`);
            if (!res.ok) return;
            const d = await res.json();

            // Customer stats
            setStatAnimated('stat-cust-enquiries', d.customerEnquiries);
            setStatAnimated('stat-cust-quotations', d.customerPendingQuotations);
            setStatAnimated('stat-cust-jobs', d.customerActiveJobs);
            setStatAnimated('stat-cust-challans', d.customerChallans);
            setTrend('stat-cust-enq-trend', d.customerEnqTrend);

            // Supplier stats
            setStatAnimated('stat-supp-grns', d.supplierGrns);
            setStatAnimated('stat-supp-pending', d.supplierPendingGrns);
            setStatAnimated('stat-supp-payments', d.supplierPaymentCount);
            setStatAnimated('stat-supp-pay-total', d.supplierPaymentTotal);

            // Vendor stats
            setStatAnimated('stat-vend-total', d.vendorOutsourceTotal);
            setStatAnimated('stat-vend-active', d.vendorActiveOutsource);
            setStatAnimated('stat-vend-completed', d.vendorCompletedOutsource);
            setStatAnimated('stat-vend-payments', d.vendorPaymentTotal);

            // AI Insights
            renderInsights(d.insights || []);

            // Activity breakdown chart
            renderActivityChart(d.activityBreakdown || [], d.recentActivityCount, d.activityTrend);

        } catch (err) {
            console.error('Failed to load portal stats:', err);
        }
    }

    // ── Render AI Insights ──
    function renderInsights(insights) {
        const grid = document.getElementById('pp-insights-grid');
        if (!grid) return;

        if (!insights || insights.length === 0) {
            grid.innerHTML = '<div class="text-center text-secondary py-2"><small>No insights available yet.</small></div>';
            return;
        }

        let html = '';
        insights.forEach((ins, idx) => {
            const iconCls = colorMap[ins.color] || 'pp-ici-blue';
            html += `
                <div class="pp-insight-card" style="animation-delay:${idx * 80}ms">
                    <div class="pp-insight-card-icon ${iconCls}">
                        <i class="bi ${esc(ins.icon)}"></i>
                    </div>
                    <div class="pp-insight-card-body">
                        <div class="pp-insight-card-title">${esc(ins.title)}</div>
                        <div class="pp-insight-card-msg">${esc(ins.message)}</div>
                    </div>
                </div>`;
        });
        grid.innerHTML = html;
    }

    // ── Render Activity Breakdown Bar Chart ──
    function renderActivityChart(breakdown, recentCount, trend) {
        const chart = document.getElementById('pp-activity-chart');
        const footer = document.getElementById('pp-activity-summary-footer');
        if (!chart) return;

        if (!breakdown || breakdown.length === 0) {
            chart.innerHTML = '<div class="text-center text-secondary py-3"><i class="bi bi-pie-chart" style="font-size:1.5rem;opacity:.2"></i><div class="small mt-1">No activity data yet</div></div>';
            if (footer) footer.innerHTML = '';
            return;
        }

        const max = Math.max(...breakdown.map(b => b.count), 1);
        let html = '';
        // Sort by count descending, take top 8
        const sorted = breakdown.sort((a, b) => b.count - a.count).slice(0, 8);
        sorted.forEach(item => {
            const pct = Math.round((item.count / max) * 100);
            const color = barColors[item.type] || '#6b7280';
            html += `
                <div class="pp-bar-row">
                    <span class="pp-bar-label">${esc(item.type)}</span>
                    <div class="pp-bar-track">
                        <div class="pp-bar-fill" style="width:${pct}%;background:${color}"></div>
                    </div>
                    <span class="pp-bar-count">${item.count}</span>
                </div>`;
        });
        chart.innerHTML = html;

        // Footer summary
        if (footer) {
            const total = breakdown.reduce((s, b) => s + b.count, 0);
            const trendHtml = trend > 0
                ? `<span style="color:#10b981"><i class="bi bi-arrow-up-short"></i>${trend}%</span>`
                : trend < 0
                    ? `<span style="color:#ef4444"><i class="bi bi-arrow-down-short"></i>${Math.abs(trend)}%</span>`
                    : `<span style="color:#94a3b8">—</span>`;
            footer.innerHTML = `<span class="small text-secondary">${total} total · ${recentCount || 0} this week ${trendHtml}</span>`;
        }
    }

    // ── Load Section Data ──
    async function loadSectionData() {
        const roles = _config.roles || [];

        if (roles.includes('Customer')) {
            // Load workspace tasks and approvals (new feature)
            loadWorkspaceSummary();
            loadWorkspaceApprovals();
            loadWorkspaceTasks();

            // Legacy section loaders (kept for backward compatibility)
            loadListSection('approvals', `${API}/customer/approvals`, 'bi-check-circle', 'bg-success-lt text-success');
            loadListSection('job-tracking', `${API}/customer/job-tracking`, 'bi-geo-alt', 'bg-primary-lt text-primary');
            loadListSection('requests', `${API}/customer/requests`, 'bi-send', 'bg-info-lt text-info');
            loadListSection('complaints', `${API}/customer/complaints`, 'bi-exclamation-triangle', 'bg-warning-lt text-warning');
            loadListSection('feedback', `${API}/customer/feedback`, 'bi-chat-heart', 'bg-danger-lt text-danger');
        }

        if (roles.includes('Supplier')) {
            loadListSection('purchase-orders', `${API}/supplier/purchase-orders`, 'bi-cart3', 'bg-teal-lt text-teal');
        }

        if (roles.includes('Vendor')) {
            loadListSection('contracts', `${API}/vendor/contracts`, 'bi-file-earmark-text', 'bg-danger-lt text-danger');
        }
    }

    async function loadListSection(sectionId, url, defaultIcon, defaultIconClass) {
        const listEl = document.getElementById(`${sectionId}-list`);
        if (!listEl) return;

        try {
            const res = await fetch(url);
            if (!res.ok) return;
            const data = await res.json();

            if (!data.items || data.items.length === 0) return;

            let html = '';
            data.items.forEach(item => {
                html += buildListItem(item, defaultIcon, defaultIconClass);
            });
            listEl.innerHTML = html;
        } catch (err) {
            console.error(`Failed to load ${sectionId}:`, err);
        }
    }

    function buildListItem(item, icon, iconClass) {
        const title = item.title || item.name || 'Untitled';
        const sub = item.subtitle || item.date || '';
        const badge = item.status
            ? `<span class="badge pp-list-item-badge ${item.statusClass || 'bg-secondary'}">${item.status}</span>`
            : '';

        return `
            <div class="pp-list-item">
                <div class="pp-list-item-icon ${iconClass}">
                    <i class="bi ${item.icon || icon}"></i>
                </div>
                <div class="pp-list-item-content">
                    <div class="pp-list-item-title">${title}</div>
                    ${sub ? `<div class="pp-list-item-sub">${sub}</div>` : ''}
                </div>
                ${badge}
            </div>`;
    }

    // ── Show Section (scroll into view) ──
    function showSection(sectionId) {
        const el = document.getElementById(`section-${sectionId}`);
        if (el) {
            el.scrollIntoView({ behavior: 'smooth', block: 'center' });
            el.style.boxShadow = '0 0 0 2px var(--pp-blue), var(--pp-shadow-lg)';
            setTimeout(() => { el.style.boxShadow = ''; }, 2000);
        }
    }

    // ── SweetAlert Dialogs ──
    function newRequest() {
        if (typeof Swal === 'undefined') return;
        Swal.fire({
            title: 'New Request',
            html:
                '<input id="swal-req-subject" class="swal2-input" placeholder="Subject">' +
                '<textarea id="swal-req-desc" class="swal2-textarea" placeholder="Describe your request..."></textarea>',
            showCancelButton: true,
            confirmButtonText: 'Submit',
            confirmButtonColor: '#3b82f6',
            preConfirm: () => {
                const subject = document.getElementById('swal-req-subject').value;
                const desc = document.getElementById('swal-req-desc').value;
                if (!subject) { Swal.showValidationMessage('Subject is required'); return false; }
                return { subject, description: desc };
            }
        }).then(result => {
            if (result.isConfirmed) {
                Swal.fire({ icon: 'success', title: 'Request Submitted', text: 'Your request has been submitted successfully.', timer: 2000, showConfirmButton: false });
            }
        });
    }

    function newComplaint() {
        if (typeof Swal === 'undefined') return;
        Swal.fire({
            title: 'New Complaint',
            html:
                '<input id="swal-cmp-subject" class="swal2-input" placeholder="Subject">' +
                '<select id="swal-cmp-priority" class="swal2-select">' +
                '  <option value="Low">Low Priority</option>' +
                '  <option value="Medium" selected>Medium Priority</option>' +
                '  <option value="High">High Priority</option>' +
                '  <option value="Critical">Critical</option>' +
                '</select>' +
                '<textarea id="swal-cmp-desc" class="swal2-textarea" placeholder="Describe the issue..."></textarea>',
            showCancelButton: true,
            confirmButtonText: 'Submit Complaint',
            confirmButtonColor: '#d97706',
            preConfirm: () => {
                const subject = document.getElementById('swal-cmp-subject').value;
                if (!subject) { Swal.showValidationMessage('Subject is required'); return false; }
                return { subject, priority: document.getElementById('swal-cmp-priority').value, description: document.getElementById('swal-cmp-desc').value };
            }
        }).then(result => {
            if (result.isConfirmed) {
                Swal.fire({ icon: 'success', title: 'Complaint Registered', text: 'Your complaint has been registered. We will get back to you soon.', timer: 2500, showConfirmButton: false });
            }
        });
    }

    function newFeedback() {
        if (typeof Swal === 'undefined') return;
        Swal.fire({
            title: 'Share Your Feedback',
            html:
                '<div style="margin-bottom:10px">' +
                '  <label style="font-weight:600">Rating</label><br>' +
                '  <div id="swal-fb-stars" style="font-size:1.8rem;cursor:pointer;color:#e2e8f0">' +
                '    <i class="bi bi-star-fill" data-val="1"></i> ' +
                '    <i class="bi bi-star-fill" data-val="2"></i> ' +
                '    <i class="bi bi-star-fill" data-val="3"></i> ' +
                '    <i class="bi bi-star-fill" data-val="4"></i> ' +
                '    <i class="bi bi-star-fill" data-val="5"></i>' +
                '  </div>' +
                '</div>' +
                '<textarea id="swal-fb-comment" class="swal2-textarea" placeholder="Your comments..."></textarea>',
            showCancelButton: true,
            confirmButtonText: 'Submit Feedback',
            confirmButtonColor: '#059669',
            didOpen: () => {
                let rating = 0;
                const container = document.getElementById('swal-fb-stars');
                if (!container) return;
                container.addEventListener('click', (e) => {
                    const star = e.target.closest('[data-val]');
                    if (!star) return;
                    rating = parseInt(star.dataset.val);
                    container.querySelectorAll('i').forEach((s, i) => {
                        s.style.color = i < rating ? '#f59e0b' : '#e2e8f0';
                    });
                    container.dataset.rating = rating;
                });
            },
            preConfirm: () => {
                const container = document.getElementById('swal-fb-stars');
                const rating = parseInt(container?.dataset.rating || '0');
                if (rating === 0) { Swal.showValidationMessage('Please select a rating'); return false; }
                return { rating, comment: document.getElementById('swal-fb-comment').value };
            }
        }).then(result => {
            if (result.isConfirmed) {
                Swal.fire({ icon: 'success', title: 'Thank You!', text: 'Your feedback has been submitted.', timer: 2000, showConfirmButton: false });
            }
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // WORKSPACE: Tasks & Approvals Management
    // ═══════════════════════════════════════════════════════════════

    async function loadWorkspaceSummary() {
        try {
            const res = await fetch(`${API}/workspace/summary`);
            if (!res.ok) return;
            const data = await res.json();

            // Update summary badges
            const apprBadge = document.getElementById('ws-badge-approvals');
            const taskBadge = document.getElementById('ws-badge-tasks');
            const apprCount = document.getElementById('ws-count-approvals');
            const taskCount = document.getElementById('ws-count-tasks');

            if (apprBadge) apprBadge.textContent = data.pendingApprovals || 0;
            if (taskBadge) taskBadge.textContent = data.activeTasks || 0;
            if (apprCount) apprCount.textContent = data.pendingApprovals || 0;
            if (taskCount) taskCount.textContent = data.activeTasks || 0;

            // Show/hide badge colors based on count
            if (apprBadge && data.pendingApprovals > 0) {
                apprBadge.className = 'badge bg-danger';
            }
            if (taskBadge && data.activeTasks > 0) {
                taskBadge.className = 'badge bg-warning text-dark';
            }

        } catch (err) {
            console.error('Failed to load workspace summary:', err);
        }
    }

    async function loadWorkspaceApprovals() {
        const listEl = document.getElementById('workspace-approvals-list');
        if (!listEl) return;

        try {
            const res = await fetch(`${API}/workspace/approvals?filter=pending`);
            if (!res.ok) return;
            const data = await res.json();

            if (!data.items || data.items.length === 0) {
                listEl.innerHTML = '<div class="pp-empty-state"><i class="bi bi-inbox"></i><p>No pending approvals</p></div>';
                return;
            }

            let html = '';
            data.items.forEach(item => {
                html += buildWorkspaceItem(item, 'approval');
            });
            listEl.innerHTML = html;

            // Attach event listeners
            attachWorkspaceListeners(listEl);

        } catch (err) {
            console.error('Failed to load workspace approvals:', err);
            listEl.innerHTML = '<div class="pp-empty-state text-danger"><i class="bi bi-exclamation-triangle"></i><p>Failed to load approvals</p></div>';
        }
    }

    async function loadWorkspaceTasks() {
        const listEl = document.getElementById('workspace-tasks-list');
        if (!listEl) return;

        try {
            const res = await fetch(`${API}/workspace/tasks?filter=pending`);
            if (!res.ok) return;
            const data = await res.json();

            if (!data.items || data.items.length === 0) {
                listEl.innerHTML = '<div class="pp-empty-state"><i class="bi bi-inbox"></i><p>No active tasks</p></div>';
                return;
            }

            let html = '';
            data.items.forEach(item => {
                html += buildWorkspaceItem(item, 'task');
            });
            listEl.innerHTML = html;

            // Attach event listeners
            attachWorkspaceListeners(listEl);

        } catch (err) {
            console.error('Failed to load workspace tasks:', err);
            listEl.innerHTML = '<div class="pp-empty-state text-danger"><i class="bi bi-exclamation-triangle"></i><p>Failed to load tasks</p></div>';
        }
    }

    function buildWorkspaceItem(item, type) {
        const priorityClass = item.priority ? `priority-${item.priority.toLowerCase()}` : '';
        const overdueClass = item.isOverdue ? 'is-overdue' : '';
        const iconClass = item.isOverdue ? 'overdue' : type;
        const icon = type === 'approval' ? 'bi-shield-check' : 'bi-list-task';

        // Build badges section
        let badges = '';
        if (item.priority && ['CRITICAL', 'URGENT', 'HIGH'].includes(item.priority)) {
            const priorityIcon = item.priority === 'CRITICAL' ? 'bi-exclamation-octagon-fill' : 
                                 item.priority === 'URGENT' ? 'bi-exclamation-triangle-fill' : 'bi-flag-fill';
            badges += `<span class="pp-ws-badge ${item.priority.toLowerCase()}"><i class="bi ${priorityIcon}"></i> ${item.priority}</span>`;
        }
        if (item.isOverdue) {
            badges += '<span class="pp-ws-badge overdue"><i class="bi bi-clock-history"></i> OVERDUE</span>';
        }
        if (type === 'approval' && item.canApprove) {
            badges += '<span class="pp-ws-badge pending-approval"><i class="bi bi-hourglass-split"></i> Awaiting</span>';
        }

        // Build meta items with special classes
        let metaHtml = '';
        if (item.jobNo) {
            metaHtml += `<span class="pp-ws-meta-item job-ref"><i class="bi bi-folder2-open"></i> ${esc(item.jobNo)}</span>`;
        }
        if (item.processName) {
            metaHtml += `<span class="pp-ws-meta-item process"><i class="bi bi-gear-wide-connected"></i> ${esc(item.processName)}</span>`;
        }
        if (item.dueDate) {
            const dueDateClass = item.isOverdue ? 'due-date overdue' : 'due-date';
            const relTime = formatRelativeTime(item.dueDate);
            metaHtml += `<span class="pp-ws-meta-item ${dueDateClass}"><i class="bi bi-calendar-event"></i> ${relTime}</span>`;
        }
        if (item.createdAt) {
            metaHtml += `<span class="pp-ws-meta-item"><i class="bi bi-clock"></i> ${formatRelativeTime(item.createdAt)}</span>`;
        }

        // Action buttons
        const actionButtons = type === 'approval' && item.canApprove
            ? `<div class="pp-ws-actions">
                   <button class="pp-ws-btn pp-ws-btn-approve" data-action="approve" data-id="${item.workspaceTaskId}" title="Approve this item">
                       <i class="bi bi-check-lg"></i> Approve
                   </button>
                   <button class="pp-ws-btn pp-ws-btn-reject" data-action="reject" data-id="${item.workspaceTaskId}" title="Reject this item">
                       <i class="bi bi-x-lg"></i> Reject
                   </button>
               </div>`
            : `<div class="pp-ws-actions">
                   <button class="pp-ws-btn pp-ws-btn-view" data-action="view" data-id="${item.workspaceTaskId}" data-url="${esc(item.actionUrl || '')}" title="View details">
                       <i class="bi bi-arrow-right-circle"></i> View
                   </button>
               </div>`;

        return `
            <div class="pp-ws-item ${priorityClass} ${overdueClass}" data-task-id="${item.workspaceTaskId}">
                <div class="pp-ws-icon ${iconClass}">
                    <i class="bi ${icon}"></i>
                </div>
                <div class="pp-ws-content">
                    <div class="pp-ws-header">
                        <h5 class="pp-ws-title">${esc(item.title)}</h5>
                        ${badges ? `<div class="pp-ws-badges">${badges}</div>` : ''}
                    </div>
                    ${item.description ? `<p class="pp-ws-desc">${esc(item.description)}</p>` : ''}
                    ${metaHtml ? `<div class="pp-ws-meta">${metaHtml}</div>` : ''}
                </div>
                ${actionButtons}
            </div>`;
    }

    // Helper function for relative time formatting
    function formatRelativeTime(dateStr) {
        if (!dateStr) return '';
        const date = new Date(dateStr);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        // Future dates
        if (diffMs < 0) {
            const futureMins = Math.abs(diffMins);
            const futureHours = Math.abs(diffHours);
            const futureDays = Math.abs(diffDays);
            if (futureMins < 60) return `in ${futureMins}m`;
            if (futureHours < 24) return `in ${futureHours}h`;
            if (futureDays === 1) return 'Tomorrow';
            if (futureDays < 7) return `in ${futureDays} days`;
            return date.toLocaleDateString('en-IN', { day: 'numeric', month: 'short' });
        }

        // Past dates
        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        if (diffHours < 24) return `${diffHours}h ago`;
        if (diffDays === 1) return 'Yesterday';
        if (diffDays < 7) return `${diffDays} days ago`;
        return date.toLocaleDateString('en-IN', { day: 'numeric', month: 'short' });
    }

    function attachWorkspaceListeners(container) {
        container.querySelectorAll('[data-action]').forEach(btn => {
            btn.addEventListener('click', handleWorkspaceAction);
        });
    }

    async function handleWorkspaceAction(e) {
        const btn = e.currentTarget;
        const action = btn.dataset.action;
        const id = btn.dataset.id;

        if (action === 'view') {
            const url = btn.dataset.url;
            if (url) {
                // Mark as viewed then navigate
                await fetch(`${API}/workspace/tasks/${id}/viewed`, { method: 'POST' });
                window.open(url, '_blank');
            }
            return;
        }

        if (action === 'approve') {
            approveItem(id);
        } else if (action === 'reject') {
            rejectItem(id);
        }
    }

    function approveItem(id) {
        if (typeof Swal === 'undefined') {
            if (confirm('Are you sure you want to approve this item?')) {
                submitApproval(id, 'approve', '');
            }
            return;
        }

        Swal.fire({
            title: 'Approve Item',
            html: '<textarea id="swal-approve-remarks" class="swal2-textarea" placeholder="Optional remarks..."></textarea>',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: '<i class="bi bi-check-lg me-1"></i> Approve',
            confirmButtonColor: '#2fb344',
            cancelButtonText: 'Cancel',
            focusConfirm: false,
            preConfirm: () => {
                return { remarks: document.getElementById('swal-approve-remarks')?.value || '' };
            }
        }).then(result => {
            if (result.isConfirmed) {
                submitApproval(id, 'approve', result.value.remarks);
            }
        });
    }

    function rejectItem(id) {
        if (typeof Swal === 'undefined') {
            const reason = prompt('Please provide a reason for rejection:');
            if (reason) {
                submitApproval(id, 'reject', reason);
            }
            return;
        }

        Swal.fire({
            title: 'Reject Item',
            html: '<textarea id="swal-reject-remarks" class="swal2-textarea" placeholder="Reason for rejection (required)..."></textarea>',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: '<i class="bi bi-x-lg me-1"></i> Reject',
            confirmButtonColor: '#d63939',
            cancelButtonText: 'Cancel',
            focusConfirm: false,
            preConfirm: () => {
                const remarks = document.getElementById('swal-reject-remarks')?.value || '';
                if (!remarks.trim()) {
                    Swal.showValidationMessage('Please provide a reason for rejection');
                    return false;
                }
                return { remarks };
            }
        }).then(result => {
            if (result.isConfirmed) {
                submitApproval(id, 'reject', result.value.remarks);
            }
        });
    }

    async function submitApproval(id, action, remarks) {
        const endpoint = action === 'approve'
            ? `${API}/workspace/approvals/${id}/approve`
            : `${API}/workspace/approvals/${id}/reject`;

        try {
            // Show loading
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: action === 'approve' ? 'Approving...' : 'Rejecting...',
                    allowOutsideClick: false,
                    didOpen: () => Swal.showLoading()
                });
            }

            const res = await fetch(endpoint, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ remarks })
            });

            const data = await res.json();

            if (!res.ok) {
                throw new Error(data.message || `Failed to ${action}`);
            }

            // Success notification
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'success',
                    title: action === 'approve' ? 'Approved!' : 'Rejected',
                    text: data.message || `Item has been ${action}ed successfully.`,
                    timer: 2000,
                    showConfirmButton: false
                });
            } else {
                alert(data.message || `Item ${action}ed successfully.`);
            }

            // Refresh the workspace lists
            refreshWorkspace();

        } catch (err) {
            console.error(`Failed to ${action}:`, err);
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: err.message || `Failed to ${action}. Please try again.`
                });
            } else {
                alert(err.message || `Failed to ${action}. Please try again.`);
            }
        }
    }

    function refreshWorkspace() {
        loadWorkspaceSummary();
        loadWorkspaceApprovals();
        loadWorkspaceTasks();
    }

    function esc(str) {
        if (!str) return '';
        const d = document.createElement('div');
        d.textContent = str;
        return d.innerHTML;
    }

    return {
        init,
        loadStats,
        showSection,
        newRequest,
        newComplaint,
        newFeedback,
        // Workspace
        loadWorkspaceSummary,
        loadWorkspaceApprovals,
        loadWorkspaceTasks,
        refreshWorkspace
    };
})();
