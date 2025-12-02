// UI enhancements: sidebar toggle + toast + loading overlay helpers
(() => {
	const sidebar = document.getElementById("sidebar");
	const btnToggle = document.getElementById("btnToggle");
	btnToggle?.addEventListener("click", () => sidebar?.classList.toggle("open"));

	// Toast API
	const stack = document.getElementById("toastStack");

	function createToast(message, type = "info", timeout = 4000) {
		if (!stack) return;

		const el = document.createElement("div");
		el.className = `toast-msg ${type}`;
		el.innerHTML = `<span>${message}</span><button class="close" aria-label="?óng">×</button>`;

		stack.appendChild(el);

		const close = () => {
			el.classList.add("fade");
			setTimeout(() => el.remove(), 250);
		};

		el.querySelector(".close")?.addEventListener("click", close);

		if (timeout > 0) {
			setTimeout(close, timeout);
		}
	}

	window.AppToast = {
		info: (msg, timeout) => createToast(msg, "info", timeout),
		success: (msg, timeout) => createToast(msg, "success", timeout),
		error: (msg, timeout) => createToast(msg, "error", timeout),
		warn: (msg, timeout) => createToast(msg, "warn", timeout),
	};

	// Loading overlay
	const overlay = document.getElementById("loadingOverlay");
	let overlayCount = 0;

	function showLoading() {
		if (!overlay) return;
		overlayCount++;
		overlay.classList.remove("d-none");
	}

	function hideLoading() {
		if (!overlay) return;
		overlayCount = Math.max(0, overlayCount - 1);
		if (overlayCount === 0) {
			overlay.classList.add("d-none");
		}
	}

	window.AppLoading = { show: showLoading, hide: hideLoading };

	// T? ??ng hi?n th? loading khi submit form có data-loading
	document.querySelectorAll("form[data-loading]").forEach((f) => {
		f.addEventListener("submit", () => showLoading());
	});
})();
