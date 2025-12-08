(() => {
  // Đọc JSON từ script#dashboard-data
  let payload = {};
  try {
    const raw = document.getElementById("dashboard-data")?.textContent?.trim();
    if (raw) {
      payload = JSON.parse(raw);
    }
  } catch (e) {
    console.error("Cannot parse dashboard payload", e);
    payload = {};
  }

  const currency = payload.Currency || "VND";

  const formatNumber = (v) =>
    typeof v === "number" ? v.toLocaleString("vi-VN") : (v ?? "0");

  const formatMoney = (v) => {
    if (typeof v !== "number") return v ?? "";
    try {
      return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: currency,
        maximumFractionDigits: 0,
      }).format(v);
    } catch {
      return v.toLocaleString("vi-VN");
    }
  };

  //1) Đổ các metric vào card
  const metrics = {
    TotalTables: payload.TotalTables ?? 0,
    ActiveTables: payload.ActiveTables ?? 0,
    OrdersToday: payload.OrdersToday ?? 0,
    OrdersInProgress: payload.OrdersInProgress ?? 0,
    RevenueToday: payload.RevenueToday ?? 0,
  };

  document.querySelectorAll(".metric[data-metric]").forEach((el) => {
    const key = el.getAttribute("data-metric");
    let val = metrics[key];

    if (key === "RevenueToday") {
      el.textContent = formatMoney(val);
    } else {
      el.textContent = formatNumber(val);
    }

    // bỏ class loading nếu có (skeleton)
    el.closest(".stat, .card")?.classList.remove("loading");
  });

  //2) Vẽ chart doanh thu theo giờ (bar)
  const revLabels = payload.RevenueByHourLabels || [];
  const revValues = payload.RevenueByHourValues || [];
  if (revLabels.length && revValues.length && window.Chart) {
    const ctx = document.getElementById("revChart");
    if (ctx) {
      new Chart(ctx, {
        type: "bar",
        data: {
          labels: revLabels,
          datasets: [
            {
              label: "Doanh thu",
              data: revValues,
              backgroundColor: "rgba(54, 162, 235, 0.5)",
              borderColor: "rgba(54, 162, 235, 1)",
              borderWidth: 1,
            },
          ],
        },
        options: {
          responsive: true,
          plugins: { legend: { display: false } },
          scales: {
            y: {
              ticks: {
                callback: (v) => v.toLocaleString("vi-VN"),
              },
              beginAtZero: true,
            },
          },
        },
      });
    }
  }

  //3) Bảng đơn hàng gần đây
  const tbody = document.getElementById("recentOrders");
  if (!tbody) return;

  const orders = Array.isArray(payload.RecentOrders)
    ? payload.RecentOrders
    : [];

  if (!orders.length) {
    tbody.innerHTML =
      '<tr><td colspan="5" class="text-center text-muted py-3">Chưa có đơn gần đây.</td></tr>';
    return;
  }

  tbody.innerHTML = orders
    .map((o) => {
      const id = o.Id;
      const code = o.Code ?? "";
      const status = o.Status ?? "";
      const total = formatMoney(o.Total ?? 0);
      const created = o.Created ?? "";

      return `
      <tr>
        <td>${code}</td>
        <td>${status}</td>
        <td class="text-end">${total}</td>
        <td>${created}</td>
        <td><a class="ghost" href="/Orders/Detail/${id}">Chi tiết</a></td>
      </tr>
    `;
    })
    .join("");
})();
