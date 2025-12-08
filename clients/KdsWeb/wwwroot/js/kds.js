// Kết nối tới backend SignalR hub /hubs/kds (backend phải bật CORS cho origin của KdsWeb)
const hubUrl = (window.__cfg && window.__cfg.kdsHub) || "/hubs/kds";
const bearer = window.__cfg && window.__cfg.bearer;

const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, bearer ? { accessTokenFactory: () => bearer } : undefined)
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .build();

function renderTicket(ticket) {
    const card = document.createElement("div");
    card.className = "ticket";
    card.dataset.id = ticket.id;
    card.innerHTML = `
        <div class="code"><b>${ticket.orderCode || (ticket.orderId ? ticket.orderId.substring(0,8) : '')}</b> - Bàn ${ticket.tableName || ''}</div>
        <div class="meta item"><b>${ticket.itemName}</b> x ${ticket.qty}</div>
        <div class="meta status">Status: ${ticket.status}</div>
        <div class="meta time">${new Date(ticket.createdAt).toLocaleString()}</div>
    `;
    return card;
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
        // ramp down quickly
        g.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.25);
        setTimeout(() => {
            try { o.stop(); ctx.close(); } catch (e) { }
        }, 300);
    } catch (e) {
        // ignore audio errors
        console.warn('playNotificationSound error', e);
    }
}

// Backward compatibility (old event names)
connection.on("TicketNew", (ticket) => {
    const list = document.getElementById("tickets");
    if (!list) return;
    list.prepend(renderTicket(ticket));
});

connection.on("TicketUpdated", (ticket) => {
    const el = document.querySelector(`.ticket[data-id="${ticket.id}"] .status`);
    if (el) el.innerText = "Status: " + ticket.status;
});

// New event names per spec: ticketsCreated (batch or single), ticketChanged (single)
connection.on("ticketsCreated", (payload) => {
    const list = document.getElementById("tickets");
    if (!list) return;
    const tickets = Array.isArray(payload) ? payload : [payload];
    tickets.forEach(t => list.prepend(renderTicket(t)));
});

connection.on("ticketChanged", (ticket) => {
    const root = document.querySelector(`.ticket[data-id="${ticket.id}"]`);
    if (!root) return;
    const statusEl = root.querySelector(".status");
    if (statusEl) statusEl.innerText = "Status: " + ticket.status;
    // Optional highlight effect
    root.classList.add("changed");
    setTimeout(() => root.classList.remove("changed"), 1500);
});

// New single-ticket event and play notification sound
connection.on("TicketCreated", (ticket) => {
    const list = document.getElementById("tickets");
    if (!list) return;
    list.prepend(renderTicket(ticket));
    playNotificationSound();
});

connection.onreconnecting((error) => {
    console.warn('SignalR reconnecting', error);
    showToast('Reconnecting...');
});

connection.onreconnected((connectionId) => {
    console.info('SignalR reconnected, connectionId=', connectionId);
    hideToast();
    // reload page to ensure data consistency after reconnect
    try { location.reload(); } catch (e) { console.warn('reload failed', e); }
});

connection.start()
    .then(() => console.info('Connected to SignalR'))
    .catch(err => console.error(err));

