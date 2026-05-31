/**
 * MinePress Agentic AI — Chat Interface
 */
const AgenticAi = (() => {
    'use strict';

    let isProcessing = false;
    let recognition = null;
    let isRecording = false;
    const recentItems = [];

    // ── DOM refs ──
    const el = (id) => document.getElementById(id);

    // ── Init ──
    function init() {
        const input = el('userInput');
        if (input) {
            input.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    send();
                }
            });
            // Auto-resize textarea
            input.addEventListener('input', () => {
                input.style.height = 'auto';
                input.style.height = Math.min(input.scrollHeight, 120) + 'px';
            });
        }

        const channelSelect = el('deliveryChannel');
        if (channelSelect) {
            channelSelect.addEventListener('change', () => {
                const group = el('deliveryAddressGroup');
                if (group) {
                    group.style.display = channelSelect.value ? '' : 'none';
                }
            });
        }

        initSpeechRecognition();
    }

    // ── Send Message ──
    async function send() {
        if (isProcessing) return;

        const input = el('userInput');
        const text = (input?.value || '').trim();
        if (!text) return;

        const outputFormat = el('outputFormat')?.value || 'text';
        const deliveryChannel = el('deliveryChannel')?.value || '';
        const deliveryAddress = el('deliveryAddress')?.value || '';
        const selectedAgent = el('agentSelect')?.value || 'auto';

        // Show user message
        appendUserMessage(text);
        if (selectedAgent !== 'auto') {
            appendSystemMessage(`🎯 Routed to ${selectedAgent}`);
        }
        input.value = '';
        input.style.height = 'auto';

        // Show typing indicator
        const typingId = showTyping();
        setProcessing(true);

        try {
            const payload = {
                inputType: 'text',
                inputData: text,
                outputFormat: outputFormat,
                deliveryChannel: deliveryChannel || null,
                deliveryAddress: deliveryAddress || null,
                selectedAgent: selectedAgent === 'auto' ? null : selectedAgent
            };

            const resp = await fetch('/api/agentic-ai/query', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            removeTyping(typingId);

            const data = await resp.json();

            if (!resp.ok || !data.success) {
                appendBotMessage(data.message || 'Something went wrong. Please try again.', 'error');
                return;
            }

            const aiResp = data.data;
            renderAiResponse(aiResp, outputFormat);
            addRecentActivity(aiResp.intent, aiResp.agent, aiResp.toolExecuted);

        } catch (err) {
            removeTyping(typingId);
            appendBotMessage('Network error. Please check your connection and try again.', 'error');
            console.error('AgenticAi error:', err);
        } finally {
            setProcessing(false);
        }
    }

    // ── Render AI Response ──
    function renderAiResponse(aiResp, requestedFormat) {
        if (aiResp.status === 'clarification_needed') {
            appendBotMessage(aiResp.message || 'I need more information. Could you clarify?', 'warning', aiResp);
            return;
        }

        if (aiResp.status === 'error') {
            appendBotMessage(aiResp.message || 'An error occurred.', 'error', aiResp);
            return;
        }

        const format = requestedFormat || aiResp.outputFormat || 'text';

        // Auto-detect: if data is an array of records, always render as table
        const isArrayData = Array.isArray(aiResp.data) && aiResp.data.length > 0;

        if ((format === 'table' || isArrayData) && aiResp.data) {
            appendBotTable(aiResp);
        } else {
            appendBotMessage(aiResp.message || 'Operation completed.', 'success', aiResp);
            // Also render data as key-value if present (single object)
            if (aiResp.data && typeof aiResp.data === 'object') {
                appendBotDataCard(aiResp.data);
            }
        }

        if (aiResp.deliveryCompleted && aiResp.deliveryChannel) {
            appendSystemMessage(`📤 Response delivered via ${aiResp.deliveryChannel}`);
        }
    }

    // ── Append Messages ──
    function appendUserMessage(text) {
        const container = el('chatMessages');
        const now = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        const html = `
            <div class="ai-msg ai-msg-user">
                <div class="ai-msg-avatar"><i class="bi bi-person-fill"></i></div>
                <div class="ai-msg-content">
                    <div class="ai-msg-bubble">${escapeHtml(text)}</div>
                    <div class="ai-msg-meta">You &bull; ${now}</div>
                </div>
            </div>`;
        container.insertAdjacentHTML('beforeend', html);
        scrollToBottom();
    }

    function appendBotMessage(text, statusType, aiResp) {
        const container = el('chatMessages');
        const now = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        const statusClass = `ai-status-${statusType || 'success'}`;
        const statusIcon = statusType === 'error' ? 'x-circle' :
                          statusType === 'warning' ? 'exclamation-triangle' :
                          'check-circle';

        let agentBadge = '';
        if (aiResp?.agent) {
            agentBadge = `<div class="ai-agent-badge"><i class="bi bi-cpu me-1"></i>${escapeHtml(aiResp.agent)}${aiResp.toolExecuted ? ' → ' + escapeHtml(aiResp.toolExecuted) : ''}</div>`;
        }

        let statusBadge = '';
        if (aiResp?.status) {
            statusBadge = `<span class="ai-status-badge ${statusClass}"><i class="bi bi-${statusIcon}"></i> ${escapeHtml(aiResp.status)}</span>`;
        }

        const shareBar = (statusType === 'success' && aiResp) ? buildShareBar(aiResp) : '';

        const html = `
            <div class="ai-msg ai-msg-bot">
                <div class="ai-msg-avatar"><i class="bi bi-robot"></i></div>
                <div class="ai-msg-content">
                    ${agentBadge}
                    <div class="ai-msg-bubble">
                        <p class="mb-1">${escapeHtml(text)}</p>
                        ${statusBadge}
                    </div>
                    ${shareBar}
                    <div class="ai-msg-meta">MinePress AI &bull; ${now}</div>
                </div>
            </div>`;
        container.insertAdjacentHTML('beforeend', html);
        scrollToBottom();
    }

    function appendBotDataCard(data) {
        const container = el('chatMessages');
        const now = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        let rows = '';
        for (const [key, value] of Object.entries(data)) {
            if (key === 'message') continue;
            if (typeof value === 'object' && value !== null) continue;
            const displayKey = formatPropertyName(key);
            rows += `<tr><td class="fw-semibold">${escapeHtml(displayKey)}</td><td>${escapeHtml(String(value ?? 'N/A'))}</td></tr>`;
        }

        if (!rows) return;

        const html = `
            <div class="ai-msg ai-msg-bot">
                <div class="ai-msg-avatar" style="visibility:hidden;"></div>
                <div class="ai-msg-content">
                    <div class="ai-msg-bubble p-2">
                        <table class="ai-data-table">
                            <thead><tr><th>Field</th><th>Value</th></tr></thead>
                            <tbody>${rows}</tbody>
                        </table>
                    </div>
                </div>
            </div>`;
        container.insertAdjacentHTML('beforeend', html);
        scrollToBottom();
    }

    function appendBotTable(aiResp) {
        const container = el('chatMessages');
        const now = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        const data = aiResp.data;

        let agentBadge = '';
        if (aiResp.agent) {
            agentBadge = `<div class="ai-agent-badge"><i class="bi bi-cpu me-1"></i>${escapeHtml(aiResp.agent)}${aiResp.toolExecuted ? ' → ' + escapeHtml(aiResp.toolExecuted) : ''}</div>`;
        }

        // Try to find array data for table
        let tableData = null;
        if (Array.isArray(data)) {
            tableData = data;
        } else if (typeof data === 'object' && data !== null) {
            // Look for array property
            for (const val of Object.values(data)) {
                if (Array.isArray(val) && val.length > 0 && typeof val[0] === 'object') {
                    tableData = val;
                    break;
                }
            }
        }

        let tableHtml = '';
        if (tableData && tableData.length > 0) {
            const headers = Object.keys(tableData[0]);
            const headerHtml = headers.map(h => `<th>${escapeHtml(formatPropertyName(h))}</th>`).join('');
            const rowsHtml = tableData.map(row => {
                const cells = headers.map(h => `<td>${escapeHtml(String(row[h] ?? 'N/A'))}</td>`).join('');
                return `<tr>${cells}</tr>`;
            }).join('');

            tableHtml = `
                <table class="ai-data-table">
                    <thead><tr>${headerHtml}</tr></thead>
                    <tbody>${rowsHtml}</tbody>
                </table>`;
        } else {
            // Fall back to key-value table
            let rows = '';
            for (const [key, value] of Object.entries(data)) {
                if (key === 'message') continue;
                if (typeof value === 'object' && value !== null) continue;
                rows += `<tr><td class="fw-semibold">${escapeHtml(formatPropertyName(key))}</td><td>${escapeHtml(String(value ?? 'N/A'))}</td></tr>`;
            }
            tableHtml = `
                <table class="ai-data-table">
                    <thead><tr><th>Field</th><th>Value</th></tr></thead>
                    <tbody>${rows}</tbody>
                </table>`;
        }

        const shareBar = buildShareBar(aiResp);

        const html = `
            <div class="ai-msg ai-msg-bot">
                <div class="ai-msg-avatar"><i class="bi bi-robot"></i></div>
                <div class="ai-msg-content">
                    ${agentBadge}
                    <div class="ai-msg-bubble p-2">
                        ${aiResp.message ? `<p class="mb-2">${escapeHtml(aiResp.message)}</p>` : ''}
                        ${tableHtml}
                    </div>
                    ${shareBar}
                    <div class="ai-msg-meta">MinePress AI &bull; ${now}</div>
                </div>
            </div>`;
        container.insertAdjacentHTML('beforeend', html);
        scrollToBottom();
    }

    function appendSystemMessage(text) {
        const container = el('chatMessages');
        const html = `
            <div class="text-center my-2">
                <span class="badge bg-secondary-lt">${escapeHtml(text)}</span>
            </div>`;
        container.insertAdjacentHTML('beforeend', html);
        scrollToBottom();
    }

    // ── Typing Indicator ──
    function showTyping() {
        const container = el('chatMessages');
        const id = 'typing-' + Date.now();
        const html = `
            <div class="ai-msg ai-msg-bot" id="${id}">
                <div class="ai-msg-avatar"><i class="bi bi-robot"></i></div>
                <div class="ai-msg-content">
                    <div class="ai-msg-bubble">
                        <div class="ai-typing">
                            <div class="ai-typing-dot"></div>
                            <div class="ai-typing-dot"></div>
                            <div class="ai-typing-dot"></div>
                        </div>
                    </div>
                </div>
            </div>`;
        container.insertAdjacentHTML('beforeend', html);
        scrollToBottom();
        return id;
    }

    function removeTyping(id) {
        const elem = document.getElementById(id);
        if (elem) elem.remove();
    }

    // ── Speech Recognition ──
    function initSpeechRecognition() {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            const micBtn = el('micBtn');
            if (micBtn) {
                micBtn.disabled = true;
                micBtn.title = 'Speech not supported in this browser';
            }
            return;
        }

        recognition = new SpeechRecognition();
        recognition.continuous = false;
        recognition.interimResults = false;
        recognition.lang = 'en-IN'; // English-India (also picks up Hindi/Hinglish)

        recognition.onresult = (event) => {
            const transcript = event.results[0][0].transcript;
            const input = el('userInput');
            if (input) {
                input.value = transcript;
                input.dispatchEvent(new Event('input'));
            }
            stopSpeech();
        };

        recognition.onerror = (event) => {
            console.warn('Speech recognition error:', event.error);
            stopSpeech();
            if (event.error === 'not-allowed') {
                appendSystemMessage('⚠️ Microphone access denied. Please allow microphone access.');
            }
        };

        recognition.onend = () => {
            if (isRecording) stopSpeech();
        };
    }

    function toggleSpeech() {
        if (isRecording) {
            stopSpeech();
        } else {
            startSpeech();
        }
    }

    function startSpeech() {
        if (!recognition) {
            appendSystemMessage('⚠️ Speech recognition is not supported in this browser.');
            return;
        }

        isRecording = true;
        recognition.start();

        const micBtn = el('micBtn');
        if (micBtn) micBtn.classList.add('recording');

        const indicator = el('speechIndicator');
        if (indicator) indicator.style.display = '';
    }

    function stopSpeech() {
        isRecording = false;
        if (recognition) {
            try { recognition.stop(); } catch (e) { /* ignore */ }
        }

        const micBtn = el('micBtn');
        if (micBtn) micBtn.classList.remove('recording');

        const indicator = el('speechIndicator');
        if (indicator) indicator.style.display = 'none';
    }

    // ── Suggestion Chips ──
    function useSuggestion(text) {
        const input = el('userInput');
        if (input) {
            input.value = text;
            input.dispatchEvent(new Event('input'));
            input.focus();
        }
    }

    // ── Recent Activity ──
    function addRecentActivity(intent, agent, tool) {
        recentItems.unshift({
            intent: intent || 'unknown',
            agent: agent || 'N/A',
            tool: tool || 'N/A',
            time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
        });

        if (recentItems.length > 10) recentItems.pop();

        const list = el('recentActivity');
        if (!list) return;

        list.innerHTML = recentItems.map(item => `
            <div class="list-group-item py-2">
                <div class="d-flex justify-content-between align-items-start">
                    <div>
                        <div class="small fw-semibold">${escapeHtml(formatPropertyName(item.intent))}</div>
                        <div class="text-muted" style="font-size:0.7rem;">
                            <i class="bi bi-cpu me-1"></i>${escapeHtml(item.agent)} → ${escapeHtml(item.tool)}
                        </div>
                    </div>
                    <span class="text-muted" style="font-size:0.65rem;">${item.time}</span>
                </div>
            </div>`).join('');
    }

    // ── Clear Chat ──
    function clearChat() {
        const container = el('chatMessages');
        if (container) {
            const name = window.__aiUserName || 'there';
            container.innerHTML = `
                <div class="ai-msg ai-msg-bot">
                    <div class="ai-msg-avatar"><i class="bi bi-robot"></i></div>
                    <div class="ai-msg-content">
                        <div class="ai-msg-bubble">
                            <p class="mb-2"><strong>Hi ${escapeHtml(name)} 👋, How may I help you?</strong></p>
                            <p class="mb-2">I can help you with your printing ERP operations. Try saying:</p>
                            <div class="ai-suggestions">
                                <button class="ai-suggestion-chip" onclick="AgenticAi.useSuggestion('Show all customers')">
                                    <i class="bi bi-people me-1"></i>Show all customers
                                </button>
                                <button class="ai-suggestion-chip" onclick="AgenticAi.useSuggestion('Show today\'s jobs')">
                                    <i class="bi bi-briefcase me-1"></i>Show today's jobs
                                </button>
                                <button class="ai-suggestion-chip" onclick="AgenticAi.useSuggestion('Show pending invoices')">
                                    <i class="bi bi-receipt me-1"></i>Show pending invoices
                                </button>
                                <button class="ai-suggestion-chip" onclick="AgenticAi.useSuggestion('Show HR summary')">
                                    <i class="bi bi-bar-chart me-1"></i>HR summary
                                </button>
                            </div>
                        </div>
                        <div class="ai-msg-meta">MinePress AI &bull; Just now</div>
                    </div>
                </div>`;
        }
    }

    // ── Share ──
    let _shareCounter = 0;

    function buildShareBar(aiResp) {
        const id = 'share-' + (++_shareCounter);
        const payload = encodeURIComponent(JSON.stringify({
            message: aiResp.message || '',
            data: aiResp.data,
            intent: aiResp.intent,
            agent: aiResp.agent,
            toolExecuted: aiResp.toolExecuted
        }));

        return `
            <div class="ai-share-bar" id="${id}">
                <button class="ai-share-btn" onclick="AgenticAi.copyResult('${id}')" title="Copy to clipboard">
                    <i class="bi bi-clipboard"></i>
                </button>
                <button class="ai-share-btn" onclick="AgenticAi.shareViaEmail('${id}')" title="Share via Email">
                    <i class="bi bi-envelope"></i>
                </button>
                <button class="ai-share-btn" onclick="AgenticAi.shareViaWhatsApp('${id}')" title="Share via WhatsApp">
                    <i class="bi bi-whatsapp"></i>
                </button>
            </div>
            <div class="ai-share-payload" id="${id}-payload" style="display:none;">${payload}</div>`;
    }

    function getSharePayload(shareId) {
        const payloadEl = document.getElementById(shareId + '-payload');
        if (!payloadEl) return null;
        try { return JSON.parse(decodeURIComponent(payloadEl.textContent)); }
        catch { return null; }
    }

    function formatShareContent(payload) {
        let text = payload.message || '';
        if (payload.data) {
            if (Array.isArray(payload.data) && payload.data.length > 0) {
                const headers = Object.keys(payload.data[0]);
                text += '\n\n' + headers.map(formatPropertyName).join(' | ') + '\n';
                text += headers.map(() => '---').join(' | ') + '\n';
                payload.data.forEach(row => {
                    text += headers.map(h => String(row[h] ?? 'N/A')).join(' | ') + '\n';
                });
            } else if (typeof payload.data === 'object') {
                text += '\n';
                for (const [key, value] of Object.entries(payload.data)) {
                    if (typeof value !== 'object' || value === null) {
                        text += `\n${formatPropertyName(key)}: ${String(value ?? 'N/A')}`;
                    }
                }
            }
        }
        return text.trim();
    }

    function copyResult(shareId) {
        const payload = getSharePayload(shareId);
        if (!payload) return;

        const text = formatShareContent(payload);
        navigator.clipboard.writeText(text).then(() => {
            const bar = document.getElementById(shareId);
            if (bar) {
                const btn = bar.querySelector('.ai-share-btn');
                if (btn) {
                    const orig = btn.innerHTML;
                    btn.innerHTML = '<i class="bi bi-check2"></i>';
                    btn.classList.add('ai-share-btn-ok');
                    setTimeout(() => { btn.innerHTML = orig; btn.classList.remove('ai-share-btn-ok'); }, 1500);
                }
            }
        }).catch(() => {
            appendSystemMessage('⚠️ Could not copy to clipboard.');
        });
    }

    function shareViaEmail(shareId) {
        const payload = getSharePayload(shareId);
        if (!payload) return;
        showShareModal('email', payload);
    }

    function shareViaWhatsApp(shareId) {
        const payload = getSharePayload(shareId);
        if (!payload) return;
        showShareModal('whatsapp', payload);
    }

    function showShareModal(channel, payload) {
        // Remove existing modal
        closeShareModal();

        const isEmail = channel === 'email';
        const label = isEmail ? 'Email Address' : 'WhatsApp Number';
        const placeholder = isEmail ? 'user@example.com' : '+91 9876543210';
        const icon = isEmail ? 'envelope' : 'whatsapp';
        const title = isEmail ? 'Share via Email' : 'Share via WhatsApp';

        const modalHtml = `
        <div class="ai-share-overlay" id="aiShareOverlay">
            <div class="ai-share-dialog">
                <div class="ai-share-dialog-header">
                    <h6 class="mb-0"><i class="bi bi-${icon} me-1"></i>${title}</h6>
                    <button type="button" class="btn-close btn-close-sm" onclick="AgenticAi.closeShareModal()"></button>
                </div>
                <div class="ai-share-dialog-body">
                    <div class="mb-3">
                        <label class="form-label small">${label}</label>
                        <input type="${isEmail ? 'email' : 'tel'}" class="form-control form-control-sm" id="aiShareRecipient" placeholder="${placeholder}" required>
                    </div>
                    ${isEmail ? `
                    <div class="mb-3">
                        <label class="form-label small">Subject (optional)</label>
                        <input type="text" class="form-control form-control-sm" id="aiShareSubject" placeholder="MinePress AI Result">
                    </div>` : ''}
                    <div class="mb-2">
                        <label class="form-label small">Preview</label>
                        <div class="border rounded p-2 bg-light" style="max-height:120px;overflow:auto;font-size:0.75rem;white-space:pre-wrap;">${escapeHtml(formatShareContent(payload).substring(0, 500))}</div>
                    </div>
                </div>
                <div class="ai-share-dialog-footer">
                    <button type="button" class="btn btn-sm btn-secondary" onclick="AgenticAi.closeShareModal()">Cancel</button>
                    <button type="button" class="btn btn-sm btn-primary" id="aiShareSendBtn" onclick="AgenticAi.sendShare()">
                        <i class="bi bi-send me-1"></i>Send
                    </button>
                </div>
            </div>
        </div>`;

        document.body.insertAdjacentHTML('beforeend', modalHtml);

        const overlay = document.getElementById('aiShareOverlay');
        overlay._shareChannel = channel;
        overlay._sharePayload = payload;

        // Close on backdrop click
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) closeShareModal();
        });

        // Focus the input
        setTimeout(() => document.getElementById('aiShareRecipient')?.focus(), 100);
    }

    function closeShareModal() {
        const overlay = document.getElementById('aiShareOverlay');
        if (overlay) overlay.remove();
    }

    async function sendShare() {
        const overlay = document.getElementById('aiShareOverlay');
        if (!overlay) return;

        const channel = overlay._shareChannel;
        const payload = overlay._sharePayload;
        const recipient = document.getElementById('aiShareRecipient')?.value?.trim();
        const subject = document.getElementById('aiShareSubject')?.value?.trim() || '';

        if (!recipient) {
            appendSystemMessage('⚠️ Please enter a recipient.');
            return;
        }

        const sendBtn = document.getElementById('aiShareSendBtn');
        if (sendBtn) {
            sendBtn.disabled = true;
            sendBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Sending…';
        }

        try {
            const body = {
                channel: channel,
                recipient: recipient,
                content: formatShareContent(payload),
                subject: subject || null,
                intent: payload.intent || null,
                agent: payload.agent || null
            };

            const resp = await fetch('/api/agentic-ai/share', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });

            const result = await resp.json();

            closeShareModal();

            if (result.success) {
                appendSystemMessage(`✅ ${result.message}`);
            } else {
                appendSystemMessage(`⚠️ ${result.message || 'Failed to share.'}`);
            }
        } catch (err) {
            closeShareModal();
            appendSystemMessage('⚠️ Network error while sharing.');
            console.error('Share error:', err);
        }
    }

    // ── Helpers ──
    function setProcessing(val) {
        isProcessing = val;
        const btn = el('sendBtn');
        const input = el('userInput');
        if (btn) btn.disabled = val;
        if (input) input.disabled = val;
    }

    function scrollToBottom() {
        const container = el('chatMessages');
        if (container) {
            setTimeout(() => container.scrollTop = container.scrollHeight, 50);
        }
    }

    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function formatPropertyName(name) {
        if (!name) return name;
        // camelCase / snake_case → Title Case
        return name
            .replace(/_/g, ' ')
            .replace(/([a-z])([A-Z])/g, '$1 $2')
            .replace(/^./, s => s.toUpperCase());
    }

    // ── Bootstrap ──
    document.addEventListener('DOMContentLoaded', init);

    return {
        send,
        clearChat,
        useSuggestion,
        toggleSpeech,
        stopSpeech,
        copyResult,
        shareViaEmail,
        shareViaWhatsApp,
        sendShare,
        closeShareModal
    };
})();
