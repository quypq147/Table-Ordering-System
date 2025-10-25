(() => {
  const btn = document.getElementById('btnToggle');
  const sidebar = document.getElementById('sidebar');
  btn?.addEventListener('click', () => sidebar?.classList.toggle('open'));

  const themeBtn = document.getElementById('themeToggle');
  themeBtn?.addEventListener('click', () => document.body.classList.toggle('dark'));

  // Bind metrics from JSON injected by controller
  let payload = {};
  try {
    const raw = document.getElementById('dashboard-data')?.textContent?.trim();
    if (raw && raw.startsWith('{')) payload = JSON.parse(raw);
  } catch {}

  const setText = (key, def='0') => {
    document.querySelectorAll(`[data-metric="${key}"]`).forEach(e => e.textContent = (payload[key] ?? def));
  };

  // Defaults for demo if controller hasn't provided data
  payload = Object.assign({
    totalTables: 24,
    activeTables: 20,
    openSessions: 12,
    avgGuests: 2.4,
    ordersToday: 180,
    ordersReady: 46,
    revenueToday: '₫12,450,000',
    avgTicket: '₫86,000',
    ticketsReady: 18,
    ticketsInProgress: 9,
    avgPrepMin: 14,
    p95PrepMin: 28,
    revenueByHour: { labels: ['09h','10h','11h','12h','13h','14h','15h'], values: [200, 560, 1200, 2100, 1800, 1500, 900] },
    stationWorkload: { labels: ['Grill','Fryer','Wok','Drink'], values: [12, 8, 10, 16] },
    recentOrders: [
      { id:'0001', code:'#A123', status:'Placed', total:'₫180,000', created:'10:12' },
      { id:'0002', code:'#A124', status:'Ready', total:'₫86,000', created:'10:18' },
      { id:'0003', code:'#A125', status:'Paid', total:'₫420,000', created:'10:22' }
    ]
  }, payload || {});

  ['totalTables','activeTables','openSessions','avgGuests','ordersToday','ordersReady','revenueToday','avgTicket','ticketsReady','ticketsInProgress','avgPrepMin','p95PrepMin']
    .forEach(k => setText(k, payload[k]));

  // Render charts
  try {
    const rev = document.getElementById('revChart');
    if (rev) {
      new Chart(rev, {
        type: 'line',
        data: {
          labels: payload.revenueByHour.labels,
          datasets: [{
            label: 'Revenue',
            data: payload.revenueByHour.values,
            tension: 0.35
          }]
        },
        options: {
          responsive: true,
          plugins: { legend: { display: false } },
          scales: { y: { grid: { display: false } }, x: { grid: { display: false } } }
        }
      });
    }

    const st = document.getElementById('stationChart');
    if (st) {
      new Chart(st, {
        type: 'bar',
        data: {
          labels: payload.stationWorkload.labels,
          datasets: [{ label: 'Tickets', data: payload.stationWorkload.values }]
        },
        options: {
          responsive: true,
          plugins: { legend: { display: false } },
          scales: { y: { grid: { display: false } }, x: { grid: { display: false } } }
        }
      });
    }
  } catch (e) { console.warn(e); }

  // Fill recent orders
  const tbody = document.getElementById('recentOrders');
  if (tbody && Array.isArray(payload.recentOrders)) {
    tbody.innerHTML = payload.recentOrders.map(o => `
      <tr>
        <td>${o.code}</td>
        <td>${o.status}</td>
        <td>${o.total}</td>
        <td>${o.created}</td>
        <td><a class="ghost" href="/Orders/Detail/${o.id}">Detail</a></td>
      </tr>
    `).join('');
  }
})();
