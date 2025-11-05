// Small enhancements for Admin UI: sidebar toggle only (removed theme persistence)
(() => {
	const sidebar = document.getElementById("sidebar");
	const btnToggle = document.getElementById("btnToggle");

	btnToggle?.addEventListener("click", () => {
		sidebar?.classList.toggle("open");
	});
})();
