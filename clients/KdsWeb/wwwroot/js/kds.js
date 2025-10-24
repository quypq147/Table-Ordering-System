// Kết nối tới backend SignalR hub /hubs/kds (backend phải bật CORS cho origin của KdsWeb)
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:5001/hubs/kds") // chỉnh theo backend
    .withAutomaticReconnect()
    .build();

connection.on("TicketNew", (ticket) => {
    const list = document.getElementById("tickets");
    const card = document.createElement("div");
    card.className = "ticket";
    card.dataset.id = ticket.id;
    card.innerHTML = `
        <div class="code"><b>${ticket.itemName}</b> x ${ticket.qty}</div>
        <div class="meta">Status: ${ticket.status}</div>
        <div class="meta">${new Date(ticket.createdAt).toLocaleString()}</div>
    `;
    list.prepend(card);
});

connection.on("TicketUpdated", (ticket) => {
    const el = document.querySelector(`.ticket[data-id="${ticket.id}"]`);
    if (el) {
        el.querySelector(".meta").innerText = "Status: " + ticket.status;
    }
});

connection.start().catch(err => console.error(err));

