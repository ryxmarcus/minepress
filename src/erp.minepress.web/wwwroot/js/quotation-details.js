// ===== MinePress Quotation Module — JS =====

const QT_API = '/api/quotation';

// ═══════════════════════════════════════════
//  CREATE MODE
// ═══════════════════════════════════════════

const QuotationCreate = {
    _items: [],
    _partyId: 0,
    _enquiryId: null,

    async init(fromEnquiryId) {
        // Initialize customer search widget
        if (window.CustomerSearch) {
            CustomerSearch.init({
                apiBase: '/api/enquiry',
                onSelect: (cust) => { this._partyId = cust.partyId; },
                onClear: () => { this._partyId = 0; }
            });
        }

        if (fromEnquiryId) {
            this._enquiryId = fromEnquiryId;
            await this.loadFromEnquiry(fromEnquiryId);
        }
        // Set default valid till to 30 days from now
        const d = new Date();
        d.setDate(d.getDate() + 30);
        $('#txtValidTill').val(d.toISOString().split('T')[0]);
    },

    async loadFromEnquiry(enquiryId) {
        try {
            const data = await $.get(`${QT_API}/from-enquiry/${enquiryId}`);
            $('#createPageTitle').text(`New Quotation from ${data.enquiryNo}`);
            $('#createPageSubtitle').text(`Converting enquiry ${data.enquiryNo} to quotation`);

            // Set customer via the customer search widget
            this._partyId = data.partyId;
            if (window.CustomerSearch && data.partyId) {
                CustomerSearch.selectById(data.partyId);
            }

            // Populate items
            if (data.items && data.items.length > 0) {
                data.items.forEach((item, idx) => {
                    const unitRate = item.costPerUnit || 0;
                    const qty = item.quantity || 0;
                    const gross = unitRate * qty;
                    this._items.push({
                        enquiryItemId: item.enquiryItemId,
                        rateCalculatorId: item.rateCalculatorId,
                        calcRefNo: item.calcRefNo,
                        itemSequence: idx + 1,
                        productName: item.productName,
                        productDescription: item.productDescription || '',
                        productTypeName: item.productTypeName || '',
                        jobTypeName: item.jobTypeName || '',
                        productSizeName: item.productSizeName || '',
                        noOfPages: item.noOfPages || 0,
                        trimWidthMm: item.trimWidthMm || 0,
                        trimHeightMm: item.trimHeightMm || 0,
                        printingMethod: item.printingMethod || '',
                        quantity: qty,
                        unitRate: unitRate,
                        grossAmount: gross,
                        discountPercent: 0,
                        discountAmount: 0,
                        taxableValue: gross,
                        cgstPercent: 9,
                        sgstPercent: 9,
                        igstPercent: 0,
                        remarks: ''
                    });
                });
                this.renderItems();
                this.recalcAll();
            }
        } catch (err) {
            Swal2.error('Failed to load enquiry data: ' + (err.responseJSON?.message || err.statusText));
        }
    },

    renderItems() {
        const tbody = $('#itemsBody');
        tbody.empty();

        if (this._items.length === 0) {
            tbody.html(`<tr id="noItemsRow"><td colspan="12" class="text-center text-muted py-4">
                <i class="bi bi-inbox" style="font-size:2rem;opacity:.3;"></i>
                <div class="mt-2">No items added yet. Click "Add Item" to begin.</div>
            </td></tr>`);
            $('#createItemCount').text(0);
            return;
        }

        $('#createItemCount').text(this._items.length);
        this._items.forEach((item, idx) => {
            tbody.append(this.buildItemRow(item, idx));
        });
    },

    buildItemRow(item, idx) {
        return `<tr data-idx="${idx}" class="qt-item-row">
            <td class="text-center text-muted">${idx + 1}</td>
            <td><input type="text" class="form-control form-control-sm" value="${this.esc(item.productName)}" onchange="QuotationCreate.updateField(${idx},'productName',this.value)" /></td>
            <td><input type="text" class="form-control form-control-sm" value="${this.esc(item.productDescription)}" onchange="QuotationCreate.updateField(${idx},'productDescription',this.value)" /></td>
            <td><input type="number" class="form-control form-control-sm text-center" value="${item.quantity}" min="1" onchange="QuotationCreate.updateNumField(${idx},'quantity',this.value)" /></td>
            <td><input type="number" class="form-control form-control-sm text-end" value="${item.unitRate.toFixed(2)}" step="0.01" onchange="QuotationCreate.updateNumField(${idx},'unitRate',this.value)" /></td>
            <td class="text-end fw-semibold qt-calc-gross">${this.fmt(item.grossAmount)}</td>
            <td><input type="number" class="form-control form-control-sm text-end" value="${item.discountPercent}" step="0.01" max="100" onchange="QuotationCreate.updateNumField(${idx},'discountPercent',this.value)" /></td>
            <td><input type="number" class="form-control form-control-sm text-end" value="${item.cgstPercent}" step="0.01" onchange="QuotationCreate.updateNumField(${idx},'cgstPercent',this.value)" /></td>
            <td><input type="number" class="form-control form-control-sm text-end" value="${item.sgstPercent}" step="0.01" onchange="QuotationCreate.updateNumField(${idx},'sgstPercent',this.value)" /></td>
            <td class="text-end fw-semibold qt-calc-tax">${this.fmt(item.totalTaxAmount || 0)}</td>
            <td class="text-end fw-bold text-primary qt-calc-net">${this.fmt(item.netAmount || 0)}</td>
            <td><button class="btn btn-ghost-danger btn-sm" onclick="QuotationCreate.removeItem(${idx})"><i class="bi bi-x-lg"></i></button></td>
        </tr>`;
    },

    updateField(idx, field, value) {
        this._items[idx][field] = value;
    },

    updateNumField(idx, field, value) {
        this._items[idx][field] = parseFloat(value) || 0;
        this.recalcItem(idx);
        this.renderItems();
        this.recalcTotals();
    },

    recalcItem(idx) {
        const item = this._items[idx];
        item.grossAmount = item.quantity * item.unitRate;
        item.discountAmount = item.grossAmount * (item.discountPercent / 100);
        item.taxableValue = item.grossAmount - item.discountAmount;
        item.cgstAmount = item.taxableValue * (item.cgstPercent / 100);
        item.sgstAmount = item.taxableValue * (item.sgstPercent / 100);
        item.igstAmount = item.taxableValue * ((item.igstPercent || 0) / 100);
        item.totalTaxAmount = item.cgstAmount + item.sgstAmount + item.igstAmount;
        item.netAmount = item.taxableValue + item.totalTaxAmount;
    },

    recalcAll() {
        this._items.forEach((_, idx) => this.recalcItem(idx));
        this.renderItems();
        this.recalcTotals();
    },

    recalcTotals() {
        let total = 0, discount = 0, taxable = 0, tax = 0, net = 0;
        this._items.forEach(item => {
            total += item.grossAmount || 0;
            discount += item.discountAmount || 0;
            taxable += item.taxableValue || 0;
            tax += item.totalTaxAmount || 0;
            net += item.netAmount || 0;
        });
        $('#sumTotal').text(this.fmtInr(total));
        $('#sumDiscount').text('-' + this.fmtInr(discount));
        $('#sumTaxable').text(this.fmtInr(taxable));
        $('#sumTax').text(this.fmtInr(tax));
        $('#sumNet').text(this.fmtInr(net));
    },

    removeItem(idx) {
        this._items.splice(idx, 1);
        this._items.forEach((item, i) => item.itemSequence = i + 1);
        this.renderItems();
        this.recalcTotals();
    },

    fmt(val) { return '₹' + (val || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); },
    fmtInr(val) { return '₹' + (val || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); },
    esc(v) { return (v || '').replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll("'", '&#39;'); }
};

// Global functions called from HTML
function addItemRow() {
    QuotationCreate._items.push({
        enquiryItemId: null,
        rateCalculatorId: null,
        calcRefNo: null,
        itemSequence: QuotationCreate._items.length + 1,
        productName: '',
        productDescription: '',
        quantity: 1,
        unitRate: 0,
        grossAmount: 0,
        discountPercent: 0,
        discountAmount: 0,
        taxableValue: 0,
        cgstPercent: 9,
        sgstPercent: 9,
        igstPercent: 0,
        cgstAmount: 0,
        sgstAmount: 0,
        igstAmount: 0,
        totalTaxAmount: 0,
        netAmount: 0,
        remarks: ''
    });
    QuotationCreate.renderItems();
}

async function saveQuotation() {
    // Get partyId from customer search widget or from enquiry data
    const partyId = QuotationCreate._partyId || (window.CustomerSearch ? CustomerSearch.getSelectedId() : 0);
    if (!partyId) {
        Swal2.warning('Please select a customer first.');
        return;
    }

    if (QuotationCreate._items.length === 0) {
        Swal2.warning('Please add at least one item.');
        return;
    }

    // Validate items
    for (let i = 0; i < QuotationCreate._items.length; i++) {
        const item = QuotationCreate._items[i];
        if (!item.productName.trim()) {
            Swal2.warning(`Item ${i + 1}: Product name is required.`);
            return;
        }
        if (item.quantity <= 0) {
            Swal2.warning(`Item ${i + 1}: Quantity must be greater than 0.`);
            return;
        }
    }

    // Calculate totals
    let totalAmount = 0, discountAmount = 0, taxableAmount = 0, taxAmount = 0, netAmount = 0;
    QuotationCreate._items.forEach(item => {
        totalAmount += item.grossAmount || 0;
        discountAmount += item.discountAmount || 0;
        taxableAmount += item.taxableValue || 0;
        taxAmount += item.totalTaxAmount || 0;
        netAmount += item.netAmount || 0;
    });

    const payload = {
        partyId: partyId,
        enquiryId: QuotationCreate._enquiryId,
        partyRefNo: $('#txtPartyRefNo').val() || null,
        validTill: $('#txtValidTill').val() || null,
        termsConditions: $('#txtTerms').val() || null,
        remarks: $('#txtRemarks').val() || null,
        totalAmount: totalAmount,
        discountAmount: discountAmount,
        taxableAmount: taxableAmount,
        taxAmount: taxAmount,
        netAmount: netAmount,
        items: QuotationCreate._items.map((item, idx) => ({
            enquiryItemId: item.enquiryItemId,
            itemSequence: idx + 1,
            productName: item.productName,
            productDescription: item.productDescription,
            quantity: item.quantity,
            unitRate: item.unitRate,
            grossAmount: item.grossAmount,
            discountPercent: item.discountPercent,
            discountAmount: item.discountAmount,
            taxableValue: item.taxableValue,
            cgstPercent: item.cgstPercent,
            cgstAmount: item.cgstAmount,
            sgstPercent: item.sgstPercent,
            sgstAmount: item.sgstAmount,
            igstPercent: item.igstPercent || 0,
            igstAmount: item.igstAmount || 0,
            totalTaxAmount: item.totalTaxAmount,
            netAmount: item.netAmount,
            rateCalculatorId: item.rateCalculatorId,
            calcRefNo: item.calcRefNo,
            remarks: item.remarks
        }))
    };

    try {
        Swal2.showLoading('Saving quotation...');
        const result = await $.ajax({
            url: `${QT_API}/save`,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload)
        });
        Swal2.hideLoading();

        // Auto-send email to customer
        try {
            await $.ajax({ url: `${QT_API}/send-email/${result.quotationId}`, method: 'POST' });
        } catch (_) { /* email send is best-effort */ }

        await Swal.fire({
            icon: 'success',
            title: 'Quotation Created & Emailed!',
            html: `<strong>${result.quotationNo}</strong> saved and emailed to the customer.`,
            confirmButtonText: 'View Quotation',
            showCancelButton: true,
            cancelButtonText: 'Back to List'
        }).then((res) => {
            if (res.isConfirmed) {
                window.location.href = `/Quotation/Details?id=${result.quotationId}`;
            } else {
                window.location.href = '/Quotation';
            }
        });
    } catch (err) {
        Swal2.hideLoading();
        Swal2.error(err.responseJSON?.message || 'Failed to save quotation.');
    }
}


// ═══════════════════════════════════════════
//  VIEW/DETAIL MODE
// ═══════════════════════════════════════════

const QuotationDetails = {
    _data: null,

    async load(id) {
        try {
            const data = await $.get(`${QT_API}/detail/${id}`);
            this._data = data;
            this.render(data);
            $('#detailsLoader').hide();
            $('#detailsContent').show();
        } catch (err) {
            $('#detailsLoader').hide();
            $('#detailsError').show();
            $('#errorMessage').text(err.responseJSON?.message || 'Failed to load quotation details.');
        }
    },

    render(d) {
        // ── Header ──
        $('#hdQuotationNo').text(d.quotationNo);
        this.renderStatusBadge('#hdStatus', d.status);
        $('#hdDate').text(d.quotationDate);
        $('#hdCreatedBy').text(d.createdByName || 'System');
        if (d.enquiryNo) {
            $('#hdEnquiryLink').html(`· <i class="bi bi-clipboard-data me-1"></i><a href="/Enquiry/Details?id=${d.enquiryId}" class="text-white-50">${this.esc(d.enquiryNo)}</a>`);
        }

        // ── Customer ──
        const initials = (d.customerName || '??').substring(0, 2).toUpperCase();
        $('#custAvatar').text(initials);
        $('#custName').text(d.customerName);
        $('#custCode').text(d.customerCode || '');
        $('#custGst').text(d.customerGst || '-');
        $('#custEmail').text(d.customerEmail || '-');
        $('#custAddress').text(d.customerAddress || '-');

        // ── Quotation Info ──
        $('#qtValidTill').text(d.validTill || 'Not set');
        $('#qtPartyRef').text(d.partyRefNo || '-');
        $('#qtEnquiry').html(d.enquiryNo
            ? `<a href="/Enquiry/Details?id=${d.enquiryId}" class="text-decoration-none">${this.esc(d.enquiryNo)}</a>`
            : '-');
        const items = d.items || [];
        $('#qtItemCount').text(items.length);
        if (d.remarks) $('#qtRemarks').text(d.remarks);

        // ── Financial Summary ──
        $('#qtTotalAmount').text(this.fmt(d.totalAmount));
        $('#qtDiscountAmount').text('-' + this.fmt(d.discountAmount));
        $('#qtTaxAmount').text(this.fmt(d.taxAmount));
        $('#qtNetAmount').text(this.fmt(d.netAmount));

        // ── Terms ──
        if (d.termsConditions) {
            $('#termsCard').show();
            $('#qtTerms').text(d.termsConditions);
        }

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
        $('#tab-activities').on('shown.bs.tab', () => {
            if (!this._activitiesLoaded && d.partyId) {
                this._activitiesLoaded = true;
                CustomerActivity.load(d.partyId, {
                    container: '#customerActivityContainer',
                    currentModule: 'QUOTATION',
                    currentId: d.quotationId
                });
            }
        });
    },

    renderStatusBadge(selector, status) {
        const cls = {
            'DRAFT': 'qt-status-draft',
            'SENT': 'qt-status-sent',
            'APPROVED': 'qt-status-approved',
            'REVISED': 'qt-status-revised',
            'CLOSED': 'qt-status-closed',
            'CANCELLED': 'qt-status-cancelled'
        }[status] || 'bg-secondary-lt';
        $(selector).attr('class', 'qt-status-badge qt-status-pill ' + cls).text(status || 'N/A');
    },

    renderItems(items) {
        const container = $('#itemsContainer');
        if (items.length === 0) {
            container.html(`<div class="text-center py-4 text-muted">
                <i class="bi bi-inbox" style="font-size:2rem;opacity:.3;"></i>
                <div class="mt-2">No items in this quotation.</div>
            </div>`);
            return;
        }

        let html = '';
        items.forEach((item, idx) => {
            html += `
            <div class="card qt-item-detail-card mb-3">
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
                                    ${item.calcRefNo ? `<a href="/RateCalculator/Details?id=${item.rateCalculatorId || ''}" class="badge bg-cyan-lt text-decoration-none"><i class="bi bi-link-45deg me-1"></i>${this.esc(item.calcRefNo)}</a>` : ''}
                                </div>
                                <div class="text-end">
                                    <div class="fs-3 fw-bold text-primary">${this.fmt(item.netAmount)}</div>
                                    <div class="text-muted small">Net Amount</div>
                                </div>
                            </div>

                            <div class="row g-3 mt-1">
                                <div class="col-6 col-md-2">
                                    <div class="qt-detail-metric">
                                        <div class="qt-detail-metric-label">Quantity</div>
                                        <div class="qt-detail-metric-value">${(item.quantity || 0).toLocaleString('en-IN')}</div>
                                    </div>
                                </div>
                                <div class="col-6 col-md-2">
                                    <div class="qt-detail-metric">
                                        <div class="qt-detail-metric-label">Unit Rate</div>
                                        <div class="qt-detail-metric-value">${this.fmt(item.unitRate)}</div>
                                    </div>
                                </div>
                                <div class="col-6 col-md-2">
                                    <div class="qt-detail-metric">
                                        <div class="qt-detail-metric-label">Gross Amt</div>
                                        <div class="qt-detail-metric-value">${this.fmt(item.grossAmount)}</div>
                                    </div>
                                </div>
                                <div class="col-6 col-md-2">
                                    <div class="qt-detail-metric">
                                        <div class="qt-detail-metric-label">Discount</div>
                                        <div class="qt-detail-metric-value">${item.discountPercent || 0}%</div>
                                    </div>
                                </div>
                                <div class="col-6 col-md-2">
                                    <div class="qt-detail-metric">
                                        <div class="qt-detail-metric-label">CGST</div>
                                        <div class="qt-detail-metric-value">${this.fmt(item.cgstAmount)} (${item.cgstPercent || 0}%)</div>
                                    </div>
                                </div>
                                <div class="col-6 col-md-2">
                                    <div class="qt-detail-metric">
                                        <div class="qt-detail-metric-label">SGST</div>
                                        <div class="qt-detail-metric-value">${this.fmt(item.sgstAmount)} (${item.sgstPercent || 0}%)</div>
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
        <div class="card bg-primary-lt border-0">
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
                        <div class="fs-2 fw-bold text-primary">${this.fmt(totals.net)}</div>
                    </div>
                </div>
            </div>
        </div>`;

        container.html(html);
    },

    setupActions(d) {
        const isClosed = d.status === 'CLOSED' || d.status === 'CANCELLED';

        if (isClosed) {
            $('#btnEmailCustomer, #btnSendQuotation, #btnApproveQuotation, #btnCancelQuotation, #btnCloseQuotation')
                .addClass('disabled').attr('aria-disabled', 'true');
        }

        $('#btnSendQuotation').on('click', () => this.changeStatus(d.quotationId, 'SENT', d.quotationNo));
        $('#btnApproveQuotation').on('click', () => this.changeStatus(d.quotationId, 'APPROVED', d.quotationNo));
        $('#btnCancelQuotation').on('click', () => this.changeStatus(d.quotationId, 'CANCELLED', d.quotationNo));
        $('#btnCloseQuotation').on('click', () => this.changeStatus(d.quotationId, 'CLOSED', d.quotationNo));
    },

    async changeStatus(id, status, quotationNo) {
        const confirmed = await Swal2.confirmStatus(quotationNo || `QTN-${id}`, status);
        if (!confirmed) return;

        try {
            await $.ajax({
                url: `${QT_API}/updatestatus`,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ quotationId: id, status })
            });
            Swal2.success(`Status updated to ${status}.`);
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
            title: 'Email Quotation?',
            html: `Send <strong>${d.quotationNo}</strong> to <strong>${d.customerEmail}</strong>?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: '<i class="bi bi-envelope me-1"></i>Send Email',
            confirmButtonColor: '#198754'
        });

        if (!result.isConfirmed) return;

        try {
            Swal2.showLoading('Sending email...');
            const res = await $.ajax({
                url: `${QT_API}/send-email/${d.quotationId}`,
                method: 'POST'
            });
            Swal2.hideLoading();
            Swal2.success(res.message || 'Quotation emailed successfully.');
            setTimeout(() => this.load(d.quotationId), 500);
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
            'CREATED':                  { icon: 'bi-plus-circle-fill',       bg: 'bg-success-lt',   color: 'text-success' },
            'STATUS_CHANGED':           { icon: 'bi-arrow-repeat',           bg: 'bg-info-lt',      color: 'text-info' },
            'SENT_TO_CUSTOMER':         { icon: 'bi-envelope-fill',          bg: 'bg-teal-lt',      color: 'text-teal' },
            'CONVERTED_FROM_ENQUIRY':   { icon: 'bi-clipboard-data',         bg: 'bg-azure-lt',     color: 'text-azure' },
            'SENT':                     { icon: 'bi-send-fill',              bg: 'bg-primary-lt',   color: 'text-primary' },
            'APPROVED':                 { icon: 'bi-check-circle-fill',      bg: 'bg-success-lt',   color: 'text-success' },
            'REVISED':                  { icon: 'bi-pencil-fill',            bg: 'bg-blue-lt',      color: 'text-blue' },
            'CLOSED':                   { icon: 'bi-lock-fill',              bg: 'bg-secondary-lt', color: 'text-secondary' },
            'CANCELLED':                { icon: 'bi-slash-circle',           bg: 'bg-danger-lt',    color: 'text-danger' },
            'DELETED':                  { icon: 'bi-trash-fill',             bg: 'bg-danger-lt',    color: 'text-danger' },
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
                if (t.newAmount != null) amountHtml += `<span class="fw-semibold text-primary">₹${Number(t.newAmount).toLocaleString('en-IN', {minimumFractionDigits:2})}</span>`;
                amountHtml += `</div>`;
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
            'DRAFT': 'bg-warning-lt', 'SENT': 'bg-info-lt', 'APPROVED': 'bg-success-lt',
            'REVISED': 'bg-blue-lt', 'CLOSED': 'bg-secondary-lt', 'CANCELLED': 'bg-danger-lt',
            'CONVERTED': 'bg-success-lt'
        }[status] || 'bg-secondary-lt';
    },

    // ── Helpers ──
    fmt(val) { return '₹' + (val || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); },
    esc(v) { return (v || '').replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll("'", '&#39;'); }
};
