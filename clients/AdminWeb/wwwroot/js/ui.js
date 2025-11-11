// UI enhancements: sidebar toggle + toast + loading overlay helpers
(() => {
	const sidebar = document.getElementById("sidebar");
	const btnToggle = document.getElementById("btnToggle");
	btnToggle?.addEventListener("click", () => sidebar?.classList.toggle("open"));

	// Toast API
	const stack = document.getElementById("toastStack");
	function showToast(message, type = "info", timeout =4000) {
		if (!stack) return;
		const el = document.createElement("div");
		el.className = `toast-msg ${type}`;
		el.innerHTML = `<span>${message}</span><button class=\"close\" aria-label=\"?óng\">×</button>`;
		stack.appendChild(el);
		const remove = () => el.classList.add("fade"), destroy = () => el.remove();
		el.querySelector(".close")?.addEventListener("click", () => destroy());
		if (timeout >0) setTimeout(destroy, timeout);
		return el;
	}
	window.AppToast = { show: showToast };

	// Loading overlay
	const overlay = document.getElementById("loadingOverlay");
	let overlayCount =0;
	function showLoading() {
		if (!overlay) return; overlayCount++; overlay.classList.remove("d-none");
	}
	function hideLoading() {
		if (!overlay) return; overlayCount = Math.max(0, overlayCount -1); if (overlayCount ===0) overlay.classList.add("d-none");
	}
	window.AppLoading = { show: showLoading, hide: hideLoading };

	// Auto wrap forms with data-loading to show overlay on submit
	document.querySelectorAll("form[data-loading]").forEach(f => {
		f.addEventListener("submit", () => showLoading());
	});
})();
