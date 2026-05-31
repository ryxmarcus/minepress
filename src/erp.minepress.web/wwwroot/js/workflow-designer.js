/**
 * MinePress ERP — Workflow Designer Engine
 * SVG-based drag-and-drop workflow designer with node palette, connections, and properties panel.
 */
const WF = (() => {
    // ── State ──
    let lookups = {};
    let templates = [];
    let currentTemplateId = null;
    let nodes = [];          // { id, stepCode, stepName, stepType, x, y, color, processId, subProcessId, departmentId, assignedUserId, assignmentRule, approvalTypeId, approvalLevelId, isMandatory, slaHours, escalateAfterHours, escalateTo, notifyVendor, notifySupplier, notifyCustomer, notifyAssignedUser, notifyDeptHead, sendEmail, sendSms, sendWhatsapp, sendPushNotification, nodeColor }
    let connections = [];    // { id, fromId, toId, label, conditionExpression, sequenceNo }
    let selectedNodeId = null;
    let selectedConnId = null;
    let connectMode = { active: false, fromId: null };
    let undoStack = [];
    let redoStack = [];
    let zoom = 1;
    let pan = { x: 0, y: 0 };
    let isPanning = false;
    let panStart = { x: 0, y: 0 };
    let activeTool = 'select'; // 'select' | 'pan'
    let dragNode = null;
    let dragOffset = { x: 0, y: 0 };
    let nextNodeId = 1;
    let nextConnId = 1;

    const NODE_W = 160, NODE_H = 56, CIRCLE_R = 28, DIAMOND_S = 38;
    const API = '/api/workflow';

    // ── Node config ──
    const NODE_CONFIG = {
        START:        { shape: 'circle',  icon: '\uf4f4', biClass: 'bi-play-fill',        defaultColor: '#4CAF50', label: 'Start' },
        END:          { shape: 'circle',  icon: '\uf5a2', biClass: 'bi-stop-fill',        defaultColor: '#607D8B', label: 'End' },
        PROCESS:      { shape: 'rect',    icon: '\uf3e5', biClass: 'bi-gear-fill',        defaultColor: '#2196F3', label: 'Process' },
        APPROVAL:     { shape: 'diamond', icon: '\uf26b', biClass: 'bi-check-circle-fill',defaultColor: '#FF9800', label: 'Approval' },
        TASK:         { shape: 'rect',    icon: '\uf377', biClass: 'bi-list-task',         defaultColor: '#9C27B0', label: 'Task' },
        NOTIFICATION: { shape: 'rect',    icon: '\uf15b', biClass: 'bi-bell-fill',        defaultColor: '#00BCD4', label: 'Notification' },
        DECISION:     { shape: 'diamond', icon: '\uf5aa', biClass: 'bi-signpost-split',   defaultColor: '#F44336', label: 'Decision' }
    };

    // ── Init ──
    function init() {
        loadLookups();
        loadTemplates();
        setupCanvasDragDrop();
        setupPaletteDrag();
        setupCanvasInteraction();
        setupKeyboard();
        setupProcessDropdownChain();

        $('#txtWfSearch, #ddlJobTypeFilter, #ddlProductTypeFilter').on('input change', filterTemplateList);
    }

    // ── API calls ──
    async function loadLookups() {
        try {
            const data = await $.get(`${API}/lookups`);
            lookups = data;
            populateDropdowns();
        } catch (e) {
            console.error('Failed to load lookups', e);
        }
    }

    async function loadTemplates() {
        try {
            templates = await $.get(`${API}/templates`);
            renderTemplateList();
            updateStats();
        } catch (e) {
            $('#wfTemplateGrid').html('<div class="col-12 text-center py-5 text-danger"><i class="bi bi-exclamation-circle me-1"></i>Failed to load workflows.</div>');
        }
    }

    function populateDropdowns() {
        // Job Type
        const jobOpts = '<option value="">— Select —</option>' +
            (lookups.jobTypes || []).map(j => `<option value="${j.id}">${j.name}</option>`).join('');
        $('#propJobType, #ddlJobTypeFilter').each(function () {
            const isFilter = $(this).attr('id').includes('Filter');
            $(this).html(isFilter ? '<option value="">All Job Types</option>' + (lookups.jobTypes || []).map(j => `<option value="${j.id}">${j.name}</option>`).join('') : jobOpts);
        });

        // Product Type
        const prodOpts = '<option value="">— Select —</option>' +
            (lookups.productTypes || []).map(p => `<option value="${p.id}">${p.name}</option>`).join('');
        $('#propProductType, #ddlProductTypeFilter').each(function () {
            const isFilter = $(this).attr('id').includes('Filter');
            $(this).html(isFilter ? '<option value="">All Product Types</option>' + (lookups.productTypes || []).map(p => `<option value="${p.id}">${p.name}</option>`).join('') : prodOpts);
        });

        // Processes
        $('#propProcess').html('<option value="">— None —</option>' +
            (lookups.processes || []).map(p => `<option value="${p.id}">${p.name}</option>`).join(''));

        // Departments
        $('#propDepartment').html('<option value="">— Select —</option>' +
            (lookups.departments || []).map(d => `<option value="${d.id}">${d.name}</option>`).join(''));

        // Users
        $('#propAssignedUser').html('<option value="">— Auto Assign —</option>' +
            (lookups.users || []).map(u => `<option value="${u.id}">${u.name}</option>`).join(''));

        // Approval Types
        $('#propApprovalType').html('<option value="">— None —</option>' +
            (lookups.approvalTypes || []).map(a => `<option value="${a.id}">${a.name}</option>`).join(''));

        // Approval Levels
        $('#propApprovalLevel').html('<option value="">— None —</option>' +
            (lookups.approvalLevels || []).map(a => `<option value="${a.id}">${a.name}</option>`).join(''));
    }

    function setupProcessDropdownChain() {
        $('#propProcess').on('change', function () {
            const pid = parseInt($(this).val());
            const subs = (lookups.subProcesses || []).filter(s => s.processId === pid);
            $('#propSubProcess').html('<option value="">— None —</option>' +
                subs.map(s => `<option value="${s.id}">${s.name}</option>`).join(''));

            // Auto-fill department from process
            if (pid) {
                const proc = (lookups.processes || []).find(p => p.id === pid);
                if (proc && proc.departmentId) {
                    $('#propDepartment').val(proc.departmentId);
                }
            }
        });
    }

    // ── Template List ──
    function updateStats() {
        $('#statTotal').text(templates.length);
        $('#statActive').text(templates.filter(t => t.isActive).length);
        $('#statDefault').text(templates.filter(t => t.isDefault).length);
        const totalSteps = templates.reduce((sum, t) => sum + (t.stepCount || 0), 0);
        $('#statSteps').text(totalSteps);
    }

    function renderTemplateList() {
        if (templates.length === 0) {
            $('#wfTemplateGrid').html(`
                <div class="col-12 text-center py-5">
                    <i class="bi bi-diagram-3" style="font-size:3rem; opacity:.3;"></i>
                    <div class="text-muted mt-2">No workflows created yet</div>
                    <button class="btn btn-primary mt-3" onclick="WF.newWorkflow()">
                        <i class="bi bi-plus-lg me-1"></i>Create First Workflow
                    </button>
                </div>`);
            return;
        }
        filterTemplateList();
    }

    function filterTemplateList() {
        const q = ($('#txtWfSearch').val() || '').toLowerCase();
        const jt = $('#ddlJobTypeFilter').val();
        const pt = $('#ddlProductTypeFilter').val();

        let filtered = templates.filter(t => {
            if (q && !t.workflowName.toLowerCase().includes(q) && !t.workflowCode.toLowerCase().includes(q)) return false;
            if (jt && t.jobTypeId != jt) return false;
            if (pt && t.printProductTypeId != pt) return false;
            return true;
        });

        if (filtered.length === 0) {
            $('#wfTemplateGrid').html('<div class="col-12 text-center py-4 text-muted">No matching workflows found.</div>');
            return;
        }

        const html = filtered.map(t => `
            <div class="col-md-4 col-lg-3">
                <div class="card wf-template-card" onclick="WF.openWorkflow(${t.workflowTemplateId})">
                    <div class="card-body">
                        <div class="wf-card-header">
                            <div class="wf-card-icon"><i class="bi bi-diagram-3"></i></div>
                            <div>
                                <div class="wf-card-title">${t.workflowName}</div>
                                <div class="wf-card-code">${t.workflowCode}</div>
                            </div>
                        </div>
                        <div class="wf-card-meta">
                            ${t.jobTypeName ? `<span class="wf-card-meta-item"><i class="bi bi-briefcase"></i>${t.jobTypeName}</span>` : ''}
                            ${t.productTypeName ? `<span class="wf-card-meta-item"><i class="bi bi-box"></i>${t.productTypeName}</span>` : ''}
                            <span class="wf-card-meta-item"><i class="bi bi-layers"></i>${t.stepCount} steps</span>
                            <span class="wf-card-meta-item"><i class="bi bi-link-45deg"></i>${t.connectionCount} links</span>
                            ${t.isDefault ? '<span class="badge bg-warning-lt">Default</span>' : ''}
                        </div>
                        <div class="wf-card-meta mt-1">
                            <span class="wf-card-meta-item"><i class="bi bi-clock"></i>${t.createdOn}</span>
                            <span class="wf-card-meta-item">v${t.version}</span>
                        </div>
                        <div class="wf-card-actions">
                            <button class="btn btn-sm btn-ghost-primary" onclick="event.stopPropagation(); WF.openWorkflow(${t.workflowTemplateId})" title="Edit">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button class="btn btn-sm btn-ghost-info" onclick="event.stopPropagation(); WF.duplicateWorkflow(${t.workflowTemplateId})" title="Duplicate">
                                <i class="bi bi-copy"></i>
                            </button>
                            <button class="btn btn-sm btn-ghost-danger ms-auto" onclick="event.stopPropagation(); WF.deleteWorkflow(${t.workflowTemplateId})" title="Delete">
                                <i class="bi bi-trash"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>`).join('');

        $('#wfTemplateGrid').html(html);
    }

    // ── View toggling ──
    function showTemplateList() {
        $('#wfDesignerView').hide();
        $('#wfListView').show();
        loadTemplates();
    }

    function showDesigner() {
        $('#wfListView').hide();
        $('#wfDesignerView').show();
        renderAllNodes();
        renderAllConnections();
        updateCounts();
        toggleEmptyState();
    }

    // ── New Workflow ──
    function newWorkflow() {
        currentTemplateId = null;
        nodes = [];
        connections = [];
        selectedNodeId = null;
        selectedConnId = null;
        undoStack = [];
        redoStack = [];
        nextNodeId = 1;
        nextConnId = 1;
        zoom = 1;
        pan = { x: 0, y: 0 };

        $('#propWfCode').val('');
        $('#propWfName').val('');
        $('#propWfDesc').val('');
        $('#propJobType').val('');
        $('#propProductType').val('');
        $('#propIsDefault').prop('checked', false);
        $('#wfVersionBadge').text('v1');
        showDesigner();
        showTemplateProperties();
    }

    // ── Open existing workflow ──
    async function openWorkflow(id) {
        try {
            const data = await $.get(`${API}/templates/${id}`);
            currentTemplateId = data.workflowTemplateId;
            nodes = [];
            connections = [];
            nextNodeId = 1;
            nextConnId = 1;

            // Map DB step IDs to our node IDs
            const stepToNodeMap = {};

            (data.steps || []).forEach(s => {
                const nid = 'n' + nextNodeId++;
                stepToNodeMap[s.workflowStepId] = nid;
                nodes.push({
                    id: nid,
                    dbId: s.workflowStepId,
                    stepCode: s.stepCode,
                    stepName: s.stepName,
                    stepType: s.stepType,
                    x: s.canvasX || 100,
                    y: s.canvasY || 100,
                    color: s.nodeColor || NODE_CONFIG[s.stepType]?.defaultColor || '#2196F3',
                    processId: s.processId,
                    subProcessId: s.subProcessId,
                    departmentId: s.departmentId,
                    assignedUserId: s.assignedUserId,
                    assignmentRule: s.assignmentRule,
                    approvalTypeId: s.approvalTypeId,
                    approvalLevelId: s.approvalLevelId,
                    isMandatory: s.isMandatory,
                    slaHours: s.slaHours,
                    escalateAfterHours: s.escalateAfterHours,
                    escalateTo: s.escalateTo,
                    notifyVendor: s.notifyVendor,
                    notifySupplier: s.notifySupplier,
                    notifyCustomer: s.notifyCustomer,
                    notifyAssignedUser: s.notifyAssignedUser,
                    notifyDeptHead: s.notifyDeptHead,
                    sendEmail: s.sendEmail,
                    sendSms: s.sendSms,
                    sendWhatsapp: s.sendWhatsapp,
                    sendPushNotification: s.sendPushNotification
                });
            });

            (data.connections || []).forEach(c => {
                const cid = 'c' + nextConnId++;
                connections.push({
                    id: cid,
                    fromId: stepToNodeMap[c.fromStepId],
                    toId: stepToNodeMap[c.toStepId],
                    label: c.label || '',
                    conditionExpression: c.conditionExpression || '',
                    sequenceNo: c.sequenceNo || 0
                });
            });

            $('#propWfCode').val(data.workflowCode);
            $('#propWfName').val(data.workflowName);
            $('#propWfDesc').val(data.description || '');
            $('#propJobType').val(data.jobTypeId || '');
            $('#propProductType').val(data.printProductTypeId || '');
            $('#propIsDefault').prop('checked', data.isDefault);
            $('#wfVersionBadge').text('v' + data.version);

            showDesigner();
            showTemplateProperties();
            setSaveIndicator(true);
        } catch (e) {
            Swal.fire('Error', 'Failed to load workflow.', 'error');
        }
    }

    // ── Save ──
    async function saveWorkflow() {
        const code = $('#propWfCode').val().trim();
        const name = $('#propWfName').val().trim();

        if (!code || !name) {
            Swal.fire('Validation', 'Workflow Code and Name are required.', 'warning');
            return;
        }

        // Build tempId → stepCode map for connection mapping
        const stepsDto = nodes.map((n, idx) => ({
            tempId: n.id,
            processId: n.processId || null,
            subProcessId: n.subProcessId || null,
            stepCode: n.stepCode,
            stepName: n.stepName,
            stepType: n.stepType,
            sequenceNo: idx + 1,
            departmentId: n.departmentId || null,
            assignedUserId: n.assignedUserId || null,
            assignmentRule: n.assignmentRule || 'AUTO',
            approvalTypeId: n.approvalTypeId || null,
            approvalLevelId: n.approvalLevelId || null,
            isMandatory: n.isMandatory || false,
            slaHours: n.slaHours || null,
            escalateAfterHours: n.escalateAfterHours || null,
            escalateTo: n.escalateTo || null,
            notifyVendor: n.notifyVendor || false,
            notifySupplier: n.notifySupplier || false,
            notifyCustomer: n.notifyCustomer || false,
            notifyAssignedUser: n.notifyAssignedUser || false,
            notifyDeptHead: n.notifyDeptHead || false,
            sendEmail: n.sendEmail || false,
            sendSms: n.sendSms || false,
            sendWhatsapp: n.sendWhatsapp || false,
            sendPushNotification: n.sendPushNotification || false,
            canvasX: n.x,
            canvasY: n.y,
            nodeColor: n.color
        }));

        const connsDto = connections.map((c, idx) => ({
            fromTempId: c.fromId,
            toTempId: c.toId,
            conditionExpression: c.conditionExpression || null,
            label: c.label || null,
            sequenceNo: idx + 1
        }));

        const dto = {
            workflowCode: code,
            workflowName: name,
            description: $('#propWfDesc').val().trim() || null,
            jobTypeId: parseInt($('#propJobType').val()) || null,
            printProductTypeId: parseInt($('#propProductType').val()) || null,
            isDefault: $('#propIsDefault').is(':checked'),
            steps: stepsDto,
            connections: connsDto
        };

        try {
            let res;
            if (currentTemplateId) {
                res = await $.ajax({ url: `${API}/templates/${currentTemplateId}`, method: 'PUT', contentType: 'application/json', data: JSON.stringify(dto) });
            } else {
                res = await $.ajax({ url: `${API}/templates`, method: 'POST', contentType: 'application/json', data: JSON.stringify(dto) });
                currentTemplateId = res.id;
            }
            setSaveIndicator(true);
            Swal.fire({ icon: 'success', title: 'Saved', text: res.message, timer: 1500, showConfirmButton: false });
        } catch (e) {
            const msg = e.responseJSON?.message || 'Failed to save workflow.';
            Swal.fire('Error', msg, 'error');
        }
    }

    // ── Delete ──
    async function deleteWorkflow(id) {
        const result = await Swal.fire({ title: 'Delete Workflow?', text: 'This action cannot be undone.', icon: 'warning', showCancelButton: true, confirmButtonColor: '#dc3545', confirmButtonText: 'Delete' });
        if (!result.isConfirmed) return;
        try {
            await $.ajax({ url: `${API}/templates/${id}`, method: 'DELETE' });
            loadTemplates();
            Swal.fire({ icon: 'success', title: 'Deleted', timer: 1200, showConfirmButton: false });
        } catch (e) {
            Swal.fire('Error', 'Failed to delete.', 'error');
        }
    }

    // ── Duplicate ──
    async function duplicateWorkflow(id) {
        try {
            const res = await $.ajax({ url: `${API}/templates/${id}/duplicate`, method: 'POST' });
            loadTemplates();
            Swal.fire({ icon: 'success', title: 'Duplicated', text: res.message, timer: 1500, showConfirmButton: false });
        } catch (e) {
            Swal.fire('Error', 'Failed to duplicate.', 'error');
        }
    }

    // ── Rendering ──
    function renderAllNodes() {
        $('#wfCanvasGroup .wf-node').remove();
        nodes.forEach(n => renderNode(n));
    }

    function renderAllConnections() {
        $('#wfCanvasGroup .wf-connection').remove();
        connections.forEach(c => renderConnection(c));
    }

    function renderNode(node) {
        const g = document.createElementNS('http://www.w3.org/2000/svg', 'g');
        g.setAttribute('class', `wf-node ${node.id === selectedNodeId ? 'selected' : ''}`);
        g.setAttribute('data-id', node.id);
        g.setAttribute('transform', `translate(${node.x}, ${node.y})`);

        const cfg = NODE_CONFIG[node.stepType] || NODE_CONFIG.PROCESS;
        const color = node.color || cfg.defaultColor;

        if (cfg.shape === 'circle') {
            // Circle shape for START/END
            const circle = svgEl('circle', { cx: 0, cy: 0, r: CIRCLE_R, fill: color, class: 'wf-node-body-circle' });
            g.appendChild(circle);

            const label = svgEl('text', { x: 0, y: 4, class: 'wf-node-label' });
            label.textContent = node.stepName.length > 8 ? node.stepName.substring(0, 8) : node.stepName;
            g.appendChild(label);

            // Ports
            g.appendChild(svgEl('circle', { cx: 0, cy: -CIRCLE_R, r: 5, class: 'wf-port wf-port-top', 'data-port': 'top' }));
            g.appendChild(svgEl('circle', { cx: 0, cy: CIRCLE_R, r: 5, class: 'wf-port wf-port-bottom', 'data-port': 'bottom' }));

        } else if (cfg.shape === 'diamond') {
            // Diamond shape for APPROVAL/DECISION
            const s = DIAMOND_S;
            const diamond = svgEl('polygon', { points: `0,${-s} ${s},0 0,${s} ${-s},0`, fill: color, class: 'wf-node-body-diamond' });
            g.appendChild(diamond);

            const label = svgEl('text', { x: 0, y: 4, class: 'wf-node-label', style: 'font-size:9px' });
            label.textContent = node.stepName.length > 10 ? node.stepName.substring(0, 10) + '..' : node.stepName;
            g.appendChild(label);

            // Ports on tips
            g.appendChild(svgEl('circle', { cx: 0, cy: -s, r: 5, class: 'wf-port wf-port-top', 'data-port': 'top' }));
            g.appendChild(svgEl('circle', { cx: s, cy: 0, r: 5, class: 'wf-port wf-port-right', 'data-port': 'right' }));
            g.appendChild(svgEl('circle', { cx: 0, cy: s, r: 5, class: 'wf-port wf-port-bottom', 'data-port': 'bottom' }));
            g.appendChild(svgEl('circle', { cx: -s, cy: 0, r: 5, class: 'wf-port wf-port-left', 'data-port': 'left' }));

        } else {
            // Rectangle
            const rect = svgEl('rect', { x: -NODE_W / 2, y: -NODE_H / 2, width: NODE_W, height: NODE_H, fill: color, class: 'wf-node-body' });
            g.appendChild(rect);

            // Icon area
            const iconBg = svgEl('rect', { x: -NODE_W / 2, y: -NODE_H / 2, width: 32, height: NODE_H, fill: 'rgba(0,0,0,.15)', rx: 8 });
            g.appendChild(iconBg);

            const iconText = svgEl('text', { x: -NODE_W / 2 + 16, y: 4, class: 'wf-node-icon', 'text-anchor': 'middle' });
            iconText.textContent = getIconChar(node.stepType);
            g.appendChild(iconText);

            const label = svgEl('text', { x: 6, y: -4, class: 'wf-node-label', 'text-anchor': 'start', style: 'font-size:11px' });
            label.textContent = node.stepName.length > 12 ? node.stepName.substring(0, 12) + '..' : node.stepName;
            g.appendChild(label);

            // Sub-label (dept name or step type)
            const sublabel = svgEl('text', { x: 6, y: 10, class: 'wf-node-sublabel', 'text-anchor': 'start' });
            const deptName = node.departmentId ? getDeptName(node.departmentId) : node.stepType;
            sublabel.textContent = deptName.length > 16 ? deptName.substring(0, 16) + '..' : deptName;
            g.appendChild(sublabel);

            // Notification badges
            if (node.sendEmail || node.sendSms || node.sendWhatsapp || node.sendPushNotification) {
                const badge = svgEl('circle', { cx: NODE_W / 2 - 6, cy: -NODE_H / 2 + 6, r: 6, fill: '#00BCD4', stroke: '#fff', 'stroke-width': 1.5 });
                g.appendChild(badge);
                const badgeIcon = svgEl('text', { x: NODE_W / 2 - 6, y: -NODE_H / 2 + 9, 'text-anchor': 'middle', fill: '#fff', style: 'font-size:8px' });
                badgeIcon.textContent = '✉';
                g.appendChild(badgeIcon);
            }

            // Mandatory badge
            if (node.isMandatory) {
                const mb = svgEl('circle', { cx: -NODE_W / 2 + 6, cy: -NODE_H / 2 + 6, r: 5, fill: '#F44336', stroke: '#fff', 'stroke-width': 1.5 });
                g.appendChild(mb);
                const mt = svgEl('text', { x: -NODE_W / 2 + 6, y: -NODE_H / 2 + 9, 'text-anchor': 'middle', fill: '#fff', style: 'font-size:7px; font-weight:bold' });
                mt.textContent = '!';
                g.appendChild(mt);
            }

            // Ports
            g.appendChild(svgEl('circle', { cx: 0, cy: -NODE_H / 2, r: 5, class: 'wf-port wf-port-top', 'data-port': 'top' }));
            g.appendChild(svgEl('circle', { cx: NODE_W / 2, cy: 0, r: 5, class: 'wf-port wf-port-right', 'data-port': 'right' }));
            g.appendChild(svgEl('circle', { cx: 0, cy: NODE_H / 2, r: 5, class: 'wf-port wf-port-bottom', 'data-port': 'bottom' }));
            g.appendChild(svgEl('circle', { cx: -NODE_W / 2, cy: 0, r: 5, class: 'wf-port wf-port-left', 'data-port': 'left' }));
        }

        document.getElementById('wfCanvasGroup').appendChild(g);
        attachNodeEvents(g, node);
    }

    function renderConnection(conn) {
        const from = nodes.find(n => n.id === conn.fromId);
        const to = nodes.find(n => n.id === conn.toId);
        if (!from || !to) return;

        const pts = getConnectionPoints(from, to);
        const g = document.createElementNS('http://www.w3.org/2000/svg', 'g');
        g.setAttribute('class', `wf-connection ${conn.id === selectedConnId ? 'selected' : ''}`);
        g.setAttribute('data-id', conn.id);

        // Curved path
        const dx = pts.x2 - pts.x1;
        const dy = pts.y2 - pts.y1;
        const cx1 = pts.x1 + dx * 0.2;
        const cy1 = pts.y1 + dy * 0.5;
        const cx2 = pts.x1 + dx * 0.8;
        const cy2 = pts.y1 + dy * 0.5;
        const d = `M${pts.x1},${pts.y1} C${cx1},${cy1} ${cx2},${cy2} ${pts.x2},${pts.y2}`;

        const style = conn.conditionExpression ? 'stroke-dasharray: 6 3;' : '';
        const line = svgEl('path', { d: d, class: 'wf-connection-line', style: style });
        g.appendChild(line);

        // Label
        if (conn.label) {
            const mx = (pts.x1 + pts.x2) / 2;
            const my = (pts.y1 + pts.y2) / 2;
            const lbg = svgEl('rect', { x: mx - 30, y: my - 8, width: 60, height: 16, class: 'wf-connection-label-bg' });
            g.appendChild(lbg);
            const lt = svgEl('text', { x: mx, y: my + 3, class: 'wf-connection-label' });
            lt.textContent = conn.label;
            g.appendChild(lt);
        }

        // Insert connections before nodes so nodes render on top
        const canvasGroup = document.getElementById('wfCanvasGroup');
        const firstNode = canvasGroup.querySelector('.wf-node');
        if (firstNode) {
            canvasGroup.insertBefore(g, firstNode);
        } else {
            canvasGroup.appendChild(g);
        }

        $(g).on('click', function (e) {
            e.stopPropagation();
            selectConnection(conn.id);
        });
    }

    function getConnectionPoints(from, to) {
        const cfgFrom = NODE_CONFIG[from.stepType] || NODE_CONFIG.PROCESS;
        const cfgTo = NODE_CONFIG[to.stepType] || NODE_CONFIG.PROCESS;

        // Simple approach: use center-to-center direction and pick appropriate port
        let x1 = from.x, y1 = from.y;
        let x2 = to.x, y2 = to.y;

        // Determine best exit port of from-node
        const angle = Math.atan2(y2 - y1, x2 - x1);
        const deg = angle * 180 / Math.PI;

        if (cfgFrom.shape === 'circle') {
            x1 = from.x + CIRCLE_R * Math.cos(angle);
            y1 = from.y + CIRCLE_R * Math.sin(angle);
        } else if (cfgFrom.shape === 'diamond') {
            if (Math.abs(deg) < 45) { x1 = from.x + DIAMOND_S; y1 = from.y; }
            else if (deg >= 45 && deg < 135) { x1 = from.x; y1 = from.y + DIAMOND_S; }
            else if (deg >= -135 && deg < -45) { x1 = from.x; y1 = from.y - DIAMOND_S; }
            else { x1 = from.x - DIAMOND_S; y1 = from.y; }
        } else {
            if (Math.abs(deg) < 45) { x1 = from.x + NODE_W / 2; y1 = from.y; }
            else if (deg >= 45 && deg < 135) { x1 = from.x; y1 = from.y + NODE_H / 2; }
            else if (deg >= -135 && deg < -45) { x1 = from.x; y1 = from.y - NODE_H / 2; }
            else { x1 = from.x - NODE_W / 2; y1 = from.y; }
        }

        // Target entry port
        const angle2 = Math.atan2(y1 - y2, x1 - x2);
        const deg2 = angle2 * 180 / Math.PI;

        if (cfgTo.shape === 'circle') {
            x2 = to.x + CIRCLE_R * Math.cos(angle2);
            y2 = to.y + CIRCLE_R * Math.sin(angle2);
        } else if (cfgTo.shape === 'diamond') {
            if (Math.abs(deg2) < 45) { x2 = to.x + DIAMOND_S; y2 = to.y; }
            else if (deg2 >= 45 && deg2 < 135) { x2 = to.x; y2 = to.y + DIAMOND_S; }
            else if (deg2 >= -135 && deg2 < -45) { x2 = to.x; y2 = to.y - DIAMOND_S; }
            else { x2 = to.x - DIAMOND_S; y2 = to.y; }
        } else {
            if (Math.abs(deg2) < 45) { x2 = to.x + NODE_W / 2; y2 = to.y; }
            else if (deg2 >= 45 && deg2 < 135) { x2 = to.x; y2 = to.y + NODE_H / 2; }
            else if (deg2 >= -135 && deg2 < -45) { x2 = to.x; y2 = to.y - NODE_H / 2; }
            else { x2 = to.x - NODE_W / 2; y2 = to.y; }
        }

        return { x1, y1, x2, y2 };
    }

    // ── SVG helpers ──
    function svgEl(tag, attrs) {
        const el = document.createElementNS('http://www.w3.org/2000/svg', tag);
        Object.entries(attrs).forEach(([k, v]) => el.setAttribute(k, v));
        return el;
    }

    function getIconChar(stepType) {
        const map = { START: '▶', END: '■', PROCESS: '⚙', APPROVAL: '✓', TASK: '☐', NOTIFICATION: '🔔', DECISION: '◆' };
        return map[stepType] || '•';
    }

    function getDeptName(deptId) {
        const d = (lookups.departments || []).find(x => x.id == deptId);
        return d ? d.name : '';
    }

    // ── Node Events ──
    function attachNodeEvents(g, node) {
        const $g = $(g);

        // Click to select
        $g.on('click', function (e) {
            e.stopPropagation();
            if (connectMode.active) {
                completeConnection(node.id);
                return;
            }
            selectNode(node.id);
        });

        // Mousedown for drag
        $g.on('mousedown', function (e) {
            if ($(e.target).hasClass('wf-port')) return;
            e.stopPropagation();
            const svgPt = getSvgPoint(e);
            dragNode = node;
            dragOffset = { x: svgPt.x - node.x, y: svgPt.y - node.y };
            pushUndo();
        });

        // Port click for connection start
        $g.find('.wf-port').on('mousedown', function (e) {
            e.stopPropagation();
            e.preventDefault();
            startConnection(node.id);
        });
    }

    // ── Canvas Interaction ──
    function setupCanvasInteraction() {
        const svg = document.getElementById('wfCanvas');

        svg.addEventListener('mousemove', function (e) {
            if (dragNode) {
                const pt = getSvgPoint(e);
                dragNode.x = Math.round((pt.x - dragOffset.x) / 10) * 10; // snap to grid
                dragNode.y = Math.round((pt.y - dragOffset.y) / 10) * 10;
                refreshCanvas();
                setSaveIndicator(false);
            } else if (isPanning) {
                const dx = e.clientX - panStart.x;
                const dy = e.clientY - panStart.y;
                panStart = { x: e.clientX, y: e.clientY };
                pan.x += dx;
                pan.y += dy;
                applyTransform();
            } else if (connectMode.active) {
                const pt = getSvgPoint(e);
                drawTempConnection(pt.x, pt.y);
            }
        });

        svg.addEventListener('mouseup', function () {
            dragNode = null;
            if (isPanning) {
                isPanning = false;
                const wrapper = document.getElementById('wfCanvasWrapper');
                wrapper.classList.remove('panning');
                svg.style.cursor = activeTool === 'pan' ? 'grab' : 'default';
            }
        });

        svg.addEventListener('mouseleave', function () {
            dragNode = null;
            if (isPanning) {
                isPanning = false;
                const wrapper = document.getElementById('wfCanvasWrapper');
                wrapper.classList.remove('panning');
                svg.style.cursor = activeTool === 'pan' ? 'grab' : 'default';
            }
        });

        // Click on canvas (deselect)
        svg.addEventListener('click', function (e) {
            if (e.target === svg || e.target.tagName === 'rect' && e.target.getAttribute('fill') === 'url(#grid)') {
                if (connectMode.active) {
                    cancelConnection();
                } else {
                    deselectAll();
                }
            }
        });

        // Pan with middle-button, ctrl+click, or left-click in pan mode
        svg.addEventListener('mousedown', function (e) {
            const isMiddle = e.button === 1;
            const isCtrlClick = e.button === 0 && e.ctrlKey;
            const isPanTool = e.button === 0 && activeTool === 'pan';
            if (isMiddle || isCtrlClick || isPanTool) {
                e.preventDefault();
                isPanning = true;
                panStart = { x: e.clientX, y: e.clientY };
                svg.style.cursor = 'grabbing';
                const wrapper = document.getElementById('wfCanvasWrapper');
                wrapper.classList.add('panning');
            }
        });

        // Zoom with wheel — zoom toward cursor
        svg.addEventListener('wheel', function (e) {
            e.preventDefault();
            const rect = svg.getBoundingClientRect();
            const mx = e.clientX - rect.left;
            const my = e.clientY - rect.top;
            const oldZoom = zoom;
            const delta = e.deltaY > 0 ? -0.05 : 0.05;
            zoom = Math.max(0.3, Math.min(3, zoom + delta));
            // Adjust pan so the point under cursor stays fixed
            pan.x = mx - (mx - pan.x) * (zoom / oldZoom);
            pan.y = my - (my - pan.y) * (zoom / oldZoom);
            applyTransform();
        }, { passive: false });
    }

    function applyTransform() {
        const g = document.getElementById('wfCanvasGroup');
        g.setAttribute('transform', `translate(${pan.x}, ${pan.y}) scale(${zoom})`);
    }

    function getSvgPoint(e) {
        const svg = document.getElementById('wfCanvas');
        const pt = svg.createSVGPoint();
        pt.x = e.clientX;
        pt.y = e.clientY;
        const ctm = document.getElementById('wfCanvasGroup').getScreenCTM().inverse();
        return pt.matrixTransform(ctm);
    }

    // ── Drag & Drop from Palette ──
    function setupPaletteDrag() {
        $(document).on('dragstart', '.wf-palette-item', function (e) {
            e.originalEvent.dataTransfer.setData('text/plain', JSON.stringify({
                stepType: $(this).data('step-type'),
                color: $(this).data('color')
            }));
            e.originalEvent.dataTransfer.effectAllowed = 'copy';
        });
    }

    function setupCanvasDragDrop() {
        const wrapper = document.getElementById('wfCanvasWrapper');

        wrapper.addEventListener('dragover', function (e) {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'copy';
            wrapper.classList.add('drag-over');
        });

        wrapper.addEventListener('dragleave', function () {
            wrapper.classList.remove('drag-over');
        });

        wrapper.addEventListener('drop', function (e) {
            e.preventDefault();
            wrapper.classList.remove('drag-over');

            try {
                const data = JSON.parse(e.dataTransfer.getData('text/plain'));
                const pt = getSvgPointFromClient(e.clientX, e.clientY);
                addNode(data.stepType, pt.x, pt.y, data.color);
            } catch (ex) {
                console.error('Invalid drop data', ex);
            }
        });
    }

    function getSvgPointFromClient(cx, cy) {
        const svg = document.getElementById('wfCanvas');
        const pt = svg.createSVGPoint();
        pt.x = cx;
        pt.y = cy;
        const ctm = document.getElementById('wfCanvasGroup').getScreenCTM().inverse();
        return pt.matrixTransform(ctm);
    }

    // ── Add Node ──
    function addNode(stepType, x, y, color) {
        pushUndo();
        const cfg = NODE_CONFIG[stepType] || NODE_CONFIG.PROCESS;
        const id = 'n' + nextNodeId++;
        const code = stepType + '_' + id.toUpperCase();

        const node = {
            id, stepCode: code, stepName: cfg.label, stepType,
            x: Math.round(x / 10) * 10, y: Math.round(y / 10) * 10,
            color: color || cfg.defaultColor,
            processId: null, subProcessId: null, departmentId: null,
            assignedUserId: null, assignmentRule: 'AUTO',
            approvalTypeId: null, approvalLevelId: null,
            isMandatory: false, slaHours: null, escalateAfterHours: null, escalateTo: null,
            notifyVendor: false, notifySupplier: false, notifyCustomer: false,
            notifyAssignedUser: false, notifyDeptHead: false,
            sendEmail: false, sendSms: false, sendWhatsapp: false, sendPushNotification: false
        };

        nodes.push(node);
        refreshCanvas();
        selectNode(id);
        setSaveIndicator(false);
    }

    // ── Selection ──
    function selectNode(id) {
        selectedNodeId = id;
        selectedConnId = null;
        refreshCanvas();
        showNodeProperties(id);
    }

    function selectConnection(id) {
        selectedConnId = id;
        selectedNodeId = null;
        refreshCanvas();
        showConnectionProperties(id);
    }

    function deselectAll() {
        selectedNodeId = null;
        selectedConnId = null;
        refreshCanvas();
        showTemplateProperties();
    }

    // ── Properties Panel ──
    function showTemplateProperties() {
        $('#wfTemplateProps').show();
        $('#wfNodeProps').hide();
        $('#wfConnProps').hide();
    }

    function showNodeProperties(nodeId) {
        const node = nodes.find(n => n.id === nodeId);
        if (!node) return;

        $('#wfTemplateProps').hide();
        $('#wfConnProps').hide();
        $('#wfNodeProps').show();

        $('#propStepName').val(node.stepName);
        $('#propStepCode').val(node.stepCode);
        $('#propStepType').val(node.stepType);
        $('#propProcess').val(node.processId || '');
        $('#propProcess').trigger('change');
        setTimeout(() => $('#propSubProcess').val(node.subProcessId || ''), 50);
        $('#propDepartment').val(node.departmentId || '');
        $('#propAssignedUser').val(node.assignedUserId || '');
        $('#propAssignmentRule').val(node.assignmentRule || 'AUTO');
        $('#propApprovalType').val(node.approvalTypeId || '');
        $('#propApprovalLevel').val(node.approvalLevelId || '');
        $('#propIsMandatory').prop('checked', node.isMandatory);
        $('#propSlaHours').val(node.slaHours || '');
        $('#propEscalateAfter').val(node.escalateAfterHours || '');
        $('#propEscalateTo').val(node.escalateTo || '');
        $('#propNotifyVendor').prop('checked', node.notifyVendor);
        $('#propNotifySupplier').prop('checked', node.notifySupplier);
        $('#propNotifyCustomer').prop('checked', node.notifyCustomer);
        $('#propNotifyUser').prop('checked', node.notifyAssignedUser);
        $('#propNotifyDeptHead').prop('checked', node.notifyDeptHead);
        $('#propSendEmail').prop('checked', node.sendEmail);
        $('#propSendSms').prop('checked', node.sendSms);
        $('#propSendWhatsapp').prop('checked', node.sendWhatsapp);
        $('#propSendPush').prop('checked', node.sendPushNotification);
        $('#propNodeColor').val(node.color || '#2196F3');

        // Show/hide approval section
        const showApproval = node.stepType === 'APPROVAL' || node.stepType === 'DECISION';
        $('#propApprovalSection').toggle(showApproval);
    }

    function showConnectionProperties(connId) {
        const conn = connections.find(c => c.id === connId);
        if (!conn) return;

        $('#wfTemplateProps').hide();
        $('#wfNodeProps').hide();
        $('#wfConnProps').show();

        $('#propConnLabel').val(conn.label || '');
        $('#propConnCondition').val(conn.conditionExpression || '');
    }

    function applyNodeProps() {
        const node = nodes.find(n => n.id === selectedNodeId);
        if (!node) return;
        pushUndo();

        node.stepName = $('#propStepName').val().trim() || node.stepName;
        node.stepCode = $('#propStepCode').val().trim() || node.stepCode;
        node.stepType = $('#propStepType').val();
        node.processId = parseInt($('#propProcess').val()) || null;
        node.subProcessId = parseInt($('#propSubProcess').val()) || null;
        node.departmentId = parseInt($('#propDepartment').val()) || null;
        node.assignedUserId = parseInt($('#propAssignedUser').val()) || null;
        node.assignmentRule = $('#propAssignmentRule').val();
        node.approvalTypeId = parseInt($('#propApprovalType').val()) || null;
        node.approvalLevelId = parseInt($('#propApprovalLevel').val()) || null;
        node.isMandatory = $('#propIsMandatory').is(':checked');
        node.slaHours = parseFloat($('#propSlaHours').val()) || null;
        node.escalateAfterHours = parseFloat($('#propEscalateAfter').val()) || null;
        node.escalateTo = $('#propEscalateTo').val().trim() || null;
        node.notifyVendor = $('#propNotifyVendor').is(':checked');
        node.notifySupplier = $('#propNotifySupplier').is(':checked');
        node.notifyCustomer = $('#propNotifyCustomer').is(':checked');
        node.notifyAssignedUser = $('#propNotifyUser').is(':checked');
        node.notifyDeptHead = $('#propNotifyDeptHead').is(':checked');
        node.sendEmail = $('#propSendEmail').is(':checked');
        node.sendSms = $('#propSendSms').is(':checked');
        node.sendWhatsapp = $('#propSendWhatsapp').is(':checked');
        node.sendPushNotification = $('#propSendPush').is(':checked');
        node.color = $('#propNodeColor').val();

        refreshCanvas();
        selectNode(node.id);
        setSaveIndicator(false);
    }

    function applyConnProps() {
        const conn = connections.find(c => c.id === selectedConnId);
        if (!conn) return;
        pushUndo();

        conn.label = $('#propConnLabel').val().trim();
        conn.conditionExpression = $('#propConnCondition').val().trim();

        refreshCanvas();
        selectConnection(conn.id);
        setSaveIndicator(false);
    }

    // ── Connections ──
    function startConnection(fromId) {
        connectMode = { active: true, fromId };
        $('#wfConnectIndicator').show();
        $('body').css('cursor', 'crosshair');
    }

    function completeConnection(toId) {
        if (!connectMode.active || !connectMode.fromId) return;
        if (connectMode.fromId === toId) { cancelConnection(); return; }

        // Check for duplicate
        const exists = connections.some(c => c.fromId === connectMode.fromId && c.toId === toId);
        if (exists) { cancelConnection(); return; }

        pushUndo();
        const id = 'c' + nextConnId++;
        connections.push({ id, fromId: connectMode.fromId, toId, label: '', conditionExpression: '', sequenceNo: connections.length + 1 });

        cancelConnection();
        refreshCanvas();
        setSaveIndicator(false);
    }

    function cancelConnection() {
        connectMode = { active: false, fromId: null };
        $('#wfConnectIndicator').hide();
        $('body').css('cursor', '');
        removeTempConnection();
    }

    function drawTempConnection(mx, my) {
        removeTempConnection();
        const from = nodes.find(n => n.id === connectMode.fromId);
        if (!from) return;

        const line = svgEl('line', { x1: from.x, y1: from.y, x2: mx, y2: my, class: 'wf-temp-connection' });
        line.id = 'wfTempConn';
        document.getElementById('wfCanvasGroup').appendChild(line);
    }

    function removeTempConnection() {
        const el = document.getElementById('wfTempConn');
        if (el) el.remove();
    }

    // ── Delete ──
    function deleteSelectedNode() {
        if (!selectedNodeId) return;
        Swal.fire({ title: 'Delete Step?', icon: 'warning', showCancelButton: true, confirmButtonColor: '#dc3545', confirmButtonText: 'Delete' })
            .then(r => {
                if (!r.isConfirmed) return;
                pushUndo();
                connections = connections.filter(c => c.fromId !== selectedNodeId && c.toId !== selectedNodeId);
                nodes = nodes.filter(n => n.id !== selectedNodeId);
                selectedNodeId = null;
                refreshCanvas();
                showTemplateProperties();
                setSaveIndicator(false);
            });
    }

    function deleteSelectedConnection() {
        if (!selectedConnId) return;
        pushUndo();
        connections = connections.filter(c => c.id !== selectedConnId);
        selectedConnId = null;
        refreshCanvas();
        showTemplateProperties();
        setSaveIndicator(false);
    }

    // ── Keyboard ──
    function setupKeyboard() {
        $(document).on('keydown', function (e) {
            if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.tagName === 'SELECT') return;

            if (e.key === 'Escape') {
                if (connectMode.active) cancelConnection();
                else deselectAll();
            }
            if (e.key === 'Delete' || e.key === 'Backspace') {
                if (selectedNodeId) deleteSelectedNode();
                else if (selectedConnId) deleteSelectedConnection();
            }
            if (e.ctrlKey && e.key === 'z') { e.preventDefault(); undo(); }
            if (e.ctrlKey && e.key === 'y') { e.preventDefault(); redo(); }
            if (e.ctrlKey && e.key === 's') { e.preventDefault(); saveWorkflow(); }
            if (e.key === 'h' || e.key === 'H') { setTool('pan'); }
            if (e.key === 'v' || e.key === 'V') { setTool('select'); }
        });
    }

    // ── Undo / Redo ──
    function pushUndo() {
        undoStack.push(JSON.stringify({ nodes, connections }));
        if (undoStack.length > 50) undoStack.shift();
        redoStack = [];
    }

    function undo() {
        if (undoStack.length === 0) return;
        redoStack.push(JSON.stringify({ nodes, connections }));
        const state = JSON.parse(undoStack.pop());
        nodes = state.nodes;
        connections = state.connections;
        refreshCanvas();
        setSaveIndicator(false);
    }

    function redo() {
        if (redoStack.length === 0) return;
        undoStack.push(JSON.stringify({ nodes, connections }));
        const state = JSON.parse(redoStack.pop());
        nodes = state.nodes;
        connections = state.connections;
        refreshCanvas();
        setSaveIndicator(false);
    }

    // ── Tool Toggle ──
    function setTool(tool) {
        activeTool = tool;
        const svg = document.getElementById('wfCanvas');
        const wrapper = document.getElementById('wfCanvasWrapper');
        // Update toolbar button active states
        $('#btnToolSelect').toggleClass('active', tool === 'select');
        $('#btnToolPan').toggleClass('active', tool === 'pan');
        if (tool === 'pan') {
            wrapper.classList.add('pan-mode');
            svg.style.cursor = 'grab';
        } else {
            wrapper.classList.remove('pan-mode');
            wrapper.classList.remove('panning');
            svg.style.cursor = 'default';
        }
    }

    function resetView() {
        zoom = 1;
        pan = { x: 0, y: 0 };
        applyTransform();
    }

    // ── Zoom ──
    function zoomIn() { zoom = Math.min(3, zoom + 0.1); applyTransform(); }
    function zoomOut() { zoom = Math.max(0.3, zoom - 0.1); applyTransform(); }

    function fitCanvas() {
        if (nodes.length === 0) return;
        const minX = Math.min(...nodes.map(n => n.x)) - 100;
        const minY = Math.min(...nodes.map(n => n.y)) - 100;
        const maxX = Math.max(...nodes.map(n => n.x)) + 200;
        const maxY = Math.max(...nodes.map(n => n.y)) + 200;

        const svg = document.getElementById('wfCanvas');
        const svgW = svg.clientWidth;
        const svgH = svg.clientHeight;
        const contentW = maxX - minX;
        const contentH = maxY - minY;

        zoom = Math.min(svgW / contentW, svgH / contentH, 1.5);
        pan = { x: -minX * zoom + (svgW - contentW * zoom) / 2, y: -minY * zoom + (svgH - contentH * zoom) / 2 };
        applyTransform();
    }

    // ── Auto Layout ──
    function autoLayout() {
        if (nodes.length === 0) return;
        pushUndo();

        // Simple top-down layout based on sequence
        const sorted = [...nodes].sort((a, b) => {
            const typeOrder = { START: 0, PROCESS: 1, APPROVAL: 2, TASK: 3, NOTIFICATION: 4, DECISION: 5, END: 6 };
            return (typeOrder[a.stepType] || 1) - (typeOrder[b.stepType] || 1);
        });

        const centerX = 400;
        const startY = 80;
        const gapY = 120;

        sorted.forEach((node, idx) => {
            node.x = centerX;
            node.y = startY + idx * gapY;
        });

        refreshCanvas();
        fitCanvas();
        setSaveIndicator(false);
    }

    // ── Clear Canvas ──
    function clearCanvas() {
        if (nodes.length === 0) return;
        Swal.fire({ title: 'Clear Canvas?', text: 'All steps and connections will be removed.', icon: 'warning', showCancelButton: true, confirmButtonColor: '#dc3545', confirmButtonText: 'Clear' })
            .then(r => {
                if (!r.isConfirmed) return;
                pushUndo();
                nodes = [];
                connections = [];
                selectedNodeId = null;
                selectedConnId = null;
                refreshCanvas();
                showTemplateProperties();
                setSaveIndicator(false);
            });
    }

    // ── AI Suggest ──
    async function aiSuggest() {
        const jobTypeId = parseInt($('#propJobType').val()) || null;

        if (nodes.length > 0) {
            const result = await Swal.fire({ title: 'AI Suggest', text: 'This will replace current steps. Continue?', icon: 'question', showCancelButton: true, confirmButtonText: 'Yes, Generate' });
            if (!result.isConfirmed) return;
        }

        try {
            Swal.fire({ title: 'Generating...', html: '<div class="spinner-border text-primary"></div><div class="mt-2">AI is analyzing processes...</div>', showConfirmButton: false, allowOutsideClick: false });

            const res = await $.ajax({
                url: `${API}/ai-suggest`,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ jobTypeId, printProductTypeId: parseInt($('#propProductType').val()) || null })
            });

            pushUndo();
            nodes = [];
            connections = [];
            nextNodeId = 1;
            nextConnId = 1;

            const suggestions = res.suggestions || [];
            const nodeIds = [];

            suggestions.forEach(s => {
                const id = 'n' + nextNodeId++;
                nodeIds.push(id);
                nodes.push({
                    id, stepCode: s.stepCode, stepName: s.stepName, stepType: s.stepType,
                    x: s.canvasX || 400, y: s.canvasY || 100,
                    color: s.nodeColor || NODE_CONFIG[s.stepType]?.defaultColor || '#2196F3',
                    processId: s.processId || null, subProcessId: s.subProcessId || null,
                    departmentId: s.departmentId || null, assignedUserId: null,
                    assignmentRule: s.assignmentRule || 'AUTO',
                    approvalTypeId: null, approvalLevelId: null,
                    isMandatory: s.isMandatory || false, slaHours: null, escalateAfterHours: null, escalateTo: null,
                    notifyVendor: false, notifySupplier: false,
                    notifyCustomer: s.notifyCustomer || false,
                    notifyAssignedUser: false, notifyDeptHead: false,
                    sendEmail: s.sendEmail || false, sendSms: s.sendSms || false,
                    sendWhatsapp: s.sendWhatsapp || false, sendPushNotification: false
                });
            });

            // Auto-connect in sequence
            for (let i = 0; i < nodeIds.length - 1; i++) {
                connections.push({
                    id: 'c' + nextConnId++, fromId: nodeIds[i], toId: nodeIds[i + 1],
                    label: '', conditionExpression: '', sequenceNo: i + 1
                });
            }

            refreshCanvas();
            fitCanvas();
            setSaveIndicator(false);

            Swal.fire({ icon: 'success', title: 'AI Generated!', text: res.message, timer: 2000, showConfirmButton: false });
        } catch (e) {
            Swal.fire('Error', 'AI suggestion failed.', 'error');
        }
    }

    // ── Quick Templates ──
    function loadQuickTemplate(type) {
        const templates = {
            book: [
                { stepType: 'START', stepName: 'Start', stepCode: 'START' },
                { stepType: 'PROCESS', stepName: 'Design / DTP', stepCode: 'DESIGN' },
                { stepType: 'APPROVAL', stepName: 'Design Approval', stepCode: 'DESIGN_APPROVAL' },
                { stepType: 'PROCESS', stepName: 'Plate Making', stepCode: 'PLATE' },
                { stepType: 'PROCESS', stepName: 'Printing', stepCode: 'PRINTING' },
                { stepType: 'PROCESS', stepName: 'Binding', stepCode: 'BINDING' },
                { stepType: 'PROCESS', stepName: 'Finishing', stepCode: 'FINISHING' },
                { stepType: 'APPROVAL', stepName: 'Quality Check', stepCode: 'QC' },
                { stepType: 'PROCESS', stepName: 'Packing', stepCode: 'PACKING' },
                { stepType: 'END', stepName: 'End', stepCode: 'END' }
            ],
            brochure: [
                { stepType: 'START', stepName: 'Start', stepCode: 'START' },
                { stepType: 'PROCESS', stepName: 'Design', stepCode: 'DESIGN' },
                { stepType: 'APPROVAL', stepName: 'Client Proof', stepCode: 'CLIENT_PROOF' },
                { stepType: 'PROCESS', stepName: 'Plate Making', stepCode: 'PLATE' },
                { stepType: 'PROCESS', stepName: 'Printing', stepCode: 'PRINTING' },
                { stepType: 'PROCESS', stepName: 'Lamination', stepCode: 'LAMINATION' },
                { stepType: 'PROCESS', stepName: 'Finishing', stepCode: 'FINISHING' },
                { stepType: 'END', stepName: 'End', stepCode: 'END' }
            ],
            simple: [
                { stepType: 'START', stepName: 'Start', stepCode: 'START' },
                { stepType: 'PROCESS', stepName: 'Printing', stepCode: 'PRINTING' },
                { stepType: 'PROCESS', stepName: 'Finishing', stepCode: 'FINISHING' },
                { stepType: 'END', stepName: 'End', stepCode: 'END' }
            ]
        };

        const steps = templates[type];
        if (!steps) return;

        pushUndo();
        nodes = [];
        connections = [];
        nextNodeId = 1;
        nextConnId = 1;

        const nodeIds = [];
        steps.forEach((s, i) => {
            const id = 'n' + nextNodeId++;
            nodeIds.push(id);
            const cfg = NODE_CONFIG[s.stepType] || NODE_CONFIG.PROCESS;
            nodes.push({
                id, stepCode: s.stepCode, stepName: s.stepName, stepType: s.stepType,
                x: 400, y: 80 + i * 100,
                color: cfg.defaultColor,
                processId: null, subProcessId: null, departmentId: null,
                assignedUserId: null, assignmentRule: 'AUTO',
                approvalTypeId: null, approvalLevelId: null,
                isMandatory: false, slaHours: null, escalateAfterHours: null, escalateTo: null,
                notifyVendor: false, notifySupplier: false, notifyCustomer: false,
                notifyAssignedUser: false, notifyDeptHead: false,
                sendEmail: false, sendSms: false, sendWhatsapp: false, sendPushNotification: false
            });
        });

        for (let i = 0; i < nodeIds.length - 1; i++) {
            connections.push({ id: 'c' + nextConnId++, fromId: nodeIds[i], toId: nodeIds[i + 1], label: '', conditionExpression: '', sequenceNo: i + 1 });
        }

        refreshCanvas();
        fitCanvas();
        setSaveIndicator(false);
    }

    // ── Utilities ──
    function refreshCanvas() {
        renderAllConnections();
        renderAllNodes();
        updateCounts();
        toggleEmptyState();
    }

    function updateCounts() {
        $('#wfStepCount').text(nodes.length + ' Steps');
        $('#wfConnCount').text(connections.length + ' Connections');
    }

    function toggleEmptyState() {
        $('#wfCanvasEmpty').toggle(nodes.length === 0);
    }

    function setSaveIndicator(saved) {
        if (saved) {
            $('#wfSaveIndicator').html('<i class="bi bi-circle-fill text-success" style="font-size:.5rem;"></i><span class="small text-muted">Saved</span>');
        } else {
            $('#wfSaveIndicator').html('<i class="bi bi-circle-fill text-warning" style="font-size:.5rem;"></i><span class="small text-muted">Unsaved</span>');
        }
    }

    // ── Public API ──
    return {
        init, showTemplateList, newWorkflow, openWorkflow, saveWorkflow,
        deleteWorkflow, duplicateWorkflow, loadTemplates,
        deleteSelectedNode, deleteSelectedConnection,
        applyNodeProps, applyConnProps,
        undo, redo, zoomIn, zoomOut, fitCanvas, autoLayout,
        clearCanvas, aiSuggest, loadQuickTemplate,
        setTool, resetView
    };
})();

$(document).ready(function () {
    WF.init();
});
