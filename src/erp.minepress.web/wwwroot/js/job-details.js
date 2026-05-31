// ===== MinePress Job Details Module — JS =====

const JB_API = '/api/job';

const JobDetails = {
    _data: null,

    async load(id) {
        try {
            const data = await $.get(`${JB_API}/detail/${id}`);
            this._data = data;
            this.render(data);
            $('#detailsLoader').hide();
            $('#detailsContent').show();
        } catch (err) {
            $('#detailsLoader').hide();
            $('#detailsError').show();
            $('#errorMessage').text(err.responseJSON?.message || 'Failed to load job details.');
        }
    },

    render(d) {
        // ── Header ──
        $('#hdJobNo').text(d.jobNo);
        this.renderStatusBadge('#hdStatus', d.status);
        $('#hdDate').text(d.jobDate);
        $('#hdCreatedBy').text(d.createdByName || 'System');
        $('#hdPriority').text(d.priority || 'NORMAL');

        // Source link
        let sourceHtml = '';
        if (d.quotationNo) {
            sourceHtml += `· <i class="bi bi-file-earmark-text me-1"></i><a href="/Quotation/Details?id=${d.quotationId}" class="text-white-50">${this.esc(d.quotationNo)}</a>`;
        }
        if (d.enquiryNo) {
            sourceHtml += ` · <i class="bi bi-clipboard-data me-1"></i><a href="/Enquiry/Details?id=${d.enquiryId}" class="text-white-50">${this.esc(d.enquiryNo)}</a>`;
        }
        $('#hdSourceLink').html(sourceHtml);

        // Progress bar
        if (d.progressPercent != null && d.progressPercent > 0) {
            $('#hdProgressWrap').show();
            $('#hdStageName').text((d.currentStage || '').replace(/_/g, ' '));
            $('#hdProgressPct').text(d.progressPercent + '%');
            $('#hdProgressBar').css('width', d.progressPercent + '%');
        }

        // ── Customer ──
        const initials = (d.customerName || '??').substring(0, 2).toUpperCase();
        $('#custAvatar').text(initials);
        $('#custName').text(d.customerName);
        $('#custCode').text(d.customerCode || '');
        $('#custGst').text(d.customerGst || '-');
        $('#custEmail').text(d.customerEmail || '-');
        $('#custAddress').text(d.customerAddress || '-');

        // ── Job Info ──
        $('#jbProduct').text(d.productName || '-');
        $('#jbDeliveryDate').text(d.deliveryDate || 'Not set');
        $('#jbPartyRef').text(d.partyRefNo || '-');
        if (d.productDescription) $('#jbDescription').text(d.productDescription);

        // Source
        let sourceText = 'Direct';
        if (d.quotationNo) {
            sourceText = `<a href="/Quotation/Details?id=${d.quotationId}" class="text-decoration-none">${this.esc(d.quotationNo)}</a>`;
        } else if (d.enquiryNo) {
            sourceText = `<a href="/Enquiry/Details?id=${d.enquiryId}" class="text-decoration-none">${this.esc(d.enquiryNo)}</a>`;
        }
        $('#jbSource').html(sourceText);

        const items = d.items || [];
        $('#jbItemCount').text(items.length);

        // ── Financial Summary ──
        $('#jbGrossAmount').text(this.fmt(d.grossAmount));
        $('#jbDiscountAmount').text('-' + this.fmt(d.discountAmount));
        $('#jbTaxAmount').text(this.fmt(d.taxAmount));
        $('#jbEstimatedCost').text(this.fmt(d.estimatedCost));
        $('#jbQuotedAmount').text(this.fmt(d.quotedAmount));
        $('#jbNetAmount').text(this.fmt(d.netAmount));

        // ── Tab count ──
        $('#tabItemCount').text(items.length);

        // ── Render items ──
        this.renderItems(items);

        // ── Render timeline ──
        this.renderTimeline(d.timeline);

        // ── Action buttons ──
        this.setupActions(d);

        // ── Customer Activity (lazy-load on tab click) ──
        this._activitiesLoaded = false;
        document.getElementById('tab-activities')?.addEventListener('shown.bs.tab', () => {
            if (!this._activitiesLoaded && d.partyId) {
                this._activitiesLoaded = true;
                CustomerActivity.load(d.partyId, {
                    container: '#customerActivityContainer',
                    currentModule: 'JOB',
                    currentId: d.jobId
                });
            }
        });

        // ── Store Issues (lazy-load on tab click) ──
        this._storeIssuesLoaded = false;
        document.getElementById('tab-store-issues')?.addEventListener('shown.bs.tab', () => {
            if (!this._storeIssuesLoaded) {
                this._storeIssuesLoaded = true;
                this.loadStoreIssues(d.jobId);
            }
        });

        // ── Job Workflow Pipeline (lazy-load on tab click) ──
        this._workflowLoaded = false;
        JobWorkflowPipeline.init(d.jobId);
        document.getElementById('tab-workflow')?.addEventListener('shown.bs.tab', () => {
            if (!this._workflowLoaded) {
                this._workflowLoaded = true;
                JobWorkflowPipeline.load();
            }
        });
    },

    renderStatusBadge(selector, status) {
        const cls = {
            'CREATED': 'jb-status-created',
            'JOB_ASSIGNED': 'jb-status-assigned',
            'ARTWORK_RECEIVED': 'jb-status-progress',
            'DESIGN_STARTED': 'jb-status-progress',
            'DESIGN_COMPLETED': 'jb-status-progress',
            'PLATE_PREPARED': 'jb-status-progress',
            'PRINTING_STARTED': 'jb-status-printing',
            'PRINTING_COMPLETED': 'jb-status-printing',
            'FINISHING_STARTED': 'jb-status-finishing',
            'FINISHING_COMPLETED': 'jb-status-finishing',
            'QUALITY_CHECK': 'jb-status-qc',
            'PACKING_DONE': 'jb-status-packed',
            'DISPATCHED': 'jb-status-dispatched',
            'DELIVERED': 'jb-status-delivered',
            'JOB_ON_HOLD': 'jb-status-hold',
            'JOB_RESUMED': 'jb-status-assigned',
            'JOB_CANCELLED': 'jb-status-cancelled',
            'JOB_REVISED': 'jb-status-revised',
            'PAYMENT_RECEIVED': 'jb-status-delivered'
        }[status] || 'bg-secondary-lt';
        const label = (status || 'N/A').replace(/_/g, ' ').replace(/\bJOB\b/g, '').trim() || status;
        $(selector).attr('class', 'jb-status-badge jb-status-pill ' + cls).text(label);
    },

    renderItems(items) {
        const container = $('#itemsContainer');
        if (items.length === 0) {
            container.html(`<div class="text-center py-4 text-muted">
                <i class="bi bi-inbox" style="font-size:2rem;opacity:.3;"></i>
                <div class="mt-2">No items in this job.</div>
            </div>`);
            return;
        }

        let html = '';
        items.forEach((item, idx) => {
            html += `
            <div class="card jb-item-detail-card mb-3">
                <div class="card-body">
                    <div class="row align-items-start">
                        <div class="col-auto">
                            <span class="avatar avatar-md rounded bg-purple-lt fw-bold">#${item.itemSequence || idx + 1}</span>
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
                                        ${item.calcRefNo ? `<a href="/RateCalculator/Details?id=${item.rateCalculatorId || ''}" class="badge bg-cyan-lt text-decoration-none"><i class="bi bi-link-45deg me-1"></i>${this.esc(item.calcRefNo)}</a>` : ''}
                                        ${item.status ? `<span class="badge bg-secondary-lt">${this.esc(item.status)}</span>` : ''}
                                    </div>
                                </div>
                                <div class="text-end">
                                    <div class="fs-3 fw-bold" style="color:#6f42c1;">${this.fmt(item.netAmount)}</div>
                                    <div class="text-muted small">Net Amount</div>
                                </div>
                            </div>

                            <div class="row g-3 mt-1">
                                <div class="col-6 col-md-2">
                                    <div class="jb-detail-metric">
                                        <div class="jb-detail-metric-label">Quantity</div>
                                        <div class="jb-detail-metric-value">${(item.quantity || 0).toLocaleString('en-IN')}</div>
                                    </div>
                                </div>
                                ${item.noOfPages ? `<div class="col-6 col-md-2">
                                    <div class="jb-detail-metric">
                                        <div class="jb-detail-metric-label">Pages</div>
                                        <div class="jb-detail-metric-value">${item.noOfPages}</div>
                                    </div>
                                </div>` : ''}
                                ${(item.trimWidthMm || item.trimHeightMm) ? `<div class="col-6 col-md-2">
                                    <div class="jb-detail-metric">
                                        <div class="jb-detail-metric-label">Trim Size</div>
                                        <div class="jb-detail-metric-value">${item.trimWidthMm}×${item.trimHeightMm}mm</div>
                                    </div>
                                </div>` : ''}
                                <div class="col-6 col-md-2">
                                    <div class="jb-detail-metric">
                                        <div class="jb-detail-metric-label">Unit Rate</div>
                                        <div class="jb-detail-metric-value">${this.fmt(item.unitRate)}</div>
                                    </div>
                                </div>
                                <div class="col-6 col-md-2">
                                    <div class="jb-detail-metric">
                                        <div class="jb-detail-metric-label">Gross Amt</div>
                                        <div class="jb-detail-metric-value">${this.fmt(item.grossAmount)}</div>
                                    </div>
                                </div>
                                <div class="col-6 col-md-2">
                                    <div class="jb-detail-metric">
                                        <div class="jb-detail-metric-label">Discount</div>
                                        <div class="jb-detail-metric-value">${item.discountPercent || 0}%</div>
                                    </div>
                                </div>
                                <div class="col-6 col-md-2">
                                    <div class="jb-detail-metric">
                                        <div class="jb-detail-metric-label">CGST</div>
                                        <div class="jb-detail-metric-value">${this.fmt(item.cgstAmount)} (${item.cgstPercent || 0}%)</div>
                                    </div>
                                </div>
                                <div class="col-6 col-md-2">
                                    <div class="jb-detail-metric">
                                        <div class="jb-detail-metric-label">SGST</div>
                                        <div class="jb-detail-metric-value">${this.fmt(item.sgstAmount)} (${item.sgstPercent || 0}%)</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>`;
        });

        // Grand totals row
        const totals = items.reduce((acc, i) => {
            acc.gross += i.grossAmount || 0;
            acc.discount += i.discountAmount || 0;
            acc.tax += i.totalTaxAmount || 0;
            acc.net += i.netAmount || 0;
            return acc;
        }, { gross: 0, discount: 0, tax: 0, net: 0 });

        html += `
        <div class="card bg-purple-lt border-0">
            <div class="card-body py-3">
                <div class="row text-center">
                    <div class="col-md-3">
                        <div class="small text-muted fw-semibold">SUBTOTAL</div>
                        <div class="fs-3 fw-bold">${this.fmt(totals.gross)}</div>
                    </div>
                    <div class="col-md-3">
                        <div class="small text-muted fw-semibold">DISCOUNT</div>
                        <div class="fs-3 fw-bold text-danger">${this.fmt(totals.discount)}</div>
                    </div>
                    <div class="col-md-3">
                        <div class="small text-muted fw-semibold">TAX</div>
                        <div class="fs-3 fw-bold">${this.fmt(totals.tax)}</div>
                    </div>
                    <div class="col-md-3">
                        <div class="small text-muted fw-semibold">NET TOTAL</div>
                        <div class="fs-2 fw-bold" style="color:#6f42c1;">${this.fmt(totals.net)}</div>
                    </div>
                </div>
            </div>
        </div>`;

        container.html(html);
    },

    setupActions(d) {
        const isClosed = d.status === 'DELIVERED' || d.status === 'JOB_CANCELLED';

        if (isClosed) {
            $('.jb-action-item').addClass('disabled').attr('aria-disabled', 'true');
            $('#btnEmailCustomer').addClass('disabled').attr('aria-disabled', 'true');
        }

        const statusMap = {
            'btnAssignJob': 'JOB_ASSIGNED',
            'btnArtworkReceived': 'ARTWORK_RECEIVED',
            'btnDesignStart': 'DESIGN_STARTED',
            'btnDesignComplete': 'DESIGN_COMPLETED',
            'btnPlatePrepared': 'PLATE_PREPARED',
            'btnPrintStart': 'PRINTING_STARTED',
            'btnPrintComplete': 'PRINTING_COMPLETED',
            'btnFinishStart': 'FINISHING_STARTED',
            'btnFinishComplete': 'FINISHING_COMPLETED',
            'btnQualityCheck': 'QUALITY_CHECK',
            'btnPackingDone': 'PACKING_DONE',
            'btnDispatch': 'DISPATCHED',
            'btnDeliver': 'DELIVERED',
            'btnPaymentReceived': 'PAYMENT_RECEIVED',
            'btnJobRevised': 'JOB_REVISED',
            'btnJobOnHold': 'JOB_ON_HOLD',
            'btnJobResumed': 'JOB_RESUMED',
            'btnJobCancel': 'JOB_CANCELLED'
        };

        Object.entries(statusMap).forEach(([btnId, status]) => {
            $(`#${btnId}`).on('click', () => this.changeStatus(d.jobId, status, d.jobNo));
        });

        // Create Challan — navigate to challan create page with job pre-selected
        $('#btnCreateChallan').on('click', () => {
            window.location.href = `/Challan/Create?FromJobId=${d.jobId}`;
        });

        // Outsource Job — navigate to outsource create page with job pre-selected
        $('#btnOutsourceJob').on('click', () => {
            window.location.href = `/Outsource/Create?FromJobId=${d.jobId}`;
        });
    },

    async changeStatus(id, status, jobNo) {
        const label = (status || '').replace(/_/g, ' ').replace(/\bJOB\b/g, '').trim();
        const confirmed = await Swal2.confirmStatus(jobNo || `JOB-${id}`, label);
        if (!confirmed) return;

        try {
            await $.ajax({
                url: `${JB_API}/updatestatus`,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ jobId: id, status })
            });
            Swal2.success(`Status updated to ${label}.`);
            setTimeout(() => this.load(id), 500);
        } catch (err) {
            Swal2.error(err.responseJSON?.message || 'Failed to update status.');
        }
    },

    async sendEmail() {
        if (!this._data) return;
        const d = this._data;

        if (!d.customerEmail) {
            Swal2.warning('Customer does not have an email address on file.');
            return;
        }

        const result = await Swal.fire({
            title: 'Email Job Confirmation?',
            html: `Send <strong>${d.jobNo}</strong> confirmation to <strong>${d.customerEmail}</strong>?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: '<i class="bi bi-envelope me-1"></i>Send Email',
            confirmButtonColor: '#6f42c1'
        });

        if (!result.isConfirmed) return;

        try {
            Swal2.showLoading('Sending email...');
            const res = await $.ajax({
                url: `${JB_API}/send-email/${d.jobId}`,
                method: 'POST'
            });
            Swal2.hideLoading();
            Swal2.success(res.message || 'Job confirmation emailed successfully.');
            setTimeout(() => this.load(d.jobId), 500);
        } catch (err) {
            Swal2.hideLoading();
            Swal2.error(err.responseJSON?.message || 'Failed to send email.');
        }
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
            'JOB_CREATED':              { icon: 'bi-plus-circle-fill',       bg: 'bg-success-lt',   color: 'text-success' },
            'JOB_ASSIGNED':             { icon: 'bi-person-check-fill',      bg: 'bg-info-lt',      color: 'text-info' },
            'ARTWORK_RECEIVED':         { icon: 'bi-image-fill',             bg: 'bg-teal-lt',      color: 'text-teal' },
            'DESIGN_STARTED':           { icon: 'bi-palette-fill',           bg: 'bg-purple-lt',    color: 'text-purple' },
            'DESIGN_COMPLETED':         { icon: 'bi-palette2',               bg: 'bg-purple-lt',    color: 'text-purple' },
            'PLATE_PREPARED':           { icon: 'bi-layers-fill',            bg: 'bg-orange-lt',    color: 'text-orange' },
            'PRINTING_STARTED':         { icon: 'bi-printer-fill',           bg: 'bg-primary-lt',   color: 'text-primary' },
            'PRINTING_COMPLETED':       { icon: 'bi-printer-fill',           bg: 'bg-primary-lt',   color: 'text-primary' },
            'FINISHING_STARTED':        { icon: 'bi-scissors',               bg: 'bg-cyan-lt',      color: 'text-cyan' },
            'FINISHING_COMPLETED':      { icon: 'bi-check2-square',          bg: 'bg-cyan-lt',      color: 'text-cyan' },
            'QUALITY_CHECK':            { icon: 'bi-shield-check',           bg: 'bg-success-lt',   color: 'text-success' },
            'PACKING_DONE':             { icon: 'bi-box-seam-fill',          bg: 'bg-yellow-lt',    color: 'text-yellow' },
            'DISPATCHED':               { icon: 'bi-truck',                  bg: 'bg-blue-lt',      color: 'text-blue' },
            'DELIVERED':                { icon: 'bi-check-circle-fill',      bg: 'bg-success-lt',   color: 'text-success' },
            'JOB_ON_HOLD':              { icon: 'bi-pause-circle-fill',      bg: 'bg-warning-lt',   color: 'text-warning' },
            'JOB_RESUMED':              { icon: 'bi-play-circle-fill',       bg: 'bg-info-lt',      color: 'text-info' },
            'JOB_CANCELLED':            { icon: 'bi-x-circle-fill',          bg: 'bg-danger-lt',    color: 'text-danger' },
            'JOB_REVISED':              { icon: 'bi-pencil-fill',            bg: 'bg-azure-lt',     color: 'text-azure' },
            'PAYMENT_RECEIVED':         { icon: 'bi-currency-rupee',         bg: 'bg-green-lt',     color: 'text-green' },
            'SENT_TO_CUSTOMER':         { icon: 'bi-envelope-fill',          bg: 'bg-teal-lt',      color: 'text-teal' },
            'CONVERTED_FROM_ENQUIRY':   { icon: 'bi-clipboard-data',         bg: 'bg-azure-lt',     color: 'text-azure' },
            'CONVERTED_FROM_QUOTATION': { icon: 'bi-file-earmark-text',      bg: 'bg-green-lt',     color: 'text-green' },
            'STORE_ISSUE':              { icon: 'bi-box-arrow-up',           bg: 'bg-orange-lt',    color: 'text-orange' },
            'STATUS_CHANGED':           { icon: 'bi-arrow-repeat',           bg: 'bg-info-lt',      color: 'text-info' },
        };
        const defaultCfg = { icon: 'bi-circle-fill', bg: 'bg-secondary-lt', color: 'text-secondary' };

        let html = '<ul class="timeline">';
        timeline.forEach(t => {
            const cfg = eventConfig[t.eventType] || eventConfig[t.eventCode] || defaultCfg;

            let statusHtml = '';
            if (t.oldStatus || t.newStatus) {
                statusHtml = `<div class="mt-2 d-flex align-items-center gap-2 flex-wrap">`;
                if (t.oldStatus) statusHtml += `<span class="badge bg-secondary-lt">${this.esc(t.oldStatus)}</span>`;
                if (t.oldStatus && t.newStatus) statusHtml += `<i class="bi bi-arrow-right small text-muted"></i>`;
                if (t.newStatus) statusHtml += `<span class="badge ${this._timelineStatusClass(t.newStatus)}">${this.esc(t.newStatus)}</span>`;
                statusHtml += `</div>`;
            }

            let amountHtml = '';
            if (t.oldAmount != null || t.newAmount != null) {
                amountHtml = `<div class="mt-2 small">`;
                if (t.oldAmount != null) amountHtml += `<span class="text-muted">₹${Number(t.oldAmount).toLocaleString('en-IN', {minimumFractionDigits:2})}</span>`;
                if (t.oldAmount != null && t.newAmount != null) amountHtml += ` <i class="bi bi-arrow-right small text-muted"></i> `;
                if (t.newAmount != null) amountHtml += `<span class="fw-semibold" style="color:#6f42c1;">₹${Number(t.newAmount).toLocaleString('en-IN', {minimumFractionDigits:2})}</span>`;
                amountHtml += `</div>`;
            }

            let processHtml = '';
            if (t.processName) {
                processHtml = `<div class="mt-2 small">
                    <i class="bi bi-gear me-1 text-purple"></i>Process: <span class="badge bg-purple-lt">${this.esc(t.processName)}</span>
                </div>`;
            }

            let commHtml = '';
            if (t.communicationMode) {
                commHtml = `<div class="mt-2 small">
                    <i class="bi bi-send me-1 text-primary"></i>via <span class="badge bg-primary-lt">${this.esc(t.communicationMode)}</span>
                    ${t.communicationReference ? ` to <strong>${this.esc(t.communicationReference)}</strong>` : ''}
                </div>`;
            }

            let attachHtml = '';
            if (t.attachmentUrl) {
                attachHtml = `<div class="mt-2 small">
                    <a href="${this.esc(t.attachmentUrl)}" target="_blank" class="text-decoration-none">
                        <i class="bi bi-paperclip me-1"></i>Attachment
                    </a>
                </div>`;
            }

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
                        ${amountHtml}
                        ${processHtml}
                        ${commHtml}
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
            'CREATED': 'bg-success-lt',
            'JOB_ASSIGNED': 'bg-info-lt',
            'ARTWORK_RECEIVED': 'bg-teal-lt',
            'DESIGN_STARTED': 'bg-purple-lt',
            'DESIGN_COMPLETED': 'bg-purple-lt',
            'PLATE_PREPARED': 'bg-orange-lt',
            'PRINTING_STARTED': 'bg-primary-lt',
            'PRINTING_COMPLETED': 'bg-primary-lt',
            'FINISHING_STARTED': 'bg-cyan-lt',
            'FINISHING_COMPLETED': 'bg-cyan-lt',
            'QUALITY_CHECK': 'bg-success-lt',
            'PACKING_DONE': 'bg-yellow-lt',
            'DISPATCHED': 'bg-blue-lt',
            'DELIVERED': 'bg-success-lt',
            'JOB_ON_HOLD': 'bg-warning-lt',
            'JOB_RESUMED': 'bg-info-lt',
            'JOB_CANCELLED': 'bg-danger-lt',
            'JOB_REVISED': 'bg-azure-lt',
            'PAYMENT_RECEIVED': 'bg-green-lt',
            'CONVERTED': 'bg-success-lt'
        }[status] || 'bg-secondary-lt';
    },

    // ── Store Issues ──
    async loadStoreIssues(jobId) {
        const container = $('#storeIssuesContainer');
        try {
            const issues = await $.get(`/api/store/jobs/${jobId}/issued-items`);
            this.renderStoreIssues(issues);
        } catch (err) {
            container.html(`<div class="text-center py-4 text-muted">
                <i class="bi bi-exclamation-triangle text-danger" style="font-size:2rem;opacity:.5;"></i>
                <div class="mt-2">Failed to load store issues.</div>
            </div>`);
        }
    },

    renderStoreIssues(issues) {
        const container = $('#storeIssuesContainer');

        if (!issues || issues.length === 0) {
            container.html(`<div class="text-center py-4 text-muted">
                <i class="bi bi-inbox" style="font-size:2rem;opacity:.3;"></i>
                <div class="mt-2">No store issues found for this job.</div>
            </div>`);
            $('#tabStoreIssueCount').hide();
            return;
        }

        let totalItems = 0;
        let totalAmount = 0;
        let html = '';

        issues.forEach(iss => {
            const statusCls = {
                'ISSUED': 'bg-green-lt',
                'DRAFT': 'bg-yellow-lt',
                'APPROVED': 'bg-blue-lt',
                'CANCELLED': 'bg-danger-lt'
            }[iss.status] || 'bg-secondary-lt';

            let itemsHtml = '';
            if (iss.items && iss.items.length) {
                itemsHtml = `<div class="table-responsive">
                    <table class="table table-sm table-hover mb-0">
                        <thead>
                            <tr>
                                <th class="small text-muted">#</th>
                                <th class="small text-muted">Material</th>
                                <th class="small text-muted">Code</th>
                                <th class="small text-muted text-center">Qty</th>
                                <th class="small text-muted">UOM</th>
                                <th class="small text-muted text-end">Rate</th>
                                <th class="small text-muted text-end">Amount</th>
                            </tr>
                        </thead>
                        <tbody>`;
                iss.items.forEach((it, idx) => {
                    totalItems++;
                    const amt = parseFloat(it.amount) || 0;
                    totalAmount += amt;
                    itemsHtml += `<tr>
                        <td class="small text-muted">${idx + 1}</td>
                        <td class="small">${this.esc(it.materialName)}</td>
                        <td class="small text-muted">${this.esc(it.materialCode || '-')}</td>
                        <td class="small text-center">${it.issuedQuantity}</td>
                        <td class="small">${this.esc(it.uom || '')}</td>
                        <td class="small text-end">${(parseFloat(it.rate) || 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}</td>
                        <td class="small text-end fw-semibold">${amt.toLocaleString('en-IN', { minimumFractionDigits: 2 })}</td>
                    </tr>`;
                });
                itemsHtml += `</tbody></table></div>`;
            }

            html += `<div class="card jb-detail-card mb-3">
                <div class="card-header py-2">
                    <div class="d-flex justify-content-between align-items-center w-100">
                        <div class="d-flex align-items-center gap-2">
                            <i class="bi bi-box-arrow-up text-orange"></i>
                            <strong>${this.esc(iss.issueNo)}</strong>
                            <span class="badge ${statusCls}">${this.esc(iss.status)}</span>
                        </div>
                        <div class="d-flex align-items-center gap-3 small text-muted">
                            <span><i class="bi bi-calendar3 me-1"></i>${this.esc(iss.issueDate)}</span>
                            <span><i class="bi bi-person me-1"></i>${this.esc(iss.createdByName || 'System')}</span>
                        </div>
                    </div>
                </div>
                <div class="card-body p-0">${itemsHtml}</div>
            </div>`;
        });

        // Summary card
        html = `<div class="card bg-orange-lt border-0 mb-3">
            <div class="card-body py-3">
                <div class="row text-center">
                    <div class="col-md-4">
                        <div class="small text-muted fw-semibold">TOTAL ISSUES</div>
                        <div class="fs-3 fw-bold">${issues.length}</div>
                    </div>
                    <div class="col-md-4">
                        <div class="small text-muted fw-semibold">TOTAL ITEMS</div>
                        <div class="fs-3 fw-bold">${totalItems}</div>
                    </div>
                    <div class="col-md-4">
                        <div class="small text-muted fw-semibold">TOTAL AMOUNT</div>
                        <div class="fs-3 fw-bold">${this.fmt(totalAmount)}</div>
                    </div>
                </div>
            </div>
        </div>` + html;

        container.html(html);
        $('#tabStoreIssueCount').text(issues.length).show();
    },

    // ── Helpers ──
    fmt(val) { return '₹' + (val || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); },
    esc(v) { return (v || '').replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll("'", '&#39;'); }
};
