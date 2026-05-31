// ===== MinePress Customer Activity Dashboard — Shared Module =====
// Usage: CustomerActivity.load(partyId, { container, currentModule, currentId })

const CustomerActivity = {
    _api: '/api/enquiry/customer-activities',

    async load(partyId, options = {}) {
        const container = $(options.container || '#customerActivityContainer');
        const currentModule = options.currentModule || null; // 'ENQUIRY', 'QUOTATION', etc.
        const currentId = options.currentId || null;

        if (!partyId) {
            container.html(this._emptyState('No customer linked to this record.'));
            return;
        }

        container.html(this._loader());

        try {
            const data = await $.get(`${this._api}/${partyId}`);
            this.render(container, data, currentModule, currentId);
        } catch {
            container.html(`<div class="text-center py-4 text-muted">
                <i class="bi bi-exclamation-circle" style="font-size:2rem;opacity:.3;"></i>
                <div class="mt-2">Failed to load customer activities.</div>
            </div>`);
        }
    },

    render(container, data, currentModule, currentId) {
        const s = data.summary || {};
        const customer = data.customer || {};
        let html = '';

        // ── Customer Profile Header ──
        html += this._renderProfileHeader(customer, s);

        // ── KPI Summary Row ──
        html += this._renderKpiCards(s);

        // ── Financial Overview ──
        html += this._renderFinancialOverview(s);

        // ── Activity Sections ──
        html += '<div class="row g-3 mt-1">';

        // Enquiries
        html += '<div class="col-lg-6">';
        html += this._renderActivityCard(
            'Enquiries', 'bi-clipboard-data', 'primary', data.enquiries || [], s.totalEnquiries || 0,
            ['#', 'Date', 'Status', 'Priority', 'Items'],
            (e) => {
                const isCurrent = currentModule === 'ENQUIRY' && e.enquiryId === currentId;
                const badge = isCurrent ? ' <span class="badge bg-primary-lt ms-1">Current</span>' : '';
                return `<tr class="${isCurrent ? 'ca-highlight-row' : ''}">
                    <td><a href="/Enquiry/Details?id=${e.enquiryId}" class="fw-semibold text-decoration-none">${this.esc(e.enquiryNo)}${badge}</a></td>
                    <td class="small">${this.esc(e.date)}</td>
                    <td>${this._statusBadge(e.status)}</td>
                    <td>${this._priorityBadge(e.priority)}</td>
                    <td class="text-center"><span class="badge bg-primary-lt">${e.itemCount || 0}</span></td>
                </tr>`;
            }
        );
        html += '</div>';

        // Quotations
        html += '<div class="col-lg-6">';
        html += this._renderActivityCard(
            'Quotations', 'bi-file-earmark-text', 'success', data.quotations || [], s.totalQuotations || 0,
            ['#', 'Date', 'Status', 'Net Amount'],
            (q) => {
                const isCurrent = currentModule === 'QUOTATION' && q.quotationId === currentId;
                const badge = isCurrent ? ' <span class="badge bg-primary-lt ms-1">Current</span>' : '';
                return `<tr class="${isCurrent ? 'ca-highlight-row' : ''}">
                    <td><a href="/Quotation/Details?id=${q.quotationId}" class="fw-semibold text-decoration-none">${this.esc(q.quotationNo)}${badge}</a></td>
                    <td class="small">${this.esc(q.date)}</td>
                    <td>${this._statusBadge(q.status)}</td>
                    <td class="text-end fw-semibold">${this.fmt(q.netAmount)}</td>
                </tr>`;
            }
        );
        html += '</div>';

        // Jobs
        html += '<div class="col-lg-6">';
        html += this._renderActivityCard(
            'Jobs', 'bi-briefcase', 'azure', data.jobs || [], s.totalJobs || 0,
            ['#', 'Date', 'Product', 'Status', 'Progress'],
            (j) => `<tr>
                <td class="fw-semibold">${this.esc(j.jobNo)}</td>
                <td class="small">${this.esc(j.date)}</td>
                <td class="small">${this.esc(j.productName || '-')}</td>
                <td>${this._statusBadge(j.statusCode)}</td>
                <td>
                    <div class="d-flex align-items-center gap-2">
                        <div class="progress flex-fill" style="height:6px;min-width:50px;">
                            <div class="progress-bar bg-azure" style="width:${j.progressPercent || 0}%"></div>
                        </div>
                        <small class="text-muted">${j.progressPercent || 0}%</small>
                    </div>
                </td>
            </tr>`
        );
        html += '</div>';

        // Invoices
        html += '<div class="col-lg-6">';
        html += this._renderActivityCard(
            'Invoices', 'bi-receipt', 'orange', data.invoices || [], s.totalInvoices || 0,
            ['#', 'Date', 'Status', 'Grand Total', 'Balance'],
            (i) => `<tr>
                <td class="fw-semibold">${this.esc(i.invoiceNo)}</td>
                <td class="small">${this.esc(i.date)}</td>
                <td>${this._statusBadge(i.status)}</td>
                <td class="text-end">${this.fmt(i.grandTotal)}</td>
                <td class="text-end fw-semibold ${(i.balanceAmount || 0) > 0 ? 'text-danger' : 'text-success'}">${this.fmt(i.balanceAmount)}</td>
            </tr>`
        );
        html += '</div>';

        // Receipts
        html += '<div class="col-lg-6">';
        html += this._renderActivityCard(
            'Receipts', 'bi-cash-stack', 'teal', data.receipts || [], s.totalReceipts || 0,
            ['#', 'Date', 'Mode', 'Amount', 'Status'],
            (r) => `<tr>
                <td class="fw-semibold">${this.esc(r.receiptNo)}</td>
                <td class="small">${this.esc(r.date)}</td>
                <td><span class="badge bg-secondary-lt">${this.esc(r.paymentMode)}</span></td>
                <td class="text-end fw-semibold text-success">${this.fmt(r.amount)}</td>
                <td>${this._statusBadge(r.status)}</td>
            </tr>`
        );
        html += '</div>';

        // Payments
        html += '<div class="col-lg-6">';
        html += this._renderActivityCard(
            'Payments', 'bi-credit-card', 'purple', data.payments || [], s.totalPayments || 0,
            ['#', 'Date', 'Mode', 'Amount', 'Status'],
            (p) => `<tr>
                <td class="fw-semibold">${this.esc(p.paymentNo)}</td>
                <td class="small">${this.esc(p.date)}</td>
                <td><span class="badge bg-secondary-lt">${this.esc(p.paymentMode)}</span></td>
                <td class="text-end fw-semibold">${this.fmt(p.amount)}</td>
                <td>${this._statusBadge(p.status)}</td>
            </tr>`
        );
        html += '</div>';

        html += '</div>'; // close row

        // ── AI Insights Panel ──
        html += this._renderAiInsights(s);

        container.html(html);
    },

    // ═══ Sub-renderers ═══

    _renderProfileHeader(c, s) {
        const initials = (c.name || '??').substring(0, 2).toUpperCase();
        const sinceDate = c.createdOn || '';
        return `
        <div class="card ca-profile-card mb-3">
            <div class="card-body">
                <div class="d-flex align-items-center gap-3 flex-wrap">
                    <span class="avatar avatar-xl rounded-circle ca-avatar-gradient fw-bold fs-2">${this.esc(initials)}</span>
                    <div class="flex-fill">
                        <h3 class="mb-0 fw-bold">${this.esc(c.name || 'Customer')}</h3>
                        <div class="text-muted small">
                            ${c.code ? `<span class="me-3"><i class="bi bi-hash me-1"></i>${this.esc(c.code)}</span>` : ''}
                            ${c.gstNo ? `<span class="me-3"><i class="bi bi-receipt me-1"></i>${this.esc(c.gstNo)}</span>` : ''}
                            ${c.email ? `<span class="me-3"><i class="bi bi-envelope me-1"></i>${this.esc(c.email)}</span>` : ''}
                            ${c.mobile ? `<span class="me-3"><i class="bi bi-phone me-1"></i>${this.esc(c.mobile)}</span>` : ''}
                        </div>
                        ${sinceDate ? `<div class="text-muted small mt-1"><i class="bi bi-calendar-check me-1"></i>Customer since ${this.esc(sinceDate)}</div>` : ''}
                    </div>
                    <div class="text-end">
                        <div class="ca-ai-badge">
                            <i class="bi bi-stars me-1"></i>AI Customer 360°
                        </div>
                    </div>
                </div>
            </div>
        </div>`;
    },

    _renderKpiCards(s) {
        const kpis = [
            { label: 'Enquiries', value: s.totalEnquiries || 0, icon: 'bi-clipboard-data', bg: 'primary' },
            { label: 'Quotations', value: s.totalQuotations || 0, icon: 'bi-file-earmark-text', bg: 'success' },
            { label: 'Jobs', value: s.totalJobs || 0, icon: 'bi-briefcase', bg: 'azure' },
            { label: 'Invoices', value: s.totalInvoices || 0, icon: 'bi-receipt', bg: 'orange' },
            { label: 'Receipts', value: s.totalReceipts || 0, icon: 'bi-cash-stack', bg: 'teal' },
            { label: 'Payments', value: s.totalPayments || 0, icon: 'bi-credit-card', bg: 'purple' },
        ];

        let html = '<div class="row g-2 mb-3">';
        kpis.forEach(k => {
            html += `
            <div class="col-6 col-md-4 col-lg-2">
                <div class="card ca-kpi-card border-0">
                    <div class="card-body py-2 px-3 text-center">
                        <div class="ca-kpi-icon bg-${k.bg}-lt text-${k.bg} mb-1">
                            <i class="bi ${k.icon}"></i>
                        </div>
                        <div class="fs-2 fw-bold text-${k.bg}">${k.value}</div>
                        <div class="text-muted small text-uppercase fw-semibold">${k.label}</div>
                    </div>
                </div>
            </div>`;
        });
        html += '</div>';
        return html;
    },

    _renderFinancialOverview(s) {
        const totalInvoiced = s.totalInvoicedAmount || 0;
        const totalReceived = s.totalReceiptAmount || 0;
        const totalPaid = s.totalPaymentAmount || 0;
        const outstanding = s.totalOutstanding || 0;
        const quotedValue = s.totalQuotedAmount || 0;

        return `
        <div class="card ca-finance-card mb-3">
            <div class="card-header">
                <h3 class="card-title"><i class="bi bi-graph-up-arrow me-2 text-success"></i>Financial Overview</h3>
                <div class="card-actions">
                    <span class="ca-ai-badge-sm"><i class="bi bi-stars me-1"></i>AI Summary</span>
                </div>
            </div>
            <div class="card-body py-3">
                <div class="row g-3 text-center">
                    <div class="col-6 col-md">
                        <div class="text-muted small fw-semibold text-uppercase">Quoted Value</div>
                        <div class="fs-3 fw-bold text-primary">${this.fmt(quotedValue)}</div>
                    </div>
                    <div class="col-6 col-md">
                        <div class="text-muted small fw-semibold text-uppercase">Total Invoiced</div>
                        <div class="fs-3 fw-bold text-orange">${this.fmt(totalInvoiced)}</div>
                    </div>
                    <div class="col-6 col-md">
                        <div class="text-muted small fw-semibold text-uppercase">Received</div>
                        <div class="fs-3 fw-bold text-success">${this.fmt(totalReceived)}</div>
                    </div>
                    <div class="col-6 col-md">
                        <div class="text-muted small fw-semibold text-uppercase">Outstanding</div>
                        <div class="fs-3 fw-bold ${outstanding > 0 ? 'text-danger' : 'text-success'}">${this.fmt(outstanding)}</div>
                    </div>
                    <div class="col-6 col-md">
                        <div class="text-muted small fw-semibold text-uppercase">Payments Made</div>
                        <div class="fs-3 fw-bold text-purple">${this.fmt(totalPaid)}</div>
                    </div>
                </div>
            </div>
        </div>`;
    },

    _renderActivityCard(title, icon, color, items, totalCount, headers, rowFn) {
        let html = `
        <div class="card ca-activity-card mb-0 h-100">
            <div class="card-header py-2">
                <div class="d-flex align-items-center gap-2">
                    <span class="avatar avatar-xs bg-${color}-lt text-${color}"><i class="bi ${icon}"></i></span>
                    <h3 class="card-title mb-0 fs-5">${title}</h3>
                </div>
                <div class="card-actions">
                    <span class="badge bg-${color}-lt text-${color}">${totalCount} total</span>
                </div>
            </div>`;

        if (items.length === 0) {
            html += `<div class="card-body text-center py-3 text-muted">
                <i class="bi bi-inbox" style="font-size:1.5rem;opacity:.3;"></i>
                <div class="mt-1 small">No records found.</div>
            </div></div>`;
            return html;
        }

        html += `<div class="table-responsive">
            <table class="table table-vcenter card-table table-hover ca-compact-table mb-0">
                <thead><tr>${headers.map(h => `<th class="small text-uppercase text-muted">${h}</th>`).join('')}</tr></thead>
                <tbody>${items.map(rowFn).join('')}</tbody>
            </table>
        </div>`;

        if (totalCount > items.length) {
            html += `<div class="card-footer py-2 text-center">
                <span class="text-muted small">Showing ${items.length} of ${totalCount}</span>
            </div>`;
        }

        html += '</div>';
        return html;
    },

    _renderAiInsights(s) {
        const insights = [];

        // Generate insights based on data
        const conversionRate = s.totalEnquiries > 0
            ? Math.round((s.totalQuotations / s.totalEnquiries) * 100) : 0;

        if (s.totalEnquiries > 0) {
            insights.push({
                icon: 'bi-graph-up',
                color: 'primary',
                text: `Enquiry-to-Quotation conversion rate: <strong>${conversionRate}%</strong>`,
            });
        }

        if ((s.totalOutstanding || 0) > 0) {
            insights.push({
                icon: 'bi-exclamation-triangle',
                color: 'warning',
                text: `Outstanding balance of <strong>${this.fmt(s.totalOutstanding)}</strong> pending collection.`,
            });
        } else if (s.totalInvoicedAmount > 0) {
            insights.push({
                icon: 'bi-check-circle',
                color: 'success',
                text: 'All invoiced amounts have been collected. Excellent payment record!',
            });
        }

        if (s.totalJobs > 0 && s.totalInvoices === 0) {
            insights.push({
                icon: 'bi-info-circle',
                color: 'info',
                text: 'Jobs completed but no invoices raised yet. Consider invoicing.',
            });
        }

        if (s.totalQuotations > 0 && s.totalJobs === 0) {
            insights.push({
                icon: 'bi-lightbulb',
                color: 'azure',
                text: 'Quotations sent but no jobs created. Follow up with customer.',
            });
        }

        if (s.avgJobCompletion != null && s.avgJobCompletion > 0) {
            insights.push({
                icon: 'bi-speedometer2',
                color: 'teal',
                text: `Average job completion: <strong>${s.avgJobCompletion}%</strong>`,
            });
        }

        if (insights.length === 0) return '';

        let html = `
        <div class="card ca-ai-insights-card mt-3">
            <div class="card-header">
                <h3 class="card-title"><i class="bi bi-stars me-2 text-warning"></i>AI Insights</h3>
                <div class="card-actions"><span class="ca-ai-badge-sm"><i class="bi bi-cpu me-1"></i>Agentic AI</span></div>
            </div>
            <div class="card-body py-2">
                <div class="row g-2">`;

        insights.forEach(i => {
            html += `
                <div class="col-md-6">
                    <div class="ca-insight-item">
                        <span class="ca-insight-icon text-${i.color}"><i class="bi ${i.icon}"></i></span>
                        <span class="small">${i.text}</span>
                    </div>
                </div>`;
        });

        html += `</div></div></div>`;
        return html;
    },

    // ═══ Helpers ═══

    _loader() {
        return `<div class="text-center py-4 text-muted">
            <div class="spinner-border spinner-border-sm text-primary" role="status"></div>
            <div class="mt-2">Loading customer activity dashboard...</div>
        </div>`;
    },

    _emptyState(msg) {
        return `<div class="text-center py-4 text-muted">
            <i class="bi bi-person-x" style="font-size:2rem;opacity:.3;"></i>
            <div class="mt-2">${msg}</div>
        </div>`;
    },

    _statusBadge(status) {
        const cls = {
            'DRAFT': 'bg-warning-lt', 'SUBMITTED': 'bg-info-lt', 'CONVERTED': 'bg-success-lt',
            'CLOSED': 'bg-secondary-lt', 'CANCELLED': 'bg-danger-lt',
            'APPROVED': 'bg-success-lt', 'REJECTED': 'bg-danger-lt', 'IN_PROGRESS': 'bg-primary-lt',
            'COMPLETED': 'bg-success-lt', 'PENDING': 'bg-warning-lt', 'SENT': 'bg-info-lt',
            'PAID': 'bg-success-lt', 'PARTIAL': 'bg-warning-lt', 'UNPAID': 'bg-danger-lt',
            'CONFIRMED': 'bg-success-lt', 'POSTED': 'bg-teal-lt',
        }[status] || 'bg-secondary-lt';
        return `<span class="badge ${cls}">${this.esc(status || 'N/A')}</span>`;
    },

    _priorityBadge(priority) {
        const cls = {
            'LOW': 'bg-secondary', 'NORMAL': 'bg-primary', 'HIGH': 'bg-warning', 'URGENT': 'bg-danger'
        }[priority] || 'bg-secondary';
        return `<span class="badge ${cls}">${this.esc(priority || '-')}</span>`;
    },

    fmt(n) {
        if (n == null || isNaN(n)) return '₹0.00';
        return '₹' + Number(n).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    },

    esc(value) {
        return (value || '')
            .toString()
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#39;');
    }
};
