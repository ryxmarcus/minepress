/**
 * ProdBreakdown — Machine Breakdown Tracker IIFE
 * Page: /Production/MachineBreakdown
 */
var ProdBreakdown = (function () {
    'use strict';

    const API = '/api/production';
    let _machines = [];
    let _breakdowns = [];
    let _modal = null;

    // ─── Init ────────────────────────────────────────────────
    function init() {
        _modal = new ((typeof minepress !== 'undefined' && minepress.Modal) || bootstrap.Modal)(
            document.getElementById('bdModal'));
        loadMachines().then(refresh);
    }

    // ─── Load Machines Dropdown ──────────────────────────────
    async function loadMachines() {
        try {
            const res = await $.getJSON(`${API}/machines`);
            _machines = res || [];
            const sel = $('#bdFilterMachine, #bdMachineId');
            const filterSel = $('#bdFilterMachine');
            const formSel = $('#bdMachineId');
            filterSel.find('option:gt(0)').remove();
            formSel.empty().append('<option value="">-- Select Machine --</option>');
            _machines.forEach(m => {
                filterSel.append(`<option value="${m.machineId}">${m.machineName} (${m.machineCode})</option>`);
                formSel.append(`<option value="${m.machineId}">${m.machineName} (${m.machineCode})</option>`);
            });
        } catch (err) {
            console.error('Failed to load machines', err);
        }
    }

    // ─── Refresh ─────────────────────────────────────────────
    async function refresh() {
        const machineId = $('#bdFilterMachine').val();
        const status = $('#bdFilterStatus').val();
        let url = `${API}/breakdowns?`;
        if (machineId) url += `machineId=${machineId}&`;
        if (status) url += `status=${encodeURIComponent(status)}&`;

        try {
            _breakdowns = await $.getJSON(url);
            renderTable();
            renderStats();
        } catch (err) {
            console.error('Failed to load breakdowns', err);
            $('#bdTableBody').html('<tr><td colspan="9" class="prod-empty"><i class="bi bi-exclamation-triangle"></i>Failed to load data</td></tr>');
        }
    }

    // ─── Render Stats ────────────────────────────────────────
    function renderStats() {
        const open = _breakdowns.filter(b => b.breakdownStatus === 'Open' || b.breakdownStatus === 'Assigned').length;
        const inProgress = _breakdowns.filter(b => b.breakdownStatus === 'In Progress').length;
        const resolved = _breakdowns.filter(b => b.breakdownStatus === 'Resolved' || b.breakdownStatus === 'Closed').length;
        const withDowntime = _breakdowns.filter(b => b.downtimeMinutes > 0);
        const avgDowntime = withDowntime.length > 0
            ? (withDowntime.reduce((s, b) => s + b.downtimeMinutes, 0) / withDowntime.length).toFixed(0)
            : '—';

        $('#bdStatOpen').text(open);
        $('#bdStatInProgress').text(inProgress);
        $('#bdStatResolved').text(resolved);
        $('#bdStatAvgDowntime').text(avgDowntime);
    }

    // ─── Render Table ────────────────────────────────────────
    function renderTable() {
        const body = $('#bdTableBody');
        if (!_breakdowns.length) {
            body.html('<tr><td colspan="9" class="prod-empty"><i class="bi bi-tools"></i>No breakdowns found</td></tr>');
            return;
        }

        body.html(_breakdowns.map(b => {
            const sevClass = getSeverityClass(b.severityLevel);
            const statusClass = getStatusClass(b.breakdownStatus);
            const startStr = b.breakdownStartTime ? new Date(b.breakdownStartTime).toLocaleString('en-IN', { dateStyle: 'short', timeStyle: 'short' }) : '—';
            const dt = b.downtimeMinutes ? `${Number(b.downtimeMinutes).toFixed(0)} min` : '—';

            let actions = `<button class="btn btn-sm btn-ghost-primary" onclick="ProdBreakdown.edit(${b.breakdownId})" title="Edit"><i class="bi bi-pencil"></i></button>`;
            if (b.breakdownStatus !== 'Resolved' && b.breakdownStatus !== 'Closed') {
                actions += ` <button class="btn btn-sm btn-ghost-success" onclick="ProdBreakdown.resolve(${b.breakdownId})" title="Resolve"><i class="bi bi-check-circle"></i></button>`;
            }
            if (b.breakdownStatus === 'Resolved') {
                actions += ` <button class="btn btn-sm btn-ghost-secondary" onclick="ProdBreakdown.close(${b.breakdownId})" title="Close"><i class="bi bi-lock"></i></button>`;
            }
            actions += ` <button class="btn btn-sm btn-ghost-danger" onclick="ProdBreakdown.remove(${b.breakdownId})" title="Delete"><i class="bi bi-trash"></i></button>`;

            return `<tr>
                <td><strong>${esc(b.machineName)}</strong><br><small class="text-muted">${esc(b.machineCode)}</small></td>
                <td>${esc(b.faultCode || '—')}<br><small class="text-muted">${truncate(b.faultDescription, 40)}</small></td>
                <td>${esc(b.faultCategory || '—')}</td>
                <td><span class="badge ${sevClass}">${esc(b.severityLevel || '—')}</span></td>
                <td>${startStr}</td>
                <td>${dt}</td>
                <td>${esc(b.technicianName || '—')}</td>
                <td><span class="badge ${statusClass}">${esc(b.breakdownStatus || 'Open')}</span></td>
                <td class="text-end">${actions}</td>
            </tr>`;
        }).join(''));
    }

    // ─── Open Create Modal ───────────────────────────────────
    function openCreateModal() {
        resetForm();
        $('#bdModalTitle').text('Report Breakdown');
        $('#bdEditId').val(0);
        $('#bdStartTime').val(toLocalDatetime(new Date()));
        _modal.show();
    }

    // ─── Edit ────────────────────────────────────────────────
    async function edit(id) {
        try {
            const b = await $.getJSON(`${API}/breakdowns/${id}`);
            resetForm();
            $('#bdModalTitle').text('Edit Breakdown');
            $('#bdEditId').val(b.breakdownId);
            $('#bdMachineId').val(b.machineId);
            $('#bdFaultCode').val(b.faultCode);
            $('#bdFaultCategory').val(b.faultCategory);
            $('#bdSeverityLevel').val(b.severityLevel);
            $('#bdFaultDescription').val(b.faultDescription);
            $('#bdStartTime').val(b.breakdownStartTime ? toLocalDatetime(new Date(b.breakdownStartTime)) : '');
            $('#bdEndTime').val(b.breakdownEndTime ? toLocalDatetime(new Date(b.breakdownEndTime)) : '');
            $('#bdReportedBy').val(b.reportedBy);
            $('#bdTechnicianName').val(b.technicianName);
            $('#bdRootCause').val(b.rootCause);
            $('#bdCorrectiveAction').val(b.correctiveAction);
            $('#bdPreventiveAction').val(b.preventiveAction);
            $('#bdSparePartsUsed').val(b.sparePartsUsed);
            $('#bdRepairCost').val(b.repairCost);
            $('#bdRemarks').val(b.remarks);
            _modal.show();
        } catch (err) {
            Swal.fire('Error', 'Failed to load breakdown details', 'error');
        }
    }

    // ─── Save (Create / Update) ──────────────────────────────
    async function save() {
        const machineId = parseInt($('#bdMachineId').val());
        const startTime = $('#bdStartTime').val();
        if (!machineId || !startTime) {
            Swal.fire('Validation', 'Machine and Start Time are required', 'warning');
            return;
        }

        const dto = {
            machineId: machineId,
            faultCode: $('#bdFaultCode').val() || null,
            faultDescription: $('#bdFaultDescription').val() || null,
            faultCategory: $('#bdFaultCategory').val() || null,
            severityLevel: $('#bdSeverityLevel').val() || null,
            breakdownStartTime: startTime,
            breakdownEndTime: $('#bdEndTime').val() || null,
            downtimeMinutes: null,
            breakdownStatus: 'Open',
            reportedBy: $('#bdReportedBy').val() || null,
            technicianId: null,
            technicianName: $('#bdTechnicianName').val() || null,
            rootCause: $('#bdRootCause').val() || null,
            correctiveAction: $('#bdCorrectiveAction').val() || null,
            preventiveAction: $('#bdPreventiveAction').val() || null,
            sparePartsUsed: $('#bdSparePartsUsed').val() || null,
            repairCost: parseFloat($('#bdRepairCost').val()) || null,
            remarks: $('#bdRemarks').val() || null
        };

        // Auto-calculate downtime if both start and end provided
        if (dto.breakdownStartTime && dto.breakdownEndTime) {
            const diff = (new Date(dto.breakdownEndTime) - new Date(dto.breakdownStartTime)) / 60000;
            if (diff > 0) dto.downtimeMinutes = Math.round(diff * 100) / 100;
        }

        const editId = parseInt($('#bdEditId').val());
        const isEdit = editId > 0;

        try {
            if (isEdit) {
                await $.ajax({ url: `${API}/breakdowns/${editId}`, type: 'PUT', contentType: 'application/json', data: JSON.stringify(dto) });
            } else {
                await $.ajax({ url: `${API}/breakdowns`, type: 'POST', contentType: 'application/json', data: JSON.stringify(dto) });
            }
            _modal.hide();
            if (typeof Swal2 !== 'undefined' && Swal2.toast) {
                Swal2.toast(isEdit ? 'Breakdown updated' : 'Breakdown reported', 'success');
            } else {
                Swal.fire({ toast: true, position: 'top-end', icon: 'success', title: isEdit ? 'Breakdown updated' : 'Breakdown reported', showConfirmButton: false, timer: 2000 });
            }
            refresh();
        } catch (err) {
            Swal.fire('Error', 'Failed to save breakdown', 'error');
        }
    }

    // ─── Resolve ─────────────────────────────────────────────
    async function resolve(id) {
        const res = await Swal.fire({ title: 'Resolve Breakdown?', text: 'Mark this breakdown as resolved?', icon: 'question', showCancelButton: true, confirmButtonText: 'Resolve' });
        if (!res.isConfirmed) return;
        try {
            await $.post(`${API}/breakdowns/${id}/resolve`);
            if (typeof Swal2 !== 'undefined' && Swal2.toast) Swal2.toast('Breakdown resolved', 'success');
            refresh();
        } catch (err) {
            Swal.fire('Error', 'Failed to resolve', 'error');
        }
    }

    // ─── Close ───────────────────────────────────────────────
    async function closeBreakdown(id) {
        const res = await Swal.fire({ title: 'Close Breakdown?', text: 'Close this resolved breakdown?', icon: 'question', showCancelButton: true, confirmButtonText: 'Close' });
        if (!res.isConfirmed) return;
        try {
            await $.post(`${API}/breakdowns/${id}/close`);
            if (typeof Swal2 !== 'undefined' && Swal2.toast) Swal2.toast('Breakdown closed', 'success');
            refresh();
        } catch (err) {
            Swal.fire('Error', 'Failed to close', 'error');
        }
    }

    // ─── Delete (soft) ───────────────────────────────────────
    async function remove(id) {
        const res = await Swal.fire({ title: 'Delete Breakdown?', text: 'This will soft-delete the record.', icon: 'warning', showCancelButton: true, confirmButtonText: 'Delete', confirmButtonColor: '#d63939' });
        if (!res.isConfirmed) return;
        try {
            await $.post(`${API}/breakdowns/${id}/delete`);
            if (typeof Swal2 !== 'undefined' && Swal2.toast) Swal2.toast('Breakdown deleted', 'success');
            refresh();
        } catch (err) {
            Swal.fire('Error', 'Failed to delete', 'error');
        }
    }

    // ─── Helpers ─────────────────────────────────────────────
    function resetForm() {
        $('#bdModal .form-control, #bdModal .form-select').val('');
        $('#bdEditId').val(0);
    }

    function toLocalDatetime(d) {
        const pad = n => String(n).padStart(2, '0');
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
    }

    function esc(s) { return s ? $('<span>').text(s).html() : ''; }

    function truncate(s, len) {
        if (!s) return '—';
        return s.length > len ? esc(s.substring(0, len)) + '…' : esc(s);
    }

    function getSeverityClass(sev) {
        switch (sev) {
            case 'Critical': return 'bg-danger';
            case 'High': return 'bg-orange';
            case 'Medium': return 'bg-warning';
            case 'Low': return 'bg-info';
            default: return 'bg-secondary';
        }
    }

    function getStatusClass(st) {
        switch (st) {
            case 'Open': return 'bg-danger-lt text-danger';
            case 'Assigned': return 'bg-blue-lt text-blue';
            case 'In Progress': return 'bg-warning-lt text-warning';
            case 'Resolved': return 'bg-success-lt text-success';
            case 'Closed': return 'bg-secondary-lt text-secondary';
            default: return 'bg-secondary-lt';
        }
    }

    // ─── Public API ──────────────────────────────────────────
    return {
        init: init,
        refresh: refresh,
        openCreateModal: openCreateModal,
        edit: edit,
        save: save,
        resolve: resolve,
        close: closeBreakdown,
        remove: remove
    };
})();
