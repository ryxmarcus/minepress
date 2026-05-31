/**
 * Production Job Allocation — Drag & Drop + AI Suggestions + Manpower + Filters
 * Uses HTML5 Drag and Drop API
 */
const ProdAllocation = (function () {
    'use strict';

    let machines = [];
    let jobs = [];
    let allMachines = [];     // unfiltered
    let allJobs = [];         // unfiltered
    let allocations = {};     // machineId -> [jobId, ...]
    let selectedJobId = null;
    let mpMachineId = null;   // manpower panel machine
    let mpJobId = null;       // manpower panel job
    let mpAssigned = [];      // assigned employees [{employeeId,name,roleCode,shiftCode}]
    let empSearchTimer = null;
    let machineAllocData = {};  // machineId -> [{allocationId, jobId, jobNo, ...}]
    let machineEmpData = {};    // machineId -> [{employeeId, employeeName, roleCode, ...}]
    let machineStatusData = {}; // machineId -> {maintenanceDue, nextMaintenanceDate, ...}
    let manpowerModal = null;   // Bootstrap modal instance

    // ── Init ───────────────────────────────────────────────────
    async function init() {
        loadFilterData();
        await loadMachines();                    // must complete first — it resets allocations{}
        await loadMachineAllocationsData();      // syncs allocations{} from server after machines are ready
        loadJobs();
        loadServerStats();
        const modalEl = document.getElementById('manpowerModal');
        if (modalEl) {
            const ModalClass = (typeof minepress !== 'undefined' && minepress.Modal) || bootstrap.Modal;
            manpowerModal = new ModalClass(modalEl);
        }
    }

    // ── Load Filter Dropdown Data ──────────────────────────────
    async function loadFilterData() {
        try {
            const res = await $.get('/api/production/filter-data');
            const $jt = $('#filterJobType');
            res.jobTypes.forEach(t => $jt.append(`<option value="${escHtml(t.jobtypename)}">${escHtml(t.jobtypename)}</option>`));

            const $mc = $('#filterMachineCategory');
            res.machineCategories.forEach(c => $mc.append(`<option value="${escHtml(c)}">${escHtml(c)}</option>`));

            const $pt = $('#filterProductType');
            res.productTypes.forEach(p => $pt.append(`<option value="${escHtml(p)}">${escHtml(p)}</option>`));
        } catch (e) { console.error('Failed to load filter data', e); }
    }

    // ── Load Machines ──────────────────────────────────────────
    async function loadMachines() {
        try {
            const res = await $.get('/api/production/machines');
            allMachines = res;
            machines = [...allMachines];
            allocations = {};
            machines.forEach(m => { allocations[m.machineId] = []; });
            renderBoard();
            loadMachineStatus();
        } catch (e) {
            console.error('Failed to load machines', e);
            Swal2.error('Failed to load machines');
        }
    }

    // ── Load Unallocated Jobs ──────────────────────────────────
    async function loadJobs(jobType) {
        try {
            const params = new URLSearchParams();
            if (jobType) params.append('jobType', jobType);
            const url = '/api/production/unallocated-jobs' + (params.toString() ? '?' + params.toString() : '');
            const res = await $.get(url);
            allJobs = res;
            jobs = [...allJobs];
            renderPool();
            updateStats();
        } catch (e) {
            console.error('Failed to load jobs', e);
        }
    }

    // ── Load Server Stats ──────────────────────────────────────
    async function loadServerStats() {
        try {
            const res = await $.get('/api/production/stats');
            $('#statManpower').text(res.manpowerAssigned);
            $('#statTodayJobs').text(res.todayJobs);
        } catch (e) { /* silent */ }
    }

    // ── Load Machine Status ────────────────────────────────────
    async function loadMachineAllocationsData() {
        try {
            const res = await $.get('/api/production/machine-allocations');
            machineAllocData = {};
            machineEmpData = {};
            res.forEach(a => {
                // Sync in-memory allocations{} so Save All and Manpower panel see server state
                if (!allocations[a.machineId]) allocations[a.machineId] = [];
                if (!allocations[a.machineId].includes(a.jobId)) allocations[a.machineId].push(a.jobId);
                // Only treat rows with a valid allocationId as "already saved on server"
                // allocationId=0 rows are legacy bad inserts (pre-ValueGeneratedOnAdd) and must be re-saved
                if (!a.allocationId || a.allocationId <= 0) return;
                if (!machineAllocData[a.machineId]) machineAllocData[a.machineId] = [];
                machineAllocData[a.machineId].push(a);
            });
            // Load employees mapped to each machine
            const machineIds = [...new Set(allMachines.map(m => m.machineId))];
            await Promise.all(machineIds.map(async mid => {
                try {
                    const emps = await $.get(`/api/production/machine-employees?machineId=${mid}`);
                    machineEmpData[mid] = emps;
                } catch (e) { machineEmpData[mid] = []; }
            }));
            renderBoard();
        } catch (e) { console.error('Failed to load machine allocations data', e); }
    }

    async function loadMachineStatus() {
        try {
            const res = await $.get('/api/production/machine-status');
            res.forEach(ms => {
                // Store status data for maintenance/breakdown blocking
                machineStatusData[ms.machineId] = ms;

                const lane = document.querySelector(`.prod-lane[data-machine-id="${ms.machineId}"]`);
                if (!lane) return;
                const dot = lane.querySelector('.prod-status-dot');
                if (dot) {
                    const allocated = allocations[ms.machineId] || [];
                    if (ms.hasActiveBreakdown) {
                        dot.className = 'prod-status-dot prod-status-breakdown';
                        dot.title = 'Breakdown — Allocations Blocked';
                    } else if (ms.maintenanceDue) {
                        dot.className = 'prod-status-dot prod-status-maint';
                        dot.title = 'Maintenance Due — Allocations Blocked';
                    } else if (allocated.length > 0) {
                        dot.className = 'prod-status-dot prod-status-running';
                        dot.title = 'Running';
                    } else {
                        dot.className = 'prod-status-dot prod-status-idle';
                        dot.title = 'Idle';
                    }
                }

                // Add/remove maintenance or breakdown overlay on lane
                if (ms.hasActiveBreakdown) {
                    lane.classList.remove('prod-lane-maintenance');
                    lane.classList.add('prod-lane-breakdown');
                } else if (ms.maintenanceDue) {
                    lane.classList.remove('prod-lane-breakdown');
                    lane.classList.add('prod-lane-maintenance');
                } else {
                    lane.classList.remove('prod-lane-maintenance');
                    lane.classList.remove('prod-lane-breakdown');
                }

                // Show/update breakdown info badge in lane header
                const header = lane.querySelector('.prod-lane-header');
                let bdBadge = header ? header.querySelector('.prod-breakdown-badge') : null;
                if (ms.hasActiveBreakdown && ms.activeBreakdown) {
                    const bd = ms.activeBreakdown;
                    const sevClass = bd.severityLevel === 'Critical' ? 'bg-danger-lt text-danger'
                        : bd.severityLevel === 'High' ? 'bg-orange-lt text-orange'
                        : 'bg-warning-lt text-warning';
                    const badgeHtml = `<div class="prod-breakdown-badge mt-1" style="font-size:.68rem;">`
                        + `<span class="badge ${sevClass} me-1"><i class="bi bi-exclamation-triangle-fill me-1"></i>${escHtml(bd.severityLevel || 'Breakdown')}</span>`
                        + `<span class="text-muted">${escHtml(bd.faultCategory || '')}${bd.downtimeMinutes ? ' · ' + bd.downtimeMinutes + ' min' : ''}</span>`
                        + `</div>`;
                    if (bdBadge) {
                        bdBadge.outerHTML = badgeHtml;
                    } else if (header) {
                        const infoDiv = header.querySelector('div:first-child');
                        if (infoDiv) infoDiv.insertAdjacentHTML('beforeend', badgeHtml);
                    }
                } else if (bdBadge) {
                    bdBadge.remove();
                }
            });
        } catch (e) { /* silent */ }
    }

    // ── Filters ────────────────────────────────────────────────
    function applyFilters() {
        const jobSearch = ($('#filterJobSearch').val() || '').toLowerCase();
        const jobType = $('#filterJobType').val() || '';
        const productType = $('#filterProductType').val() || '';
        const machineName = ($('#filterMachineName').val() || '').toLowerCase();
        const machineCategory = $('#filterMachineCategory').val() || '';
        const priority = $('#filterPriority').val() || '';

        // When job type filter changes, reload jobs from server (filters via trn_job_item)
        if (jobType !== _lastJobTypeFilter) {
            _lastJobTypeFilter = jobType;
            loadJobs(jobType || undefined).then(() => {
                applyClientFilters(jobSearch, productType, priority, machineName, machineCategory);
            });
            return;
        }

        applyClientFilters(jobSearch, productType, priority, machineName, machineCategory);
    }

    let _lastJobTypeFilter = '';

    function applyClientFilters(jobSearch, productType, priority, machineName, machineCategory) {
        // Filter jobs client-side (search, product type, priority)
        jobs = allJobs.filter(j => {
            if (jobSearch && !(j.jobNo || '').toLowerCase().includes(jobSearch) &&
                !(j.productName || '').toLowerCase().includes(jobSearch) &&
                !(j.partyName || '').toLowerCase().includes(jobSearch)) return false;
            if (productType && (j.productName || '') !== productType) return false;
            if (priority && (j.priority || '') !== priority) return false;
            return true;
        });

        // Filter machines
        machines = allMachines.filter(m => {
            if (machineName && !(m.machineName || '').toLowerCase().includes(machineName)) return false;
            if (machineCategory && (m.machineCategory || '') !== machineCategory) return false;
            return true;
        });

        renderBoard();
        renderPool();
        updateStats();
        loadMachineStatus();
    }

    function clearFilters() {
        $('#filterJobSearch').val('');
        $('#filterJobType').val('');
        $('#filterProductType').val('');
        $('#filterMachineName').val('');
        $('#filterMachineCategory').val('');
        $('#filterPriority').val('');
        _lastJobTypeFilter = '';
        loadJobs(); // Reload all jobs (no job type filter)
        machines = [...allMachines];
        renderBoard();
        renderPool();
        updateStats();
        loadMachineStatus();
    }

    // ── Update Stats ───────────────────────────────────────────
    function updateStats() {
        $('#statTotalMachines').text(machines.length);

        let allocatedCount = 0;
        Object.values(allocations).forEach(arr => { if (arr.length > 0) allocatedCount++; });
        $('#statRunning').text(allocatedCount);
        $('#statIdle').text(machines.length - allocatedCount);

        const poolJobs = getPoolJobIds();
        $('#statUnallocated').text(poolJobs.length);
        $('#poolCount').text(poolJobs.length);
    }

    // ── Get pool job IDs (not allocated to any machine) ────────
    function getPoolJobIds() {
        const allocatedIds = new Set();
        Object.values(allocations).forEach(arr => arr.forEach(id => allocatedIds.add(id)));
        return jobs.filter(j => !allocatedIds.has(j.jobId)).map(j => j.jobId);
    }

    // ── Render Swim Lane Board ─────────────────────────────────
    function renderBoard() {
        const $board = $('#machineBoard');
        $board.empty();

        machines.forEach(m => {
            const allocatedJobs = allocations[m.machineId] || [];
            const serverAllocs = machineAllocData[m.machineId] || [];
            const mappedEmps = machineEmpData[m.machineId] || [];

            // Build allocated jobs info HTML
            let allocInfoHtml = '';
            if (serverAllocs.length > 0) {
                allocInfoHtml = '<div class="prod-alloc-box">';
                allocInfoHtml += `<div class="prod-alloc-box-header"><i class="bi bi-briefcase me-1"></i>Allocated Jobs (${serverAllocs.length})</div>`;
                serverAllocs.forEach(a => {
                    const empChips = (a.employees || []).map(e =>
                        `<span class="prod-emp-chip" title="${escHtml(e.employeeName)} (${escHtml(e.roleCode)})">`
                        + `<i class="bi bi-person-fill"></i>${escHtml((e.employeeName || '').split(' ')[0])}</span>`
                    ).join('');
                    allocInfoHtml += `<div class="prod-alloc-item">`
                        + `<div class="d-flex align-items-center justify-content-between">`
                        + `<span class="prod-alloc-jobno">${escHtml(a.jobNo)}</span>`
                        + `<div class="d-flex align-items-center gap-1">`
                        + `<span class="badge bg-green-lt text-green" style="font-size:.6rem">${escHtml(a.allocationStatus || 'ALLOCATED')}</span>`
                        + `<button class="btn btn-sm btn-ghost-danger py-0 px-1 prod-alloc-delete-btn" onclick="ProdAllocation.deleteAllocatedJob(${a.jobId}, ${a.machineId})" title="Delete Allocation">`
                        + `<i class="bi bi-trash"></i></button>`
                        + `</div>`
                        + `</div>`
                        + (empChips ? `<div class="prod-alloc-emps mt-1">${empChips}</div>` : '')
                        + `</div>`;
                });
                allocInfoHtml += '</div>';
            }

            // Build mapped employees HTML — draggable icons with names
            let empBarHtml = '';
            if (mappedEmps.length > 0) {
                const empChipsList = mappedEmps.slice(0, 4).map(e => {
                    const firstName = (e.employeeName || '?').split(' ')[0];
                    const primaryCls = e.isPrimaryMachine ? ' prod-emp-chip-primary' : '';
                    return `<span class="prod-emp-chip prod-emp-draggable${primaryCls}" draggable="true" data-emp-id="${e.employeeId}" data-emp-name="${escHtml(e.employeeName)}" data-machine-id="${m.machineId}" title="${escHtml(e.employeeName)} — ${escHtml(e.roleCode || 'Operator')} (drag to move)">`
                        + `<i class="bi bi-grip-vertical prod-emp-grip"></i>`
                        + `<i class="bi bi-person-fill"></i>${escHtml(firstName)}`
                        + `<i class="bi bi-x prod-emp-delete-x" onclick="event.stopPropagation();ProdAllocation.deleteManpower(${e.employeeId}, ${m.machineId})" title="Remove"></i>`
                        + `</span>`;
                }).join('');
                const moreCount = mappedEmps.length > 4 ? `<span class="prod-emp-chip prod-emp-chip-more">+${mappedEmps.length - 4} more</span>` : '';
                empBarHtml = `<div class="prod-emp-bar-chips">${empChipsList}${moreCount}</div>`;
            }

            const html = `
                <div class="prod-lane" data-machine-id="${m.machineId}">
                    <div class="prod-lane-header prod-emp-drop-zone" data-machine-id="${m.machineId}">
                        <div>
                            <span class="prod-status-dot prod-status-idle me-2" title="Idle"></span>
                            <span>${escHtml(m.machineName)}</span>
                            <div class="prod-machine-specs mt-1">
                                ${m.maxColors ? `<span class="me-2"><i class="bi bi-palette me-1"></i>${m.maxColors}C</span>` : ''}
                                ${m.maxSpeedPerHour ? `<span class="me-2"><i class="bi bi-speedometer me-1"></i>${m.maxSpeedPerHour}/hr</span>` : ''}
                                ${m.maxSheetLengthMm && m.maxSheetWidthMm ? `<span><i class="bi bi-arrows-angle-expand me-1"></i>${m.maxSheetLengthMm}×${m.maxSheetWidthMm}mm</span>` : ''}
                            </div>
                            ${empBarHtml}
                        </div>
                        <div class="d-flex align-items-center gap-1">
                            <button class="btn btn-ghost-cyan btn-sm py-0 px-1" onclick="ProdAllocation.openManpowerPanel(${m.machineId})" title="Assign Manpower">
                                <i class="bi bi-people"></i>
                            </button>
                            <span class="badge prod-lane-count bg-cyan-lt text-cyan">${mappedEmps.length}</span>
                        </div>
                    </div>
                    ${allocInfoHtml}
                    <div class="prod-lane-body" data-machine-id="${m.machineId}"></div>
                </div>`;
            $board.append(html);
        });

        // Render allocated jobs in lanes
        machines.forEach(m => {
            renderLaneJobs(m.machineId);
        });

        // Setup drop zones
        setupDropZones();

        // Setup employee drag-and-drop between machines
        setupEmployeeDragDrop();
    }

    // ── Render Jobs in a Machine Lane ──────────────────────────
    function renderLaneJobs(machineId) {
        const $body = $(`.prod-lane-body[data-machine-id="${machineId}"]`);
        $body.empty();

        const allocatedJobIds = allocations[machineId] || [];
        if (allocatedJobIds.length === 0) {
            $body.html('<div class="prod-empty"><i class="bi bi-inbox"></i>Drop jobs here</div>');
            return;
        }

        allocatedJobIds.forEach((jobId, idx) => {
            const job = jobs.find(j => j.jobId === jobId) || allJobs.find(j => j.jobId === jobId);
            if (!job) return;
            $body.append(createJobCard(job, idx + 1));
        });

        setupDraggables($body);
    }

    // ── Render Pool ────────────────────────────────────────────
    function renderPool() {
        const $body = $('#jobPoolBody');
        $body.empty();

        const poolJobIds = getPoolJobIds();
        if (poolJobIds.length === 0) {
            $body.html('<div class="prod-empty w-100"><i class="bi bi-check-circle"></i>All jobs allocated!</div>');
            return;
        }

        poolJobIds.forEach(id => {
            const job = jobs.find(j => j.jobId === id);
            if (!job) return;
            $body.append(createJobCard(job, null));
        });

        setupDraggables($body);
    }

    // ── Create Job Card HTML ───────────────────────────────────
    function createJobCard(job, seq) {
        const priClass = getPriorityClass(job.priority);
        const delivery = job.deliveryDate ? new Date(job.deliveryDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short' }) : '—';
        const daysLeft = job.deliveryDate ? Math.ceil((new Date(job.deliveryDate) - new Date()) / 86400000) : null;
        const urgencyBadge = daysLeft !== null && daysLeft <= 2
            ? '<span class="badge bg-danger-lt text-danger ms-1" style="font-size:.6rem">URGENT</span>'
            : daysLeft !== null && daysLeft <= 5
            ? '<span class="badge bg-warning-lt text-warning ms-1" style="font-size:.6rem">SOON</span>'
            : '';

        return `
        <div class="prod-job-card" draggable="true" data-job-id="${job.jobId}">
            ${seq ? `<span class="prod-seq-badge">${seq}</span>` : ''}
            <div class="d-flex align-items-center justify-content-between">
                <span class="prod-job-no">${escHtml(job.jobNo)}</span>
                <span class="badge store-status-badge ${priClass}" style="font-size:.6rem">${escHtml(job.priority || 'Normal')}</span>
            </div>
            <div class="prod-job-product mt-1" title="${escHtml(job.productName || '')}">${escHtml(job.productName || '—')}</div>
            <div class="prod-job-meta mt-1 d-flex justify-content-between">
                <span><i class="bi bi-person me-1"></i>${escHtml(job.partyName || '—')}</span>
                <span><i class="bi bi-calendar-event me-1"></i>${delivery}${urgencyBadge}</span>
            </div>
            <div class="prod-job-meta d-flex justify-content-between">
                <span><i class="bi bi-stack me-1"></i>Qty: ${job.quantity?.toLocaleString('en-IN') || '—'}</span>
                ${job.aiPriorityScore ? `<span><i class="bi bi-robot me-1"></i>AI: ${job.aiPriorityScore}</span>` : ''}
            </div>
            <div class="mt-1 d-flex gap-1">
                <button class="btn btn-sm btn-ghost-primary py-0 px-1" onclick="ProdAllocation.showAiSuggestions(${job.jobId})" title="AI Suggest">
                    <i class="bi bi-stars"></i>
                </button>
                <button class="btn btn-sm btn-ghost-danger py-0 px-1" onclick="ProdAllocation.removeFromMachine(${job.jobId})" title="Remove">
                    <i class="bi bi-x-lg"></i>
                </button>
            </div>
        </div>`;
    }

    // ── Priority CSS Class ─────────────────────────────────────
    function getPriorityClass(priority) {
        if (!priority) return 'prod-priority-normal';
        const p = priority.toLowerCase();
        if (p === 'urgent' || p === 'critical') return 'prod-priority-urgent';
        if (p === 'high') return 'prod-priority-high';
        if (p === 'low') return 'prod-priority-low';
        return 'prod-priority-normal';
    }

    // ── Setup Draggables ───────────────────────────────────────
    function setupDraggables($container) {
        $container.find('.prod-job-card[draggable="true"]').each(function () {
            const el = this;
            el.addEventListener('dragstart', function (e) {
                el.classList.add('dragging');
                e.dataTransfer.setData('text/plain', el.dataset.jobId);
                e.dataTransfer.effectAllowed = 'move';
            });
            el.addEventListener('dragend', function () {
                el.classList.remove('dragging');
                document.querySelectorAll('.prod-lane-body.drag-over').forEach(z => z.classList.remove('drag-over'));
            });
        });
    }

    // ── Setup Drop Zones ───────────────────────────────────────
    function setupDropZones() {
        // Machine lane bodies
        document.querySelectorAll('.prod-lane-body').forEach(zone => {
            zone.addEventListener('dragover', function (e) {
                e.preventDefault();
                e.dataTransfer.dropEffect = 'move';
                this.classList.add('drag-over');
            });
            zone.addEventListener('dragleave', function () {
                this.classList.remove('drag-over');
            });
            zone.addEventListener('drop', function (e) {
                e.preventDefault();
                this.classList.remove('drag-over');
                const jobId = parseInt(e.dataTransfer.getData('text/plain'));
                const machineId = parseInt(this.dataset.machineId);
                allocateJob(jobId, machineId);
            });
        });

        // Pool body
        const poolBody = document.getElementById('jobPoolBody');
        if (poolBody) {
            poolBody.addEventListener('dragover', function (e) {
                e.preventDefault();
                e.dataTransfer.dropEffect = 'move';
            });
            poolBody.addEventListener('drop', function (e) {
                e.preventDefault();
                const jobId = parseInt(e.dataTransfer.getData('text/plain'));
                removeFromMachine(jobId);
            });
        }
    }

    // ── Check if job is already allocated on server ───────────
    function isJobServerAllocated(jobId) {
        for (const mid in machineAllocData) {
            const allocs = machineAllocData[mid] || [];
            if (allocs.some(a => a.jobId === jobId)) return { allocated: true, machineId: parseInt(mid), machineName: allocs.find(a => a.jobId === jobId)?.machineName || mid };
        }
        return { allocated: false };
    }

    // ── Allocate Job to Machine ────────────────────────────────
    function allocateJob(jobId, machineId) {
        // Block allocation to machines with active breakdown
        const ms = machineStatusData[machineId];
        if (ms && ms.hasActiveBreakdown) {
            const machineName = (machines.find(m => m.machineId === machineId) || allMachines.find(m => m.machineId === machineId))?.machineName || 'Machine';
            const bd = ms.activeBreakdown || {};
            Swal.fire({
                title: 'Machine Breakdown',
                html: `<b>${escHtml(machineName)}</b> has an active breakdown and cannot accept new job allocations.<br><br>`
                    + (bd.faultCategory ? `<small class="text-muted">Fault: ${escHtml(bd.faultCategory)}${bd.severityLevel ? ' (' + escHtml(bd.severityLevel) + ')' : ''}</small><br>` : '')
                    + (bd.technicianName ? `<small class="text-muted">Technician: ${escHtml(bd.technicianName)}</small>` : ''),
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        // Block allocation to machines under maintenance
        if (ms && ms.maintenanceDue) {
            const machineName = (machines.find(m => m.machineId === machineId) || allMachines.find(m => m.machineId === machineId))?.machineName || 'Machine';
            Swal.fire({
                title: 'Machine Under Maintenance',
                html: `<b>${escHtml(machineName)}</b> is currently under maintenance and cannot accept new job allocations.<br><br>`
                    + (ms.nextMaintenanceType ? `<small class="text-muted">Maintenance type: ${escHtml(ms.nextMaintenanceType)}</small>` : ''),
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        // Check if job is already allocated on server
        const serverAlloc = isJobServerAllocated(jobId);
        if (serverAlloc.allocated) {
            if (serverAlloc.machineId === machineId) {
                Swal2.toast(`This job is already allocated to this machine`, 'warning');
                return;
            }
            // Moving to different machine — confirm with user
            const job = jobs.find(j => j.jobId === jobId) || allJobs.find(j => j.jobId === jobId);
            const jobNo = job ? job.jobNo : jobId;
            Swal.fire({
                title: 'Job Already Allocated',
                text: `Job ${jobNo} is already allocated to ${serverAlloc.machineName}. Do you want to move it to the new machine? The old allocation and manpower will be removed.`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, Move Job',
                cancelButtonText: 'Cancel'
            }).then(result => {
                if (result.isConfirmed) doAllocateJob(jobId, machineId);
            });
            return;
        }

        doAllocateJob(jobId, machineId);
    }

    function doAllocateJob(jobId, machineId) {
        // Remove from any current machine
        Object.keys(allocations).forEach(mid => {
            allocations[mid] = allocations[mid].filter(id => id !== jobId);
        });

        // Add to target machine
        if (!allocations[machineId]) allocations[machineId] = [];
        allocations[machineId].push(jobId);

        // Re-render
        renderPool();
        machines.forEach(m => renderLaneJobs(m.machineId));
        setupDropZones();
        updateStats();
        loadMachineStatus();

        // Toast
        const job = (jobs.find(j => j.jobId === jobId) || allJobs.find(j => j.jobId === jobId));
        const machine = (machines.find(m => m.machineId === machineId) || allMachines.find(m => m.machineId === machineId));
        if (job && machine) {
            Swal2.toast(`${job.jobNo} → ${machine.machineName}`, 'success');
        }
    }

    // ── Delete Allocated Job (from server + UI) ────────────────
    async function deleteAllocatedJob(jobId, machineId) {
        const result = await Swal.fire({
            title: 'Delete Allocation?',
            text: 'This will remove the job allocation and all assigned manpower from this machine.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d63939',
            confirmButtonText: 'Yes, delete',
            cancelButtonText: 'Cancel'
        });
        if (!result.isConfirmed) return;

        Swal.fire({ title: 'Please wait...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
        try {
            await $.ajax({
                url: '/api/production/deallocate-job',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ jobId: jobId, machineId: machineId })
            });
            Swal2.toast('Allocation deleted successfully', 'success');

            // Immediately purge from machineAllocData so saveAllAllocations filter stays accurate
            if (machineAllocData[machineId]) {
                machineAllocData[machineId] = machineAllocData[machineId].filter(a => a.jobId !== jobId);
            }

            // Also remove from client-side allocations
            const mid = String(machineId);
            if (allocations[mid]) {
                allocations[mid] = allocations[mid].filter(id => id !== jobId);
            }

            loadMachineAllocationsData();
            loadJobs();
            loadServerStats();
        } catch (e) {
            console.error('Delete allocation failed', e);
            Swal2.error('Failed to delete allocation');
        } finally {
            Swal.close();
        }
    }

    // ── Remove Job from Machine (back to pool) ─────────────────
    async function removeFromMachine(jobId) {
        // Check if the job has a server-side allocation that needs cleanup
        const serverAlloc = isJobServerAllocated(jobId);
        if (serverAlloc.allocated) {
            try {
                await $.ajax({
                    url: '/api/production/deallocate-job',
                    method: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ jobId: jobId, machineId: serverAlloc.machineId })
                });
                Swal2.toast('Job deallocated from machine', 'success');

                // Immediately purge from machineAllocData so saveAllAllocations filter stays accurate
                const dMid = serverAlloc.machineId;
                if (machineAllocData[dMid]) {
                    machineAllocData[dMid] = machineAllocData[dMid].filter(a => a.jobId !== jobId);
                }

                loadMachineAllocationsData();
                loadJobs(); // Refresh pool to include deallocated job
                loadServerStats();
            } catch (e) {
                console.error('Deallocate failed', e);
                Swal2.error('Failed to deallocate job from server');
            }
        }

        // Also remove from client-side allocations
        Object.keys(allocations).forEach(mid => {
            allocations[mid] = allocations[mid].filter(id => id !== jobId);
        });

        renderPool();
        machines.forEach(m => renderLaneJobs(m.machineId));
        setupDropZones();
        updateStats();
        loadMachineStatus();
    }

    // ── Move Job Up/Down in Machine Queue ──────────────────────
    function moveJob(jobId, direction) {
        for (const mid in allocations) {
            const arr = allocations[mid];
            const idx = arr.indexOf(jobId);
            if (idx === -1) continue;

            const newIdx = direction === 'up' ? idx - 1 : idx + 1;
            if (newIdx < 0 || newIdx >= arr.length) return;

            // Swap
            [arr[idx], arr[newIdx]] = [arr[newIdx], arr[idx]];
            renderLaneJobs(parseInt(mid));
            setupDropZones();
            return;
        }
    }

    // ── Save All Allocations ───────────────────────────────────
    async function saveAllAllocations() {
        const items = [];
        Object.entries(allocations).forEach(([mid, jobIds]) => {
            const machineId = parseInt(mid);
            // Only send jobs NOT already saved on the server for this machine
            const serverJobIds = new Set((machineAllocData[machineId] || []).map(a => a.jobId));
            jobIds.forEach(jid => {
                if (!serverJobIds.has(jid)) {
                    items.push({ jobId: jid, machineId });
                }
            });
        });

        if (items.length === 0) {
            Swal2.toast('No new allocations to save', 'info');
            return;
        }

        Swal.fire({ title: 'Please wait...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
        try {
            const res = await $.ajax({
                url: '/api/production/save-allocations',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ allocations: items })
            });
            Swal.close();
            Swal2.toast(res.message || 'Allocations saved!', 'success');
            if (res.skippedCount > 0 && res.skippedJobs && res.skippedJobs.length > 0) {
                Swal2.toast(`Skipped: ${res.skippedJobs.join(', ')}`, 'info');
            }
            loadServerStats();
            loadMachineAllocationsData();
            loadJobs(); // Refresh pool to remove newly allocated jobs
        } catch (e) {
            Swal.close();
            console.error('Save failed', e);
            const resp = e?.responseJSON;
            const msg = resp?.message || e?.statusText || 'Failed to save allocations';
            const detail = resp?.detail || resp?.errors || '';
            console.error('Server error detail:', detail);
            Swal2.error(msg + (detail ? `\n${JSON.stringify(detail)}` : ''));
        }
    }

    // ── Manpower Panel ─────────────────────────────────────────
    function openManpowerPanel(machineId, jobId) {
        // Block if machine has active breakdown
        const ms = machineStatusData[machineId];
        if (ms && ms.hasActiveBreakdown) {
            const machineName = (machines.find(m => m.machineId === machineId) || allMachines.find(m => m.machineId === machineId))?.machineName || 'Machine';
            Swal.fire({
                title: 'Machine Breakdown',
                html: `<b>${escHtml(machineName)}</b> has an active breakdown. Manpower cannot be assigned until the breakdown is resolved.`,
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        // Block if machine is under maintenance
        if (ms && ms.maintenanceDue) {
            const machineName = (machines.find(m => m.machineId === machineId) || allMachines.find(m => m.machineId === machineId))?.machineName || 'Machine';
            Swal.fire({
                title: 'Machine Under Maintenance',
                html: `<b>${escHtml(machineName)}</b> is under maintenance. Manpower cannot be assigned until maintenance is complete.`,
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        mpMachineId = machineId;
        mpAssigned = [];

        // Pick first allocated job for this machine or use provided jobId
        const machineJobs = allocations[machineId] || [];
        const serverJobs = (machineAllocData[machineId] || []).map(a => a.jobId);
        mpJobId = jobId || machineJobs[0] || serverJobs[0] || null;

        const machine = (machines.find(m => m.machineId === machineId) || allMachines.find(m => m.machineId === machineId));
        const job = mpJobId ? (jobs.find(j => j.jobId === mpJobId) || allJobs.find(j => j.jobId === mpJobId)) : null;

        $('#mpMachineName').text(machine ? machine.machineName : '—');
        $('#mpJobNo').text(job ? job.jobNo : '(no job)');
        $('#mpEmployeeList').html('<div class="prod-empty"><i class="bi bi-search"></i>Search to find employees</div>');
        $('#mpAssignedList').html('<div class="prod-empty"><i class="bi bi-hourglass-split"></i>Loading mapped employees…</div>');
        $('#mpDeptFilter').val('');
        $('#mpEmpSearch').val('');

        // Show modal
        if (manpowerModal) manpowerModal.show();

        // Load employees already mapped to this machine from mst_employee_machine_mapping
        loadMachineEmployees();
    }

    async function loadMachineEmployees() {
        if (!mpMachineId) return;
        try {
            const mappings = await $.get(`/api/production/machine-employees?machineId=${mpMachineId}`);
            if (mappings.length > 0) {
                mpAssigned = mappings.map(m => ({
                    employeeId: m.employeeId,
                    name: m.employeeName || m.employeeCode,
                    employeeCode: m.employeeCode,
                    roleCode: m.roleCode || 'Operator',
                    shiftCode: 'GENERAL',
                    skillLevel: m.skillLevel,
                    isPrimary: m.isPrimaryMachine,
                    experienceYears: m.experienceYears,
                    fromMapping: true
                }));
                renderAssignedList();
            } else {
                // No mapping found — try loading from existing job-manpower allocation
                if (mpJobId) await loadExistingManpower();
            }
        } catch (e) {
            // Fallback to job-level manpower
            if (mpJobId) await loadExistingManpower();
        }
    }

    async function loadExistingManpower() {
        if (!mpMachineId || !mpJobId) return;
        try {
            const res = await $.get(`/api/production/manpower?machineId=${mpMachineId}&jobId=${mpJobId}`);
            mpAssigned = res.map(m => ({
                employeeId: m.employeeId,
                name: m.employeeName || m.employeeCode,
                employeeCode: m.employeeCode,
                roleCode: m.roleCode,
                shiftCode: m.shiftCode
            }));
            renderAssignedList();
        } catch (e) { /* silent */ }
    }

    function closeManpowerPanel() {
        if (manpowerModal) manpowerModal.hide();
        mpMachineId = null;
        mpJobId = null;
        mpAssigned = [];
    }

    async function searchEmployees() {
        const dept = $('#mpDeptFilter').val() || '';
        const search = ($('#mpEmpSearch').val() || '').trim();

        if (!dept && search.length < 2) {
            $('#mpEmployeeList').html('<div class="prod-empty"><i class="bi bi-search"></i>Select department or type 2+ characters</div>');
            return;
        }

        clearTimeout(empSearchTimer);
        empSearchTimer = setTimeout(async () => {
            try {
                const params = new URLSearchParams();
                if (dept) params.append('dept', dept);
                if (search) params.append('search', search);
                const res = await $.get(`/api/production/employees?${params.toString()}`);
                renderEmployeeList(res);
            } catch (e) {
                $('#mpEmployeeList').html('<div class="prod-empty"><i class="bi bi-exclamation-triangle"></i>Failed to load</div>');
            }
        }, 300);
    }

    function renderEmployeeList(employees) {
        const $list = $('#mpEmployeeList');
        $list.empty();

        if (employees.length === 0) {
            $list.html('<div class="prod-empty"><i class="bi bi-emoji-neutral"></i>No employees found</div>');
            return;
        }

        const assignedIds = new Set(mpAssigned.map(a => a.employeeId));
        const mappedIds = new Set(mpAssigned.filter(a => a.fromMapping).map(a => a.employeeId));

        employees.forEach(emp => {
            const isAssigned = assignedIds.has(emp.employeeId);
            const isMapped = mappedIds.has(emp.employeeId);
            const mappedBadge = isMapped
                ? '<span class="badge bg-cyan-lt text-cyan ms-1" style="font-size:.6rem;">Mapped</span>'
                : '';
            $list.append(`
                <div class="prod-emp-item ${isAssigned ? 'prod-emp-assigned' : ''}" data-emp-id="${emp.employeeId}">
                    <div class="d-flex align-items-center gap-2 flex-fill">
                        <span class="avatar avatar-xs" style="background:var(--tblr-primary-lt)">
                            ${escHtml((emp.firstName || '?')[0])}
                        </span>
                        <div>
                            <div class="fw-medium" style="font-size:.82rem;">${escHtml(emp.fullName)}${mappedBadge}</div>
                            <div class="text-muted" style="font-size:.7rem;">${escHtml(emp.empCode || '')} · ${escHtml(emp.deptName || '')}</div>
                        </div>
                    </div>
                    ${isAssigned
                        ? '<span class="badge bg-green-lt text-green" style="font-size:.65rem;">Assigned</span>'
                        : `<button class="btn btn-sm btn-ghost-primary py-0 px-1" onclick="ProdAllocation.assignEmployee(${emp.employeeId}, '${escHtml(emp.fullName)}', '${escHtml(emp.empCode || '')}')">
                               <i class="bi bi-plus-lg"></i>
                           </button>`
                    }
                </div>`);
        });
    }

    async function assignEmployee(empId, name, code) {
        // Check if already in current assignment list
        if (mpAssigned.some(a => a.employeeId === empId)) {
            Swal2.toast(`${name} is already assigned to this machine`, 'warning');
            return;
        }

        // Server-side duplicate check across ALL machines
        try {
            const dupCheck = await $.get(`/api/production/check-manpower-duplicate?employeeId=${empId}&machineId=${mpMachineId}`);
            if (dupCheck.isDuplicate) {
                if (dupCheck.sameMachine) {
                    Swal2.toast(dupCheck.message, 'warning');
                    return;
                }
                // On another machine — ask user whether to move
                const result = await Swal.fire({
                    title: 'Employee Already Assigned',
                    html: `<b>${escHtml(name)}</b> is currently assigned to <b>${escHtml(dupCheck.machineName)}</b> as <b>${escHtml(dupCheck.roleName)}</b>.<br><br>`
                        + `Do you want to <b>move</b> them to this machine, or <b>cancel</b>?`,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Move to This Machine',
                    cancelButtonText: 'Cancel'
                });
                if (!result.isConfirmed) return;

                // Find the source machine ID from machineEmpData
                let fromMachineId = null;
                for (const mid in machineEmpData) {
                    if ((machineEmpData[mid] || []).some(e => e.employeeId === empId)) {
                        fromMachineId = parseInt(mid);
                        break;
                    }
                }
                if (fromMachineId) {
                    Swal.fire({ title: 'Please wait...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
                    try {
                        const moveRes = await $.ajax({
                            url: '/api/production/move-manpower',
                            method: 'POST',
                            contentType: 'application/json',
                            data: JSON.stringify({ employeeId: empId, fromMachineId: fromMachineId, toMachineId: mpMachineId })
                        });
                        Swal2.toast(moveRes.message || `${name} moved successfully`, 'success');
                        loadMachineAllocationsData();
                        loadServerStats();
                        // Refresh manpower panel
                        loadMachineEmployees();
                        return;
                    } catch (e) {
                        console.error('Move manpower failed', e);
                        const errMsg = e.responseJSON?.message || 'Failed to move employee';
                        Swal2.error(errMsg);
                        return;
                    } finally {
                        Swal.close();
                    }
                }
            }
        } catch (e) {
            console.error('Duplicate check failed', e);
        }

        const role = $('#mpRoleFilter').val() || 'Operator';
        const shift = $('#mpShiftFilter').val() || 'GENERAL';
        mpAssigned.push({ employeeId: empId, name, employeeCode: code, roleCode: role, shiftCode: shift });
        renderAssignedList();
        // Refresh available list to show assigned state
        searchEmployees();
    }

    async function unassignEmployee(empId) {
        const emp = mpAssigned.find(a => a.employeeId === empId);
        const empName = emp ? emp.name : 'Employee';

        // Confirm removal
        const result = await Swal.fire({
            title: 'Remove Employee?',
            text: `Remove ${empName} from this machine? This will also remove the machine mapping and all job manpower allocations.`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d63939',
            confirmButtonText: 'Yes, Remove',
            cancelButtonText: 'Cancel'
        });
        if (!result.isConfirmed) return;

        Swal.fire({ title: 'Please wait...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
        // Call server to remove from mst_employee_machine_mapping + trn_job_machine_manpower_allocation
        if (mpMachineId) {
            try {
                const res = await $.ajax({
                    url: '/api/production/remove-manpower',
                    method: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ employeeId: empId, machineId: mpMachineId })
                });
                Swal2.toast(res.message || `${empName} removed`, 'success');
            } catch (e) {
                console.error('Remove manpower failed', e);
                Swal.close();
                Swal2.error('Failed to remove employee from machine');
                return;
            }
        }
        Swal.close();

        // Remove from local list
        mpAssigned = mpAssigned.filter(a => a.employeeId !== empId);
        renderAssignedList();
        searchEmployees();

        // Refresh machine data
        loadMachineAllocationsData();
        loadServerStats();
    }

    function renderAssignedList() {
        const $list = $('#mpAssignedList');
        $list.empty();

        if (mpAssigned.length === 0) {
            $list.html('<div class="prod-empty"><i class="bi bi-inbox"></i>No employees assigned</div>');
            return;
        }

        mpAssigned.forEach(emp => {
            const primaryBadge = emp.isPrimary
                ? '<span class="badge bg-primary-lt text-primary ms-1" style="font-size:.6rem;">PRIMARY</span>'
                : '';
            const skillBadge = emp.skillLevel
                ? `<span class="badge bg-cyan-lt text-cyan ms-1" style="font-size:.6rem;">${escHtml(emp.skillLevel)}</span>`
                : '';
            const expText = emp.experienceYears
                ? ` · ${emp.experienceYears}yr`
                : '';
            const mappingIndicator = emp.fromMapping
                ? '<i class="bi bi-link-45deg text-success me-1" title="Machine-mapped employee" style="font-size:.7rem;"></i>'
                : '';

            $list.append(`
                <div class="prod-emp-item">
                    <div class="d-flex align-items-center gap-2 flex-fill">
                        <span class="avatar avatar-xs" style="background:var(--tblr-green-lt)">
                            ${escHtml((emp.name || '?')[0])}
                        </span>
                        <div>
                            <div class="fw-medium" style="font-size:.82rem;">
                                ${mappingIndicator}${escHtml(emp.name)}${primaryBadge}${skillBadge}
                            </div>
                            <div class="text-muted" style="font-size:.7rem;">${escHtml(emp.roleCode)} · ${escHtml(emp.shiftCode)}${expText}</div>
                        </div>
                    </div>
                    <button class="btn btn-sm btn-ghost-danger py-0 px-1" onclick="ProdAllocation.unassignEmployee(${emp.employeeId})">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>`);
        });
    }

    async function saveManpowerAllocation() {
        if (!mpMachineId) { Swal2.toast('No machine selected', 'warning'); return; }
        if (!mpJobId) { Swal2.toast('No job allocated to this machine', 'warning'); return; }
        if (mpAssigned.length === 0) { Swal2.toast('No employees assigned', 'warning'); return; }

        Swal.fire({ title: 'Please wait...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
        try {
            const payload = {
                jobId: mpJobId,
                machineId: mpMachineId,
                employees: mpAssigned.map(a => ({
                    employeeId: a.employeeId,
                    roleCode: a.roleCode,
                    shiftCode: a.shiftCode
                }))
            };

            const res = await $.ajax({
                url: '/api/production/save-manpower',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload)
            });

            Swal.close();
            Swal2.toast(res.message || 'Manpower saved!', 'success');
            closeManpowerPanel();
            loadServerStats();
            loadMachineAllocationsData();
        } catch (e) {
            Swal.close();
            console.error('Save manpower failed', e);
            const msg = e?.responseJSON?.message || 'Failed to save manpower allocation';
            Swal2.error(msg);
        }
    }

    // ── AI Suggestions ─────────────────────────────────────────
    async function showAiSuggestions(jobId) {
        selectedJobId = jobId;
        const $panel = $('#aiSuggestionPanel');
        const $body = $('#aiSuggestionBody');
        $panel.show();
        $body.html('<div class="text-center p-3"><div class="spinner-border spinner-border-sm text-primary"></div> Analyzing job specifications…</div>');

        try {
            const res = await $.get(`/api/production/ai-suggestions/${jobId}`);
            $body.empty();

            // Job specs header
            $body.append(`
                <div class="p-2 border-bottom" style="font-size:.78rem">
                    <strong>${escHtml(res.jobNo)}</strong> — ${escHtml(res.productName || '—')}
                    <div class="text-muted mt-1">
                        ${res.specs.sheetLength ? `Sheet: ${res.specs.sheetLength}×${res.specs.sheetWidth}mm` : ''}
                        ${res.specs.gsm ? ` | GSM: ${res.specs.gsm}` : ''}
                        ${res.specs.colors ? ` | Colors: ${res.specs.colors}` : ''}
                        ${res.specs.printSide ? ` | Side: ${res.specs.printSide}` : ''}
                    </div>
                </div>`);

            if (res.suggestions.length === 0) {
                $body.append('<div class="prod-empty"><i class="bi bi-emoji-neutral"></i>No machines found</div>');
                return;
            }

            res.suggestions.forEach(s => {
                const scoreClass = s.confidence === 'High' ? 'prod-ai-score-high'
                    : s.confidence === 'Medium' ? 'prod-ai-score-medium' : 'prod-ai-score-low';
                const badgeClass = s.confidence === 'High' ? 'bg-green-lt text-green'
                    : s.confidence === 'Medium' ? 'bg-azure-lt text-azure' : 'bg-orange-lt text-orange';

                $body.append(`
                    <div class="prod-ai-suggestion" onclick="ProdAllocation.allocateFromAi(${jobId}, ${s.machineId})">
                        <div class="prod-ai-score ${scoreClass}">${s.score}</div>
                        <div class="flex-fill">
                            <div class="d-flex align-items-center gap-2">
                                <strong style="font-size:.82rem">${escHtml(s.machineName)}</strong>
                                <span class="prod-ai-badge ${badgeClass}">${s.confidence}</span>
                                ${!s.isCompatible ? '<span class="prod-ai-badge bg-danger-lt text-danger">Incompatible</span>' : ''}
                            </div>
                            <div class="prod-ai-reasons mt-1">
                                ${s.reasons.map(r => `<span class="d-inline-block me-2">${escHtml(r)}</span>`).join('')}
                            </div>
                            <div class="prod-machine-specs mt-1">
                                ${s.maxColors ? `${s.maxColors}C` : ''} 
                                ${s.maxSpeedPerHour ? `| ${s.maxSpeedPerHour}/hr` : ''} 
                                ${s.hourlyRunningCost ? `| ₹${s.hourlyRunningCost}/hr` : ''}
                                ${s.maxSheetLengthMm && s.maxSheetWidthMm ? ` | ${s.maxSheetLengthMm}×${s.maxSheetWidthMm}mm` : ''}
                            </div>
                        </div>
                        <div>
                            <button class="btn btn-sm btn-primary py-1 px-2" title="Assign">
                                <i class="bi bi-arrow-right"></i>
                            </button>
                        </div>
                    </div>`);
            });
        } catch (e) {
            $body.html('<div class="prod-empty"><i class="bi bi-exclamation-triangle"></i>Failed to load suggestions</div>');
        }
    }

    function allocateFromAi(jobId, machineId) {
        allocateJob(jobId, machineId);
        $('#aiSuggestionPanel').hide();
    }

    function closeAiPanel() {
        $('#aiSuggestionPanel').hide();
        selectedJobId = null;
    }

    // ── Employee Drag & Drop Between Machines ─────────────────
    function setupEmployeeDragDrop() {
        // Make employee chips draggable
        document.querySelectorAll('.prod-emp-draggable').forEach(chip => {
            chip.addEventListener('dragstart', function (e) {
                e.stopPropagation(); // Don't trigger job card drag
                e.dataTransfer.setData('application/emp-move', JSON.stringify({
                    empId: parseInt(chip.dataset.empId),
                    empName: chip.dataset.empName,
                    fromMachineId: parseInt(chip.dataset.machineId)
                }));
                e.dataTransfer.effectAllowed = 'move';
                chip.classList.add('prod-emp-dragging');
            });
            chip.addEventListener('dragend', function () {
                chip.classList.remove('prod-emp-dragging');
                document.querySelectorAll('.prod-emp-drop-highlight').forEach(z => z.classList.remove('prod-emp-drop-highlight'));
            });
        });

        // Make lane headers accept employee drops
        document.querySelectorAll('.prod-emp-drop-zone').forEach(zone => {
            zone.addEventListener('dragover', function (e) {
                // Only accept employee drops (check dataTransfer types)
                if (e.dataTransfer.types.includes('application/emp-move')) {
                    e.preventDefault();
                    e.dataTransfer.dropEffect = 'move';
                    this.classList.add('prod-emp-drop-highlight');
                }
            });
            zone.addEventListener('dragleave', function () {
                this.classList.remove('prod-emp-drop-highlight');
            });
            zone.addEventListener('drop', async function (e) {
                this.classList.remove('prod-emp-drop-highlight');
                const data = e.dataTransfer.getData('application/emp-move');
                if (!data) return;
                e.preventDefault();
                e.stopPropagation();

                const { empId, empName, fromMachineId } = JSON.parse(data);
                const toMachineId = parseInt(this.dataset.machineId);

                if (fromMachineId === toMachineId) {
                    Swal2.toast(`${empName} is already on this machine`, 'info');
                    return;
                }

                await moveManpower(empId, empName, fromMachineId, toMachineId);
            });
        });
    }

    async function moveManpower(empId, empName, fromMachineId, toMachineId) {
        const fromMachine = (machines.find(m => m.machineId === fromMachineId) || allMachines.find(m => m.machineId === fromMachineId));
        const toMachine = (machines.find(m => m.machineId === toMachineId) || allMachines.find(m => m.machineId === toMachineId));

        // Check target breakdown status
        const ms = machineStatusData[toMachineId];
        if (ms && ms.hasActiveBreakdown) {
            Swal.fire({
                title: 'Machine Breakdown',
                html: `<b>${escHtml(toMachine?.machineName || '—')}</b> has an active breakdown and cannot accept manpower.`,
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        // Check target maintenance status
        if (ms && ms.maintenanceDue) {
            Swal.fire({
                title: 'Machine Under Maintenance',
                html: `<b>${escHtml(toMachine?.machineName || '—')}</b> is under maintenance and cannot accept manpower.`,
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        const result = await Swal.fire({
            title: 'Move Employee?',
            html: `Move <b>${escHtml(empName)}</b> from <b>${escHtml(fromMachine?.machineName || '—')}</b> to <b>${escHtml(toMachine?.machineName || '—')}</b>?`
                + `<br><br><small class="text-muted">This will update the machine mapping and reassign job allocations.</small>`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, Move',
            cancelButtonText: 'Cancel'
        });
        if (!result.isConfirmed) return;

        Swal.fire({ title: 'Please wait...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
        try {
            const res = await $.ajax({
                url: '/api/production/move-manpower',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ employeeId: empId, fromMachineId, toMachineId })
            });
            Swal2.toast(res.message || `${empName} moved successfully`, 'success');
            loadMachineAllocationsData();
            loadServerStats();
        } catch (e) {
            console.error('Move manpower failed', e);
            const errMsg = e.responseJSON?.message || 'Failed to move employee';
            Swal2.error(errMsg);
        } finally {
            Swal.close();
        }
    }

    async function deleteManpower(empId, machineId) {
        const empData = (machineEmpData[machineId] || []).find(e => e.employeeId === empId);
        const empName = empData?.employeeName || 'Employee';
        const machineName = (machines.find(m => m.machineId === machineId) || allMachines.find(m => m.machineId === machineId))?.machineName || '—';

        const result = await Swal.fire({
            title: 'Remove Employee?',
            html: `Remove <b>${escHtml(empName)}</b> from <b>${escHtml(machineName)}</b>?<br><br><small class="text-muted">This will remove the machine mapping and all job manpower allocations.</small>`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d63939',
            confirmButtonText: 'Yes, Remove',
            cancelButtonText: 'Cancel'
        });
        if (!result.isConfirmed) return;

        Swal.fire({ title: 'Please wait...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
        try {
            const res = await $.ajax({
                url: '/api/production/remove-manpower',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ employeeId: empId, machineId })
            });
            Swal2.toast(res.message || `${empName} removed`, 'success');
            loadMachineAllocationsData();
            loadServerStats();
        } catch (e) {
            console.error('Delete manpower failed', e);
            Swal2.error('Failed to remove employee');
        } finally {
            Swal.close();
        }
    }

    // ── HTML Escape ────────────────────────────────────────────
    function escHtml(str) {
        if (!str) return '';
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    // ── Public API ─────────────────────────────────────────────
    return {
        init,
        showAiSuggestions,
        allocateFromAi,
        closeAiPanel,
        deleteAllocatedJob,
        removeFromMachine,
        moveJob,
        applyFilters,
        clearFilters,
        saveAllAllocations,
        openManpowerPanel,
        closeManpowerPanel,
        searchEmployees,
        assignEmployee,
        unassignEmployee,
        saveManpowerAllocation,
        moveManpower,
        deleteManpower,
        refresh: async function () { await loadMachines(); loadJobs(); loadServerStats(); loadMachineAllocationsData(); }
    };
})();
