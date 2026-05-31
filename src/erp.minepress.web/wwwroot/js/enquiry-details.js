// ===== MinePress Enquiry Details — JS =====

const EnquiryDetails = {
    _data: null,
    _api: '/api/enquiry',

    async load(id) {
        try {
            const data = await $.get(`${this._api}/detail/${id}`);
            this._data = data;
            this.render(data);
            $('#detailsLoader').hide();
            $('#detailsContent').show();
        } catch (err) {
            $('#detailsLoader').hide();
            $('#detailsError').show();
            $('#errorMessage').text(err.responseJSON?.message || 'Failed to load enquiry details.');
        }
    },

    render(d) {
        // ── Header ──
        $('#hdEnquiryNo').text(d.enquiryNo);
        this.renderStatusBadge('#hdStatus', d.status);
        this.renderPriorityBadge('#hdPriority', d.priority);
        $('#hdDate').text(d.enquiryDate);
        $('#hdCreatedBy').text(d.createdByName || 'System');

        // ── Customer ──
        const initials = (d.customerName || '??').substring(0, 2).toUpperCase();
        $('#custAvatar').text(initials);
        $('#custName').text(d.customerName);
        $('#custCode').text(d.customerCode || '');
        $('#custGst').text(d.customerGst || '-');
        $('#custEmail').text(d.customerEmail || '-');

        // ── Contact ──
        $('#contactName').text(d.contactPerson || '-');
        $('#contactMobile').text(d.contactMobile || '-');
        $('#contactEmail').text(d.contactEmail || '-');
        $('#enqSource').text(d.enquirySource || '-');

        // ── Info ──
        $('#enqDelivery').text(d.expectedDeliveryDate || 'Not set');
        $('#enqPriority').text(d.priority || '-');
        $('#enqItemCount').text(d.items ? d.items.length : 0);
        if (d.remarks) {
            $('#enqRemarks').text(d.remarks);
        }

        // ── Summary ──
        const items = d.items || [];
        const totalQty = items.reduce((s, i) => s + (i.quantity || 0), 0);
        const estValue = items.reduce((s, i) => s + (i.rateCalc?.netTotal || 0), 0);

        $('#sumItemCount').text(items.length);
        $('#sumTotalQty').text(totalQty.toLocaleString('en-IN'));
        $('#sumEstValue').text(this.fmt(estValue));
        $('#tabItemCount').text(items.length);

        // ── Timeline ──
        this.renderTimeline(d.timeline || []);

        // ── Render items & costing & BOM ──
        this.renderItems(items);
        this.renderCosting(items);
        this.renderBom(items);

        // ── Action buttons ──
        this.setupActions(d);

        // ── Print button ──
        $('#btnPrintEnquiry').attr('href', `/Enquiry/Print?id=${d.enquiryId}`);

        // ── Customer Activities (lazy-load on tab click) ──
        this._activitiesLoaded = false;
        $('#tab-activities').on('shown.bs.tab', () => {
            if (!this._activitiesLoaded && d.partyId) {
                this._activitiesLoaded = true;
                CustomerActivity.load(d.partyId, {
                    container: '#activitiesContainer',
                    currentModule: 'ENQUIRY',
                    currentId: d.enquiryId
                });
            }
        });
    },

    renderStatusBadge(selector, status) {
        const cls = {
            'DRAFT': 'enq-status-draft',
            'SUBMITTED': 'enq-status-submitted',
            'CONVERTED': 'enq-status-converted',
            'CLOSED': 'enq-status-closed',
            'CANCELLED': 'enq-status-cancelled'
        }[status] || 'bg-secondary-lt';
        $(selector).attr('class', 'enq-status-badge enq-status-pill ' + cls).text(status || 'N/A');
    },

    renderPriorityBadge(selector, priority) {
        const map = {
            'LOW': { cls: 'bg-secondary', icon: '' },
            'NORMAL': { cls: 'bg-primary', icon: '' },
            'HIGH': { cls: 'bg-warning', icon: 'bi-exclamation-triangle-fill' },
            'URGENT': { cls: 'bg-danger', icon: 'bi-exclamation-circle-fill' }
        };
        const p = map[priority] || { cls: 'bg-secondary', icon: '' };
        const iconHtml = p.icon ? `<i class="${p.icon} me-1"></i>` : '';
        $(selector).attr('class', 'badge ' + p.cls).html(iconHtml + (priority || 'N/A'));
    },

    renderItems(items) {
        const container = $('#itemsContainer');
        if (items.length === 0) {
            container.html(`<div class="text-center py-4 text-muted">
                <i class="bi bi-inbox" style="font-size:2rem;opacity:.3;"></i>
                <div class="mt-2">No items in this enquiry.</div>
            </div>`);
            return;
        }

        let html = '';
        items.forEach((item, idx) => {
            const rc = item.rateCalc;
            const hasCalc = !!rc;
            const netTotal = hasCalc ? this.fmt(rc.netTotal) : '-';
            const costPerUnit = hasCalc ? this.fmt(rc.costPerUnit) : '-';
            const grandTotal = hasCalc ? this.fmt(rc.grandTotal) : '-';
            const taxAmt = hasCalc ? this.fmt(rc.taxAmount) : '-';

            html += `
            <div class="card enq-item-detail-card mb-3">
                <div class="card-body">
                    <div class="row align-items-start">
                        <div class="col-auto">
                            <span class="avatar avatar-md rounded bg-primary-lt fw-bold">#${item.itemSequence || idx + 1}</span>
                        </div>
                        <div class="col">
                            <div class="d-flex justify-content-between align-items-start flex-wrap">
                                <div>
                                    <h3 class="mb-1">${this.esc(item.productName)}</h3>
                                    ${item.productDescription ? `<p class="text-muted small mb-2">${this.esc(item.productDescription)}</p>` : ''}
                                    <div class="d-flex gap-2 flex-wrap mb-2">
                                        ${item.jobTypeName ? `<span class="badge bg-blue-lt"><i class="bi bi-printer me-1"></i>${this.esc(item.jobTypeName)}</span>` : ''}
                                        ${item.productTypeName ? `<span class="badge bg-purple-lt"><i class="bi bi-box me-1"></i>${this.esc(item.productTypeName)}</span>` : ''}
                                        ${item.productSizeName ? `<span class="badge bg-teal-lt"><i class="bi bi-rulers me-1"></i>${this.esc(item.productSizeName)}</span>` : ''}
                                        ${item.printingMethod ? `<span class="badge bg-orange-lt"><i class="bi bi-palette me-1"></i>${this.esc(item.printingMethod)}</span>` : ''}
                                    </div>
                                </div>
                                <div class="text-end">
                                    <div class="fs-3 fw-bold text-success">${netTotal}</div>
                                    <div class="text-muted small">Net Total</div>
                                </div>
                            </div>

                            <div class="row g-3 mt-1">
                                <div class="col-6 col-md-2">
                                    <div class="enq-detail-metric">
                                        <div class="enq-detail-metric-label">Quantity</div>
                                        <div class="enq-detail-metric-value">${(item.quantity || 0).toLocaleString('en-IN')}</div>
                                    </div>
                                </div>
                                ${item.noOfPages ? `<div class="col-6 col-md-2">
                                    <div class="enq-detail-metric">
                                        <div class="enq-detail-metric-label">Pages</div>
                                        <div class="enq-detail-metric-value">${item.noOfPages}</div>
                                    </div>
                                </div>` : ''}
                                ${(item.trimWidthMm || item.trimHeightMm) ? `<div class="col-6 col-md-2">
                                    <div class="enq-detail-metric">
                                        <div class="enq-detail-metric-label">Trim Size</div>
                                        <div class="enq-detail-metric-value">${item.trimWidthMm}×${item.trimHeightMm}mm</div>
                                    </div>
                                </div>` : ''}
                                <div class="col-6 col-md-2">
                                    <div class="enq-detail-metric">
                                        <div class="enq-detail-metric-label">Subtotal</div>
                                        <div class="enq-detail-metric-value">${grandTotal}</div>
                                    </div>
                                </div>
                                <div class="col-6 col-md-2">
                                    <div class="enq-detail-metric">
                                        <div class="enq-detail-metric-label">Tax</div>
                                        <div class="enq-detail-metric-value">${taxAmt}</div>
                                    </div>
                                </div>
                                <div class="col-6 col-md-2">
                                    <div class="enq-detail-metric">
                                        <div class="enq-detail-metric-label">Cost/Unit</div>
                                        <div class="enq-detail-metric-value">${costPerUnit}</div>
                                    </div>
                                </div>
                            </div>

                            ${item.calcRefNo ? `<div class="mt-2 small"><a href="/RateCalculator/Details?id=${item.rateCalc?.rateCalcId || ''}" class="badge bg-cyan-lt text-decoration-none"><i class="bi bi-link-45deg me-1"></i>${this.esc(item.calcRefNo)}</a></div>` : ''}
                        </div>
                    </div>
                </div>
            </div>`;
        });

        // Grand totals row
        const totals = items.reduce((acc, i) => {
            const rc = i.rateCalc;
            if (rc) {
                acc.grand += rc.grandTotal || 0;
                acc.tax += rc.taxAmount || 0;
                acc.net += rc.netTotal || 0;
            }
            return acc;
        }, { grand: 0, tax: 0, net: 0 });

        html += `
        <div class="card bg-primary-lt border-0">
            <div class="card-body py-3">
                <div class="row text-center">
                    <div class="col-md-4">
                        <div class="small text-muted fw-semibold">SUBTOTAL</div>
                        <div class="fs-3 fw-bold">${this.fmt(totals.grand)}</div>
                    </div>
                    <div class="col-md-4">
                        <div class="small text-muted fw-semibold">TAX</div>
                        <div class="fs-3 fw-bold">${this.fmt(totals.tax)}</div>
                    </div>
                    <div class="col-md-4">
                        <div class="small text-muted fw-semibold">NET TOTAL</div>
                        <div class="fs-2 fw-bold text-primary">${this.fmt(totals.net)}</div>
                    </div>
                </div>
            </div>
        </div>`;

        container.html(html);
    },

    renderCosting(items) {
        const container = $('#costingContainer');
        const itemsWithCosting = items.filter(i => i.rateCalc?.costBreakdown);

        if (itemsWithCosting.length === 0) {
            container.html(`<div class="text-center py-4 text-muted">
                <i class="bi bi-calculator" style="font-size:2rem;opacity:.3;"></i>
                <div class="mt-2">No costing data available. Items need rate calculations.</div>
            </div>`);
            return;
        }

        let html = '';
        itemsWithCosting.forEach(item => {
            let breakdown = [];
            try { breakdown = JSON.parse(item.rateCalc.costBreakdown || '[]'); } catch { }

            if (breakdown.length === 0) return;

            html += `
            <div class="card mb-3">
                <div class="card-header">
                    <h3 class="card-title"><i class="bi bi-calculator me-2 text-orange"></i>${this.esc(item.productName)} — Cost Breakdown</h3>
                    <div class="card-actions">
                        <span class="badge bg-success-lt fs-5">${this.fmt(item.rateCalc.netTotal)}</span>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table table-vcenter card-table table-striped">
                        <thead>
                            <tr>
                                <th>Component</th>
                                <th>Category</th>
                                <th>Detail</th>
                                <th class="text-end">Amount</th>
                            </tr>
                        </thead>
                        <tbody>`;

            breakdown.forEach(b => {
                html += `<tr>
                    <td><i class="${this.esc(b.icon || 'bi bi-dot')} me-1 text-muted"></i>${this.esc(b.name || b.component || '')}</td>
                    <td><span class="badge bg-secondary-lt">${this.esc(b.category || '')}</span></td>
                    <td class="text-muted small">${this.esc(b.detail || '')}</td>
                    <td class="text-end fw-semibold">${this.fmt(b.amount || 0)}</td>
                </tr>`;
            });

            html += `</tbody></table></div></div>`;
        });

        container.html(html || container.html());
    },

    renderBom(items) {
        const container = $('#bomContainer');
        const itemsWithBom = items.filter(i => i.rateCalc?.bomData);

        if (itemsWithBom.length === 0) {
            container.html(`<div class="text-center py-4 text-muted">
                <i class="bi bi-box-seam" style="font-size:2rem;opacity:.3;"></i>
                <div class="mt-2">No BOM data available. Items need rate calculations.</div>
            </div>`);
            return;
        }

        let html = '';
        itemsWithBom.forEach(item => {
            let bom = [];
            try { bom = JSON.parse(item.rateCalc.bomData || '[]'); } catch { }

            if (bom.length === 0) return;

            html += `
            <div class="card mb-3">
                <div class="card-header">
                    <h3 class="card-title"><i class="bi bi-box-seam me-2 text-teal"></i>${this.esc(item.productName)} — Bill of Materials</h3>
                </div>
                <div class="table-responsive">
                    <table class="table table-vcenter card-table table-striped">
                        <thead>
                            <tr>
                                <th>Category</th>
                                <th>Material</th>
                                <th>Specification</th>
                                <th>For Part</th>
                                <th class="text-end">Qty</th>
                                <th>Unit</th>
                                <th class="text-end">Rate</th>
                                <th class="text-end">Amount</th>
                            </tr>
                        </thead>
                        <tbody>`;

            bom.forEach(b => {
                html += `<tr>
                    <td><span class="badge bg-azure-lt">${this.esc(b.category || '')}</span></td>
                    <td class="fw-semibold">${this.esc(b.material_name || b.materialName || '')}</td>
                    <td class="text-muted small">${this.esc(b.specification || '')}</td>
                    <td class="small">${this.esc(b.for_part || b.forPart || '')}</td>
                    <td class="text-end">${b.quantity || 0}</td>
                    <td class="small">${this.esc(b.unit || '')}</td>
                    <td class="text-end">${this.fmt(b.rate || 0)}</td>
                    <td class="text-end fw-semibold">${this.fmt(b.amount || 0)}</td>
                </tr>`;
            });

            html += `</tbody></table></div></div>`;
        });

        container.html(html || container.html());
    },

    renderTimeline(timeline) {
        const container = $('#timelineContainer');

        if (!timeline || timeline.length === 0) {
            container.html(`<div class="text-center py-4 text-muted">
                <i class="bi bi-clock-history" style="font-size:2rem;opacity:.3;"></i>
                <div class="mt-2">No timeline events yet.</div>
            </div>`);
            return;
        }

        const eventConfig = {
            'CREATED':          { icon: 'bi-plus-circle-fill',        bg: 'bg-success-lt',  color: 'text-success' },
            'STATUS_CHANGED':   { icon: 'bi-arrow-repeat',            bg: 'bg-info-lt',     color: 'text-info' },
            'SUBMITTED':        { icon: 'bi-send-fill',               bg: 'bg-primary-lt',  color: 'text-primary' },
            'ASSIGNED':         { icon: 'bi-person-plus-fill',        bg: 'bg-azure-lt',    color: 'text-azure' },
            'FOLLOWUP':         { icon: 'bi-telephone-fill',          bg: 'bg-warning-lt',  color: 'text-warning' },
            'QUOTATION_SENT':   { icon: 'bi-file-earmark-arrow-up',   bg: 'bg-teal-lt',     color: 'text-teal' },
            'APPROVED':         { icon: 'bi-check-circle-fill',       bg: 'bg-success-lt',  color: 'text-success' },
            'REJECTED':         { icon: 'bi-x-circle-fill',           bg: 'bg-danger-lt',   color: 'text-danger' },
            'CLOSED':           { icon: 'bi-lock-fill',               bg: 'bg-secondary-lt', color: 'text-secondary' },
            'CANCELLED':        { icon: 'bi-slash-circle',            bg: 'bg-danger-lt',   color: 'text-danger' },
            'UPDATED':          { icon: 'bi-pencil-fill',             bg: 'bg-blue-lt',     color: 'text-blue' },
            'NOTIFICATION_SENT':{ icon: 'bi-bell-fill',               bg: 'bg-teal-lt',     color: 'text-teal' },
            'NOTIFICATION_FAILED':{ icon: 'bi-bell-slash-fill',       bg: 'bg-danger-lt',   color: 'text-danger' },
        };
        const defaultCfg = { icon: 'bi-circle-fill', bg: 'bg-secondary-lt', color: 'text-secondary' };

        let html = '<ul class="timeline">';
        timeline.forEach(t => {
            const cfg = eventConfig[t.eventType] || eventConfig[t.eventCode] || defaultCfg;

            // Status change badges
            let statusHtml = '';
            if (t.oldStatus || t.newStatus) {
                statusHtml = `<div class="mt-2 d-flex align-items-center gap-2 flex-wrap">`;
                if (t.oldStatus) statusHtml += `<span class="badge bg-secondary-lt">${this.esc(t.oldStatus)}</span>`;
                if (t.oldStatus && t.newStatus) statusHtml += `<i class="bi bi-arrow-right small text-muted"></i>`;
                if (t.newStatus) statusHtml += `<span class="badge ${this._timelineStatusClass(t.newStatus)}">${this.esc(t.newStatus)}</span>`;
                statusHtml += `</div>`;
            }

            // Follow-up info
            let followupHtml = '';
            if (t.followupDate) {
                const fDate = new Date(t.followupDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
                followupHtml = `<div class="mt-2 small">
                    <i class="bi bi-calendar-event me-1 text-primary"></i>Follow-up: <strong>${fDate}</strong>
                    ${t.followupMode ? ` via <span class="badge bg-primary-lt">${this.esc(t.followupMode)}</span>` : ''}
                </div>`;
            }

            // Attachment
            let attachHtml = '';
            if (t.attachmentUrl) {
                attachHtml = `<div class="mt-2 small">
                    <a href="${this.esc(t.attachmentUrl)}" target="_blank" class="text-decoration-none">
                        <i class="bi bi-paperclip me-1"></i>Attachment
                    </a>
                </div>`;
            }

            // Remarks
            let remarksHtml = '';
            if (t.remarks) {
                remarksHtml = `<div class="mt-2 small text-muted fst-italic"><i class="bi bi-chat-left-text me-1"></i>${this.esc(t.remarks)}</div>`;
            }

            html += `
            <li class="timeline-event">
                <div class="timeline-event-icon ${cfg.bg}">
                    <i class="bi ${cfg.icon} ${cfg.color}"></i>
                </div>
                <div class="card timeline-event-card">
                    <div class="card-body p-3">
                        <div class="d-flex justify-content-between align-items-start">
                            <div class="fw-semibold ${cfg.color}">${this.esc(t.eventTitle || t.eventType)}</div>
                            <div class="text-muted small text-nowrap ms-3">${this.esc(t.createdOn)}</div>
                        </div>
                        ${t.eventDescription ? `<div class="mt-1 small text-muted">${this.esc(t.eventDescription)}</div>` : ''}
                        ${statusHtml}
                        ${followupHtml}
                        ${attachHtml}
                        ${remarksHtml}
                    </div>
                </div>
            </li>`;
        });
        html += '</ul>';

        container.html(html);
    },

    _timelineStatusClass(status) {
        return {
            'DRAFT': 'bg-warning-lt', 'SUBMITTED': 'bg-info-lt', 'CONVERTED': 'bg-success-lt',
            'APPROVED': 'bg-success-lt', 'REJECTED': 'bg-danger-lt', 'CLOSED': 'bg-secondary-lt',
            'CANCELLED': 'bg-danger-lt', 'IN_PROGRESS': 'bg-primary-lt', 'COMPLETED': 'bg-success-lt'
        }[status] || 'bg-secondary-lt';
    },

    setupActions(d) {
        const isDraft = d.status === 'DRAFT';
        const isClosed = d.status === 'CLOSED' || d.status === 'CANCELLED';

        if (!isDraft) {
            $('#btnEditEnquiry').addClass('disabled').attr('aria-disabled', 'true');
        } else {
            $('#btnEditEnquiry').attr('href', `/Enquiry/Create?editId=${d.enquiryId}`);
        }

        if (isClosed) {
            $('#btnConvertQuotation, #btnSubmitEnquiry, #btnCancelEnquiry, #btnCloseEnquiry')
                .addClass('disabled').attr('aria-disabled', 'true');
        }

        $('#btnSubmitEnquiry').on('click', () => this.changeStatus(d.enquiryId, 'SUBMITTED', d.enquiryNo));
        $('#btnCancelEnquiry').on('click', () => this.changeStatus(d.enquiryId, 'CANCELLED', d.enquiryNo));
        $('#btnCloseEnquiry').on('click', () => this.changeStatus(d.enquiryId, 'CLOSED', d.enquiryNo));

        $('#btnConvertQuotation').on('click', () => {
            window.location.href = `/Quotation/Create?fromEnquiryId=${d.enquiryId}`;
        });

        $('#btnSendNotification').on('click', () => {
            Swal2.info('Notification feature coming soon.');
        });
    },

    async changeStatus(id, status, enquiryNo) {
        const confirmed = await Swal2.confirmStatus(enquiryNo || `ENQ-${id}`, status);
        if (!confirmed) return;

        try {
            await $.ajax({
                url: `${this._api}/updatestatus`,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ enquiryId: id, status })
            });
            Swal2.success(`Status updated to ${status}.`);
            setTimeout(() => this.load(id), 500);
        } catch (err) {
            Swal2.error(err.responseJSON?.message || 'Failed to update status.');
        }
    },

    // ── Helpers ──
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
    },

    showAlert(message, type) {
        const iconMap = { success: 'success', danger: 'error', warning: 'warning', info: 'info' };
        Swal2.toast(message, iconMap[type] || 'info');
    }
};
