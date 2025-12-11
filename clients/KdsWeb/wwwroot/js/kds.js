// Kết nối tới backend SignalR hub /hubs/kds (backend phải bật CORS cho origin của KdsWeb)
const hubUrl = (window.__cfg && window.__cfg.kdsHub) || "/hubs/kds";
const bearer = window.__cfg && window.__cfg.bearer;

const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, bearer ? { accessTokenFactory: () => bearer } : undefined)
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .build();

// Tạo HTML thẻ phiếu theo giao diện mới
function createTicketHTML(ticket) {
    let statusClass = "status-new";
    let statusLabel = "MỚI";
    let btnHtml = `<button onclick="changeStatus('${ticket.id}', 'start')" class="btn btn-outline-primary btn-action"><i class="fas fa-fire me-2"></i>Nấu ngay</button>`;

    if (ticket.status === "InProgress") {
        statusClass = "status-inprogress";
        statusLabel = "ĐANG NẤU";
        btnHtml = `<button onclick="changeStatus('${ticket.id}', 'done')" class="btn btn-outline-success btn-action"><i class="fas fa-check me-2"></i>Hoàn thành</button>`;
    } else if (ticket.status === "Ready") {
        statusClass = "status-ready";
        statusLabel = "ĐÃ XONG";
        btnHtml = `<button onclick="changeStatus('${ticket.id}', 'served')" class="btn btn-dark btn-action"><i class="fas fa-concierge-bell me-2"></i>Đã bê ra</button>`;
    }

    const noteHtml = ticket.note ? `<span class="modifier"><i class="fas fa-comment-dots me-1"></i> ${ticket.note}</span>` : '';

    const createdIso = typeof ticket.createdAt === 'string' ? ticket.createdAt : new Date(ticket.createdAt).toISOString();

    return `
    <div class="ticket-card" id="ticket-${ticket.id}" data-id="${ticket.id}" data-created="${createdIso}">
        <div class="ticket-header ${statusClass}">
            <div>
                <span class="fw-bold fs-5">#${ticket.orderCode || (ticket.orderId ? String(ticket.orderId).substring(0,8) : '---')}</span>
                <div class="small opacity-75">Bàn: ${ticket.tableCode || ticket.tableName || ''}</div>
            </div>
            <div class="text-end">
                <div class="timer-badge">00:00</div>
                <div class="small opacity-75 mt-1">${statusLabel}</div>
            </div>
        </div>
        <div class="ticket-body">
            <div class="order-item">
                 <div class="d-flex justify-content-between">
                    <span>${ticket.itemName}</span>
                    <span class="badge bg-dark rounded-pill">x ${ticket.qty}</span>
                </div>
                ${noteHtml}
            </div>
        </div>
        <div class="ticket-footer">
            ${btnHtml}
        </div>
    </div>`;
}

// Simple toast helpers (non-blocking)
function ensureToastContainer() {
    let t = document.getElementById('kds-toast');
    if (!t) {
        t = document.createElement('div');
        t.id = 'kds-toast';
        t.style.position = 'fixed';
        t.style.top = '10px';
        t.style.right = '10px';
        t.style.zIndex = 9999;
        t.style.padding = '10px 14px';
        t.style.background = 'rgba(0,0,0,0.75)';
        t.style.color = '#fff';
        t.style.borderRadius = '4px';
        t.style.fontFamily = 'sans-serif';
        t.style.boxShadow = '0 2px 6px rgba(0,0,0,0.3)';
        document.body.appendChild(t);
    }
    return t;
}

function showToast(message) {
    const t = ensureToastContainer();
    t.textContent = message;
    t.style.display = 'block';
}

function hideToast() {
    const t = document.getElementById('kds-toast');
    if (t) t.style.display = 'none';
}

// Play a short notification sound using WebAudio API
function playNotificationSound() {
    try {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        if (!AudioContext) return;
        const ctx = new AudioContext();
        const o = ctx.createOscillator();
        const g = ctx.createGain();
        o.type = 'sine';
        o.frequency.value = 880; // A5
        g.gain.value = 0.0015; // low volume
        o.connect(g);
        g.connect(ctx.destination);
        o.start();
        g.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.25);
        setTimeout(() => { try { o.stop(); ctx.close(); } catch (e) { } }, 300);
    } catch (e) {
        console.warn('playNotificationSound error', e);
    }
}

// Backward compatibility (old event names) -> adapt to new container
connection.on("TicketNew", (ticket) => {
    const container = document.getElementById("tickets-container") || document.getElementById("tickets");
    if (!container) return;
    container.insertAdjacentHTML('afterbegin', createTicketHTML(ticket));
});

connection.on("TicketUpdated", (ticket) => {
    const root = document.querySelector(`.ticket-card[data-id="${ticket.id}"]`) || document.querySelector(`.ticket[data-id="${ticket.id}"]`);
    if (!root) return;
    const header = root.querySelector('.ticket-header');
    if (header) {
        header.classList.remove('status-new','status-inprogress','status-ready','status-late');
        if (ticket.status === 'InProgress') header.classList.add('status-inprogress');
        else if (ticket.status === 'Ready') header.classList.add('status-ready');
        else header.classList.add('status-new');
    }
    root.classList.add("changed");
    setTimeout(() => root.classList.remove("changed"), 1500);
});

// New event names per spec: ticketsCreated (batch or single), ticketChanged (single)
connection.on("ticketsCreated", (payload) => {
    const container = document.getElementById("tickets-container") || document.getElementById("tickets");
    if (!container) return;
    const tickets = Array.isArray(payload) ? payload : [payload];
    tickets.forEach(t => container.insertAdjacentHTML('afterbegin', createTicketHTML(t)));
    if (tickets.length) playNotificationSound();
});

connection.on("ticketChanged", (ticket) => {
    const root = document.querySelector(`.ticket-card[data-id="${ticket.id}"]`);
    if (!root) return;
    const header = root.querySelector('.ticket-header');
    if (header) {
        header.classList.remove('status-new','status-inprogress','status-ready','status-late');
        if (ticket.status === 'InProgress') header.classList.add('status-inprogress');
        else if (ticket.status === 'Ready') header.classList.add('status-ready');
        else header.classList.add('status-new');
    }
    root.classList.add("changed");
    setTimeout(() => root.classList.remove("changed"), 1500);
});

// Single-ticket event and play notification sound
connection.on("TicketCreated", (ticket) => {
    const container = document.getElementById("tickets-container") || document.getElementById("tickets");
    if (!container) return;
    container.insertAdjacentHTML('afterbegin', createTicketHTML(ticket));
    playNotificationSound();
});

connection.onreconnecting((error) => {
    console.warn('SignalR reconnecting', error);
    showToast('Reconnecting...');
});

connection.onreconnected((connectionId) => {
    console.info('SignalR reconnected, connectionId=', connectionId);
    hideToast();
    try { location.reload(); } catch (e) { console.warn('reload failed', e); }
});

connection.start()
    .then(() => console.info('Connected to SignalR'))
    .catch(err => console.error(err));

// Bootstrap toast API for KdsWeb
(function (w, $) {
    function ensureContainer() {
        if ($('#toast-container').length === 0) {
            $('body').append('<div aria-live="polite" aria-atomic="true" class="position-fixed top-0 end-0 p-3" style="z-index:1080; min-width:280px;"><div id="toast-container" class="toast-container"></div><template id="toast-template"><div class="toast align-items-center text-bg-light border-0" role="alert" aria-live="assertive" aria-atomic="true"><div class="d-flex"><div class="toast-body"><span class="message"></span><div class="progress mt-2" style="height:3px;"><div class="progress-bar" role="progressbar" style="width:0%;"></div></div></div><button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button></div></div></template></div>');
        }
    }
    function show(type, message) {
        ensureContainer();
        const $template = $('#toast-template');
        const $toast = $($template.html());
        const $bar = $toast.find('.progress-bar');
        const classes = {
            success: 'text-bg-success',
            error: 'text-bg-danger',
            info: 'text-bg-info'
        };
        $toast.removeClass('text-bg-light').addClass(classes[type] || 'text-bg-secondary');
        $toast.find('.message').text(message);
        $('#toast-container').append($toast);
        const t = new bootstrap.Toast($toast[0], { autohide: true, delay: 5000 });
        t.show();
        const duration = 5000;
        const start = Date.now();
        const interval = setInterval(function () {
            const pct = Math.min(100, ((Date.now() - start) / duration) * 100);
            $bar.css('width', pct + '%');
            if (pct >= 100) { clearInterval(interval); }
        }, 50);
        $toast.on('hidden.bs.toast', function () { clearInterval(interval); $toast.remove(); });
    }
    w.toast = w.toast || {};
    w.toast.success = function (m) { show('success', m); };
    w.toast.error = function (m) { show('error', m); };
    w.toast.info = function (m) { show('info', m); };
})(window, jQuery);

