// ===== MinePress Vendor Activity Dashboard — Shared Module =====
// Usage: VendorActivity.load(vendorId, { container, currentModule, currentId })

const VendorActivity = {
    _api: '/api/outsource/vendor-activities',

    async load(vendorId, options = {}) {
        const container = $(options.container || '#vendorActivityContainer');
        const currentModule = options.currentModule || null;
        const currentId = options.currentId || null;

        if (!vendorId) {
            container.html(this._emptyState('No vendor linked to this record.'));
            return;
        }

        container.html(this._loader());

        try {
            const data = await $.get(`${this._api}/${vendorId}`);
            this.render(container, data, currentModule, currentId);
        } catch {
            container.html(`<div class="text-center py-4 text-muted">
                <i class="bi bi-exclamation-circle" style="font-size:2rem;opacity:.3;"></i>
                <div class="mt-2">Failed to load vendor activities.</div>
            </div>`);
        }
    },

    render(container, data, currentModule, currentId) {
        const s = data.summary || {};
        const vendor = data.vendor || {};
        let html = '';

        // ── Vendor Profile Header ──
        html += this._renderProfileHeader(vendor, s);

        // ── KPI Summary Row ──
        html += this._renderKpiCards(s);

        // ── Financial Overview ──
        html += this._renderFinancialOverview(s);

        // ── Activity Sections ──
        html += '<div class="row g-3 mt-1">';

        // Outsource Orders
        html += '<div class="col-lg-12">';
        html += this._renderActivityCard(
            'Outsource Orders', 'bi-box-arrow-up-right', 'purple', data.outsourceOrders || [], s.totalOrders || 0,
            ['#', 'Date', 'Job', 'Customer', 'Process', 'Qty', 'Amount', 'Status'],
            (o) => {
                const isCurrent = currentModule === 'OUTSOURCE' && o.outsourceId === currentId;
                const badge = isCurrent ? ' <span class="badge bg-purple-lt ms-1">Current</span>' : '';
                return `<tr class="${isCurrent ? 'va-highlight-row' : ''}">
                    <td><a href="/Outsource/Details?id=${o.outsourceId}" class="fw-semibold text-decoration-none">${this.esc(o.outsourceNo)}${badge}</a></td>
                    <td class="small">${this.esc(o.date)}</td>
                    <td class="small">${this.esc(o.jobNo)}</td>
                    <td class="small">${this.esc(o.customerName)}</td>
                    <td class="small">${this.esc(o.processType || '—')}</td>
                    <td class="text-end">${o.totalQuantity ?? 0}</td>
                    <td class="text-end fw-semibold">${this.fmt(o.totalAmount)}</td>
                    <td>${this._statusBadge(o.status)}</td>
                </tr>`;
            }
        );
        html += '</div>';

        // Dispatches
        html += '<div class="col-lg-6">';
        html += this._renderActivityCard(
            'Dispatches', 'bi-send', 'azure', data.dispatches || [], s.totalDispatches || 0,
            ['Outsource', 'Date', 'Challan', 'Qty', 'Remarks'],
            (d) => `<tr>
                <td class="fw-semibold small">${this.esc(d.outsourceNo)}</td>
                <td class="small">${this.esc(d.dispatchDate)}</td>
                <td class="small">${this.esc(d.challanNo || '—')}</td>
                <td class="text-end">${d.totalQuantity ?? 0}</td>
                <td class="small text-muted">${this.esc(d.remarks || '—')}</td>
            </tr>`
        );
        html += '</div>';

        // Receives
        html += '<div class="col-lg-6">';
        html += this._renderActivityCard(
            'Receives', 'bi-box-arrow-in-down', 'success', data.receives || [], s.totalReceives || 0,
            ['Outsource', 'Date', 'Good Qty', 'Rejected', 'Remarks'],
            (r) => `<tr>
                <td class="fw-semibold small">${this.esc(r.outsourceNo)}</td>
                <td class="small">${this.esc(r.receiveDate)}</td>
                <td class="text-end text-success fw-semibold">${r.receivedQuantity ?? 0}</td>
                <td class="text-end ${(r.rejectedQuantity || 0) > 0 ? 'text-danger fw-semibold' : ''}">${r.rejectedQuantity ?? 0}</td>
                <td class="small text-muted">${this.esc(r.remarks || '—')}</td>
            </tr>`
        );
        html += '</div>';

        // Payments
        html += '<div class="col-lg-6">';
        html += this._renderActivityCard(
            'Payments', 'bi-credit-card', 'teal', data.payments || [], s.totalPayments || 0,
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

        // ── AI Insights ──
        html += this._renderAiInsights(s);

        container.html(html);
    },

    // ═══ Sub-renderers ═══

    _renderProfileHeader(v, s) {
        const initials = (v.name || '??').substring(0, 2).toUpperCase();
        return `
        <div class="card va-profile-card mb-3">
            <div class="card-body">
                <div class="d-flex align-items-center gap-3 flex-wrap">
                    <span class="avatar avatar-xl rounded-circle va-avatar-gradient fw-bold fs-2">${this.esc(initials)}</span>
                    <div class="flex-fill">
                        <h3 class="mb-0 fw-bold">${this.esc(v.name || 'Vendor')}</h3>
                        <div class="text-muted small">
                            ${v.code ? `<span class="me-3"><i class="bi bi-hash me-1"></i>${this.esc(v.code)}</span>` : ''}
                            ${v.vendorType ? `<span class="me-3"><i class="bi bi-tag me-1"></i>${this.esc(v.vendorType)}</span>` : ''}
                            ${v.gstNo ? `<span class="me-3"><i class="bi bi-receipt me-1"></i>${this.esc(v.gstNo)}</span>` : ''}
                        </div>
                        <div class="text-muted small mt-1">
                            ${v.email ? `<span class="me-3"><i class="bi bi-envelope me-1"></i>${this.esc(v.email)}</span>` : ''}
                            ${v.mobile ? `<span class="me-3"><i class="bi bi-phone me-1"></i>${this.esc(v.mobile)}</span>` : ''}
                            ${v.serviceArea ? `<span class="me-3"><i class="bi bi-geo-alt me-1"></i>${this.esc(v.serviceArea)}</span>` : ''}
                        </div>
                        ${v.address ? `<div class="text-muted small mt-1"><i class="bi bi-house me-1"></i>${this.esc(v.address)}</div>` : ''}
                        <div class="text-muted small mt-1">
                            ${v.contractStart ? `<span class="me-3"><i class="bi bi-calendar-check me-1"></i>Contract: ${this.esc(v.contractStart)} — ${this.esc(v.contractEnd || 'Ongoing')}</span>` : ''}
                            ${v.createdOn ? `<span><i class="bi bi-clock-history me-1"></i>Since ${this.esc(v.createdOn)}</span>` : ''}
                        </div>
                    </div>
                    <div class="text-end">
                        <div class="va-ai-badge">
                            <i class="bi bi-stars me-1"></i>Vendor 360°
                        </div>
                    </div>
                </div>
            </div>
        </div>`;
    },

    _renderKpiCards(s) {
        const kpis = [
            { label: 'Total Orders', value: s.totalOrders || 0, icon: 'bi-box-arrow-up-right', bg: 'purple' },
            { label: 'Active', value: s.activeOrders || 0, icon: 'bi-lightning', bg: 'azure' },
            { label: 'Completed', value: s.completedOrders || 0, icon: 'bi-check-circle', bg: 'success' },
            { label: 'Dispatches', value: s.totalDispatches || 0, icon: 'bi-send', bg: 'primary' },
            { label: 'Receives', value: s.totalReceives || 0, icon: 'bi-box-arrow-in-down', bg: 'teal' },
            { label: 'Rework', value: s.reworkOrders || 0, icon: 'bi-arrow-repeat', bg: 'orange' },
        ];

        let html = '<div class="row g-2 mb-3">';
        kpis.forEach(k => {
            html += `
            <div class="col-6 col-md-4 col-lg-2">
                <div class="card va-kpi-card border-0">
                    <div class="card-body py-2 px-3 text-center">
                        <div class="va-kpi-icon bg-${k.bg}-lt text-${k.bg} mb-1">
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
        const totalValue = s.totalOutsourceValue || 0;
        const totalPaid = s.totalPaymentAmount || 0;
        const pending = s.pendingAmount || 0;
        const dispQty = s.totalDispatchedQty || 0;
        const recvQty = s.totalReceivedQty || 0;
        const rejQty = s.totalRejectedQty || 0;

        return `
        <div class="card va-finance-card mb-3">
            <div class="card-header">
                <h3 class="card-title"><i class="bi bi-graph-up-arrow me-2 text-purple"></i>Financial & Quantity Overview</h3>
                <div class="card-actions">
                    <span class="va-ai-badge-sm"><i class="bi bi-stars me-1"></i>AI Summary</span>
                </div>
            </div>
            <div class="card-body py-3">
                <div class="row g-3 text-center">
                    <div class="col-6 col-md">
                        <div class="text-muted small fw-semibold text-uppercase">Total Value</div>
                        <div class="fs-3 fw-bold text-purple">${this.fmt(totalValue)}</div>
                    </div>
                    <div class="col-6 col-md">
                        <div class="text-muted small fw-semibold text-uppercase">Paid</div>
                        <div class="fs-3 fw-bold text-success">${this.fmt(totalPaid)}</div>
                    </div>
                    <div class="col-6 col-md">
                        <div class="text-muted small fw-semibold text-uppercase">Pending</div>
                        <div class="fs-3 fw-bold ${pending > 0 ? 'text-danger' : 'text-success'}">${this.fmt(pending)}</div>
                    </div>
                    <div class="col-6 col-md">
                        <div class="text-muted small fw-semibold text-uppercase">Dispatched Qty</div>
                        <div class="fs-3 fw-bold text-azure">${dispQty.toLocaleString('en-IN')}</div>
                    </div>
                    <div class="col-6 col-md">
                        <div class="text-muted small fw-semibold text-uppercase">Received Qty</div>
                        <div class="fs-3 fw-bold text-teal">${recvQty.toLocaleString('en-IN')}</div>
                    </div>
                    <div class="col-6 col-md">
                        <div class="text-muted small fw-semibold text-uppercase">Rejected Qty</div>
                        <div class="fs-3 fw-bold ${rejQty > 0 ? 'text-danger' : 'text-success'}">${rejQty.toLocaleString('en-IN')}</div>
                    </div>
                </div>
            </div>
        </div>`;
    },

    _renderActivityCard(title, icon, color, items, totalCount, headers, rowFn) {
        let html = `
        <div class="card va-activity-card mb-0 h-100">
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
            <table class="table table-vcenter card-table table-hover va-compact-table mb-0">
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

        if (s.totalOrders > 0 && s.completedOrders > 0) {
            const completionRate = Math.round((s.completedOrders / s.totalOrders) * 100);
            insights.push({
                icon: 'bi-graph-up',
                color: 'purple',
                text: `Order completion rate: <strong>${completionRate}%</strong> (${s.completedOrders} of ${s.totalOrders}).`
            });
        }

        if ((s.totalRejectedQty || 0) > 0 && (s.totalReceivedQty || 0) > 0) {
            const rejectRate = ((s.totalRejectedQty / (s.totalReceivedQty + s.totalRejectedQty)) * 100).toFixed(1);
            insights.push({
                icon: 'bi-exclamation-triangle',
                color: 'warning',
                text: `Rejection rate: <strong>${rejectRate}%</strong>. Total rejected: ${s.totalRejectedQty.toLocaleString('en-IN')} units.`
            });
        } else if ((s.totalReceivedQty || 0) > 0) {
            insights.push({
                icon: 'bi-check-circle',
                color: 'success',
                text: 'Zero rejections recorded. Excellent quality from this vendor!'
            });
        }

        if ((s.pendingAmount || 0) > 0) {
            insights.push({
                icon: 'bi-currency-rupee',
                color: 'orange',
                text: `Pending payment of <strong>${this.fmt(s.pendingAmount)}</strong> to this vendor.`
            });
        } else if ((s.totalPaymentAmount || 0) > 0) {
            insights.push({
                icon: 'bi-check-circle',
                color: 'success',
                text: 'All outsource payments are cleared. No pending dues.'
            });
        }

        if ((s.reworkOrders || 0) > 0) {
            insights.push({
                icon: 'bi-arrow-repeat',
                color: 'orange',
                text: `<strong>${s.reworkOrders}</strong> order(s) currently in rework. Monitor vendor quality.`
            });
        }

        if ((s.delayedOrders || 0) > 0) {
            insights.push({
                icon: 'bi-clock',
                color: 'danger',
                text: `<strong>${s.delayedOrders}</strong> order(s) with delayed returns. Follow up with vendor.`
            });
        }

        if ((s.activeOrders || 0) > 0) {
            insights.push({
                icon: 'bi-lightning',
                color: 'azure',
                text: `<strong>${s.activeOrders}</strong> active outsource order(s) in progress.`
            });
        }

        if (insights.length === 0) return '';

        let html = `
        <div class="card va-ai-insights-card mt-3">
            <div class="card-header">
                <h3 class="card-title"><i class="bi bi-stars me-2 text-warning"></i>AI Insights</h3>
                <div class="card-actions"><span class="va-ai-badge-sm"><i class="bi bi-cpu me-1"></i>Agentic AI</span></div>
            </div>
            <div class="card-body py-2">
                <div class="row g-2">`;

        insights.forEach(i => {
            html += `
                <div class="col-md-6">
                    <div class="va-insight-item">
                        <span class="va-insight-icon text-${i.color}"><i class="bi ${i.icon}"></i></span>
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
            <div class="spinner-border spinner-border-sm text-purple" role="status"></div>
            <div class="mt-2">Loading vendor activity dashboard...</div>
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
            'OUTSOURCE_CREATED': 'bg-warning-lt', 'VENDOR_ASSIGNED': 'bg-info-lt',
            'MATERIAL_SENT': 'bg-azure-lt', 'VENDOR_ACKNOWLEDGED': 'bg-teal-lt',
            'PROCESS_STARTED': 'bg-cyan-lt', 'PROCESS_COMPLETED': 'bg-info-lt',
            'QUALITY_CHECKED': 'bg-success-lt', 'MATERIAL_RECEIVED': 'bg-success-lt',
            'RETURN_DELAYED': 'bg-warning-lt', 'REWORK_REQUIRED': 'bg-orange-lt',
            'REWORK_SENT': 'bg-orange-lt', 'REWORK_COMPLETED': 'bg-teal-lt',
            'PAYMENT_INITIATED': 'bg-yellow-lt', 'PAYMENT_COMPLETED': 'bg-success-lt',
            'OUTSOURCE_CLOSED': 'bg-secondary-lt', 'OUTSOURCE_CANCELLED': 'bg-danger-lt',
            'PAID': 'bg-success-lt', 'PARTIAL': 'bg-warning-lt', 'PENDING': 'bg-warning-lt',
            'POSTED': 'bg-teal-lt', 'CANCELLED': 'bg-danger-lt',
        }[status] || 'bg-secondary-lt';
        return `<span class="badge ${cls}">${this.esc((status || 'N/A').replace(/_/g, ' '))}</span>`;
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
