const JobWorkflowPipeline = {
    _jobId: 0,
    _loaded: false,
    _activePhase: null,
    _data: null,

    init(jobId) {
        this._jobId = jobId || 0;
        this._loaded = false;
        this._activePhase = null;
        this._data = null;

        const refreshBtn = document.getElementById('btnRefreshPipeline');
        if (refreshBtn && !refreshBtn.dataset.bound) {
            refreshBtn.dataset.bound = '1';
            refreshBtn.addEventListener('click', () => this.load(true));
        }
    },

    async load(force = false) {
        if (!this._jobId) return;
        if (this._loaded && !force) return;

        this.toggleState('loading');

        try {
            const data = await $.get(`/api/workspace/pipeline/job/${this._jobId}`);
            this._data = data;
            this.render(data);
            this._loaded = true;
            this.toggleState('loaded');
        } catch (err) {
            this.toggleState('error', err?.responseJSON?.message || 'Failed to load workflow pipeline.');
        }
    },

    render(data) {
        this.renderProgressStrip(data);
        this.renderPhaseTabs(data.phases || []);
        this.renderPipeline(data.steps || [], data.currentIndex);
    },

    renderProgressStrip(data) {
        const completedEl = document.getElementById('wpmCompletedCount');
        const totalEl = document.getElementById('wpmTotalSteps');
        const pctEl = document.getElementById('wpmProgressPct');
        const fillEl = document.getElementById('wpmProgressFill');

        if (completedEl) completedEl.textContent = data.completedSteps || 0;
        if (totalEl) totalEl.textContent = data.totalSteps || 0;
        if (pctEl) pctEl.textContent = `${data.progressPct || 0}%`;
        if (fillEl) fillEl.style.width = `${data.progressPct || 0}%`;
    },

    renderPhaseTabs(phases) {
        const container = document.getElementById('wpmPhaseTabsInner');
        const tabsWrapper = document.getElementById('wpmPhaseTabs');
        if (!container || !tabsWrapper) return;

        if (!phases || phases.length === 0) {
            tabsWrapper.classList.add('d-none');
            return;
        }

        tabsWrapper.classList.remove('d-none');

        const html = phases.map((phase, idx) => {
            const isComplete = phase.completedCount === phase.stepCount && phase.stepCount > 0;
            const isActive = phase.hasCurrentStep || (this._activePhase === idx);
            const classes = [
                'wpm-phase-tab',
                isComplete ? 'complete' : '',
                isActive ? 'active' : ''
            ].filter(Boolean).join(' ');

            return `
                <div class="${classes}" data-phase="${idx}" onclick="JobWorkflowPipeline.scrollToPhase(${idx})">
                    <span>${this.esc(phase.name)}</span>
                    <div class="wpm-phase-tab-progress">
                        <span class="wpm-phase-tab-dot"></span>
                        <span class="small">${phase.completedCount}/${phase.stepCount}</span>
                    </div>
                </div>
            `;
        }).join('');

        container.innerHTML = html;

        // Auto-select the phase with current step
        const currentPhase = phases.findIndex(p => p.hasCurrentStep);
        if (currentPhase >= 0) {
            this._activePhase = currentPhase;
        }
    },

    renderPipeline(steps, currentIndex) {
        const container = document.getElementById('wpmPipeline');
        const pipelineWrapper = document.getElementById('wpmPipelineContainer');
        const legend = document.getElementById('wpmLegend');

        if (!container || !pipelineWrapper) return;

        if (!steps || steps.length === 0) {
            container.innerHTML = `
                <div class="text-center py-4 text-muted">
                    <i class="bi bi-inbox fs-1 mb-2 d-block"></i>
                    <div>No workflow steps found for this job.</div>
                </div>
            `;
            pipelineWrapper.classList.remove('d-none');
            if (legend) legend.classList.add('d-none');
            return;
        }

        pipelineWrapper.classList.remove('d-none');
        if (legend) legend.classList.remove('d-none');

        const html = steps.map((step, idx) => {
            const statusClass = this.getStatusClass(step.status, step.isCurrent);
            const isOverdue = step.isOverdue ? 'overdue' : '';
            const isCurrent = step.isCurrent ? 'current' : '';
            const itemBadge = step.isPerItemStep && step.itemTaskCount > 0
                ? `<span class="wpm-item-badge" title="${step.itemTaskCount} item(s)">${step.itemTaskCount}</span>`
                : '';

            return `
                <div class="wpm-step ${statusClass} ${isOverdue} ${isCurrent}" data-step-index="${idx}">
                    ${idx < steps.length - 1 ? '<div class="wpm-step-connector"></div>' : ''}
                    <div class="wpm-step-circle" title="${this.esc(step.processName)}">
                        <i class="bi ${this.esc(step.icon || 'bi-circle')}"></i>
                        ${itemBadge}
                        ${this.renderTooltip(step)}
                    </div>
                    <div class="wpm-step-info">
                        <div class="wpm-step-name">${this.esc(step.shortName || step.processName)}</div>
                        <div class="wpm-step-status">${this.esc(step.statusLabel)}</div>
                    </div>
                </div>
            `;
        }).join('');

        container.innerHTML = html;

        // Scroll to current step after render
        setTimeout(() => this.scrollToCurrent(currentIndex), 100);
    },

    renderTooltip(step) {
        const rows = [
            { label: 'Process', value: step.processName },
            { label: 'Status', value: step.statusLabel },
            { label: 'Department', value: step.department || '-' },
            { label: 'Assigned', value: step.assignedTo || '-' }
        ];

        if (step.completedOn) {
            rows.push({ label: 'Completed', value: step.completedOn });
        }
        if (step.dueDate && !step.isCompleted) {
            rows.push({ label: 'Due', value: step.dueDate });
        }

        const rowsHtml = rows.map(r => `
            <div class="wpm-tooltip-row">
                <span class="wpm-tooltip-label">${this.esc(r.label)}</span>
                <span class="wpm-tooltip-value">${this.esc(r.value)}</span>
            </div>
        `).join('');

        // Per-item breakdown for multi-item production steps
        let itemBreakdownHtml = '';
        if (step.isPerItemStep && step.itemBreakdown && step.itemBreakdown.length > 0) {
            const itemRows = step.itemBreakdown.map(item => {
                const statusClass = this.getStatusClass(item.status, false);
                return `
                    <div class="wpm-tooltip-item-row">
                        <span class="wpm-tooltip-item-dot ${statusClass}"></span>
                        <span class="wpm-tooltip-item-name">${this.esc(item.itemLabel)}</span>
                        <span class="wpm-tooltip-item-status">${this.esc(item.statusLabel)}</span>
                    </div>
                `;
            }).join('');
            itemBreakdownHtml = `
                <div class="wpm-tooltip-divider"></div>
                <div class="wpm-tooltip-items-title">Items (${step.itemBreakdown.length})</div>
                ${itemRows}
            `;
        }

        return `
            <div class="wpm-step-tooltip">
                <div class="wpm-tooltip-title">${this.esc(step.processCode)}</div>
                ${rowsHtml}
                ${itemBreakdownHtml}
            </div>
        `;
    },

    getStatusClass(status, isCurrent) {
        const s = (status || '').toUpperCase();
        const classes = [];

        switch (s) {
            case 'COMPLETED':
                classes.push('completed');
                break;
            case 'APPROVED':
                classes.push('approved');
                break;
            case 'IN_PROGRESS':
                classes.push('in-progress');
                break;
            case 'PENDING':
                classes.push('pending');
                break;
            case 'REJECTED':
                classes.push('rejected');
                break;
            case 'CANCELLED':
                classes.push('cancelled');
                break;
            case 'QUEUED':
            case 'NOT_STARTED':
            default:
                classes.push('waiting');
                break;
        }

        return classes.join(' ');
    },

    scrollToCurrent(currentIndex) {
        if (currentIndex < 0) return;

        const container = document.getElementById('wpmPipelineContainer');
        const pipeline = document.getElementById('wpmPipeline');
        if (!container || !pipeline) return;

        const steps = pipeline.querySelectorAll('.wpm-step');
        if (!steps[currentIndex]) return;

        const step = steps[currentIndex];
        const containerRect = container.getBoundingClientRect();
        const stepRect = step.getBoundingClientRect();

        // Scroll horizontally to center the current step
        const scrollLeft = step.offsetLeft - (container.clientWidth / 2) + (step.clientWidth / 2);
        container.scrollTo({ left: scrollLeft, behavior: 'smooth' });
    },

    scrollToPhase(phaseIndex) {
        if (!this._data || !this._data.phases) return;

        const phase = this._data.phases[phaseIndex];
        if (!phase || !phase.stepIndices || phase.stepIndices.length === 0) return;

        const firstStepIndex = phase.stepIndices[0];
        const pipeline = document.getElementById('wpmPipeline');
        const container = document.getElementById('wpmPipelineContainer');
        if (!pipeline || !container) return;

        const steps = pipeline.querySelectorAll('.wpm-step');
        if (!steps[firstStepIndex]) return;

        const step = steps[firstStepIndex];
        const scrollLeft = step.offsetLeft - 20;
        container.scrollTo({ left: scrollLeft, behavior: 'smooth' });

        // Update active phase tab
        this._activePhase = phaseIndex;
        const tabs = document.querySelectorAll('.wpm-phase-tab');
        tabs.forEach((tab, idx) => {
            tab.classList.toggle('active', idx === phaseIndex);
        });
    },

    toggleState(state, errorText) {
        const loader = document.getElementById('wpmLoader');
        const pipeline = document.getElementById('wpmPipelineContainer');
        const phaseTabs = document.getElementById('wpmPhaseTabs');
        const legend = document.getElementById('wpmLegend');
        const error = document.getElementById('wpmError');
        const errorTextEl = document.getElementById('wpmErrorText');

        // Hide all
        loader?.classList.add('d-none');
        pipeline?.classList.add('d-none');
        phaseTabs?.classList.add('d-none');
        legend?.classList.add('d-none');
        error?.classList.add('d-none');

        if (state === 'loading') {
            loader?.classList.remove('d-none');
        } else if (state === 'loaded') {
            pipeline?.classList.remove('d-none');
            phaseTabs?.classList.remove('d-none');
            legend?.classList.remove('d-none');
        } else if (state === 'error') {
            if (errorTextEl) errorTextEl.textContent = errorText || 'Failed to load workflow pipeline.';
            error?.classList.remove('d-none');
        }
    },

    esc(v) {
        return (v || '').toString()
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#39;');
    }
};