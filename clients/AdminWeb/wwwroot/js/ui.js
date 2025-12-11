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

// Bootstrap toast API for AdminWeb
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
        // contextual bg
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
