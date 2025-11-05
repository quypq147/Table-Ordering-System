(() => {
  const btn = document.getElementById('btnToggle');
  const sidebar = document.getElementById('sidebar');
  btn?.addEventListener('click', () => sidebar?.classList.toggle('open'));

  // Bind metrics from JSON injected by controller
  let payload = {};
  try {
    const raw = document.getElementById('dashboard-data')?.textContent?.trim();
    if (raw && raw.startsWith('{')) payload = JSON.parse(raw);
  } catch {}

  // Ensure required structure
  const toMoney = (v, currency) => {
    if (typeof v === 'number') return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: currency || 'VND' }).format(v);
    return v ?? '';
  };

  const setText = (key, def='0') => {
    const val = payload[key] ?? def;
    document.querySelectorAll(`[data-metric="${key}"]`).forEach(e => e.textContent = val);
  };

  // Map DashboardVm properties
  setText('TotalTables', '0');
  setText('ActiveTables', '0');
  setText('OrdersToday', '0');
  setText('OrdersInProgress', '0');
  setText('OrdersReady', '0');

  // RevenueToday is currency
  const revElems = document.querySelectorAll('[data-metric="RevenueToday"]');
  const currency = payload.Currency || 'VND';
  const formattedRev = toMoney(payload.RevenueToday ??0, currency);
  revElems.forEach(e => e.textContent = formattedRev);

  // Render charts from RevenueByHour and OrdersToday breakdown if available
  try {
    const rev = document.getElementById('revChart');
    if (rev && Array.isArray(payload.RevenueByHourLabels) && Array.isArray(payload.RevenueByHourValues)) {
      new Chart(rev, {
        type: 'line',
        data: {
          labels: payload.RevenueByHourLabels,
          datasets: [{ label: 'Doanh thu', data: payload.RevenueByHourValues, tension:0.35 }]
        },
        options: { responsive: true, plugins: { legend: { display: false } } }
      });
    }

    const st = document.getElementById('stationChart');
    if (st && typeof payload.OrdersToday === 'number') {
      // If you later have a status breakdown, replace with real series. For now, show InProgress vs Ready using existing fields
      const labels = ['Đang chế biến', 'Sẵn sàng'];
      const values = [payload.OrdersInProgress ||0, payload.OrdersReady ||0];
      new Chart(st, {
        type: 'bar',
        data: { labels, datasets: [{ label: 'Đơn', data: values }] },
        options: { responsive: true, plugins: { legend: { display: false } } }
      });
    }
  } catch (e) { console.warn(e); }

  // Fill recent orders if provided
  const tbody = document.getElementById('recentOrders');
  if (tbody && Array.isArray(payload.RecentOrders)) {
    tbody.innerHTML = payload.RecentOrders.map(o => `
      <tr>
        <td>${o.code}</td>
        <td>${o.status}</td>
        <td>${toMoney(o.total, currency)}</td>
        <td>${o.created}</td>
        <td><a class="ghost" href="/Orders/Detail/${o.id}">Chi tiết</a></td>
      </tr>
    `).join('');
  }
})();
