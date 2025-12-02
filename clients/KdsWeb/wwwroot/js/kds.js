// Kết nối tới backend SignalR hub /hubs/kds (backend phải bật CORS cho origin của KdsWeb)
const hubUrl = (window.__cfg && window.__cfg.kdsHub) || "/hubs/kds";
const bearer = window.__cfg && window.__cfg.bearer;

const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, bearer ? { accessTokenFactory: () => bearer } : undefined)
    .withAutomaticReconnect()
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

connection.start()
    .then(() => console.info('Connected to SignalR'))
    .catch(err => console.error(err));

