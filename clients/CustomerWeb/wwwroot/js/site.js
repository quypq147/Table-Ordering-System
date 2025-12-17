// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Debounce utility
function debounce(fn, delay) { let t; return function () { const ctx = this, args = arguments; clearTimeout(t); t = setTimeout(function () { fn.apply(ctx, args); }, delay); }; }
function formatVnd(value) { try { return Number(value).toLocaleString('vi-VN') + ' VNĐ'; } catch { return value + ' VNĐ'; } }

// Tailwind Toast for CustomerWeb
(function (w, $) {
    const containerSelector = '#toast-container';
    function ensureContainer() { if ($(containerSelector).length === 0) { $('body').append('<div id="toast-container" class="fixed top-4 right-4 z-50 space-y-3 pointer-events-none"></div>'); } }
    function show(type, message) {
        ensureContainer();
        const $template = $('#toast-template');
        let $toast = $template.length ? $($template.html()) : $('<div class="toast-item transform transition-all duration-300 translate-x-4 opacity-0 pointer-events-auto min-w-[260px] max-w-sm rounded-xl shadow-lg border bg-white overflow-hidden"><div class="flex items-start p-4"><div class="icon mr-3"></div><div class="flex-1"><div class="message text-sm font-medium text-gray-800"></div><div class="progress h-1 mt-3 rounded bg-gray-100"><div class="bar h-1 rounded"></div></div></div><button type="button" class="ml-3 text-gray-400 hover:text-gray-600 close-btn"><i class="fas fa-times"></i></button></div></div>');
        const colors = { success: { bg: 'bg-green-50', border: 'border-green-200', icon: '<i class="fas fa-check-circle text-green-500"></i>', bar: 'bg-green-500' }, error: { bg: 'bg-red-50', border: 'border-red-200', icon: '<i class="fas fa-times-circle text-red-500"></i>', bar: 'bg-red-500' }, info: { bg: 'bg-blue-50', border: 'border-blue-200', icon: '<i class="fas fa-info-circle text-blue-500"></i>', bar: 'bg-blue-500' } }[type] || { bg: 'bg-gray-50', border: 'border-gray-200', icon: '<i class="fas fa-bell text-gray-500"></i>', bar: 'bg-gray-500' };
        $toast.addClass(colors.bg).addClass(colors.border);
        $toast.find('.icon').html(colors.icon);
        $toast.find('.message').text(message);
        const $bar = $toast.find('.bar'); $bar.addClass(colors.bar).css({ width: '0%' });
        $(containerSelector).append($toast);
        requestAnimationFrame(function () { $toast.removeClass('translate-x-4 opacity-0'); $toast.addClass('translate-x-0 opacity-100'); });
        let dismissed = false; const duration = 5000; const start = Date.now();
        const interval = setInterval(function () { const pct = Math.min(100, ((Date.now() - start) / duration) * 100); $bar.css('width', pct + '%'); if (pct >= 100) { clearInterval(interval); dismiss(); } }, 50);
        function dismiss() { if (dismissed) return; dismissed = true; $toast.addClass('translate-x-4 opacity-0'); setTimeout(function () { $toast.remove(); }, 250); }
        $toast.find('.close-btn').on('click', function () { clearInterval(interval); dismiss(); });
    }
    w.toast = w.toast || {}; w.toast.success = function (m) { show('success', m); }; w.toast.error = function (m) { show('error', m); }; w.toast.info = function (m) { show('info', m); };

    // Confirm modal API
    const $modal = $('#confirm-modal');
    function confirmModal(message) { return new Promise(function (resolve) { $('#confirm-message').text(message || 'Bạn có chắc không?'); $modal.removeClass('hidden').addClass('flex'); const off = function () { $modal.addClass('hidden').removeClass('flex'); $('#confirm-ok').off('click', ok); $('#confirm-cancel').off('click', cancel); }; const ok = function () { off(); resolve(true); }; const cancel = function () { off(); resolve(false); }; $('#confirm-ok').on('click', ok); $('#confirm-cancel').on('click', cancel); }); }
    w.ui = w.ui || {}; w.ui.confirm = confirmModal;
})(window, jQuery);

(function ($) {
    const state = { orderId: null, pendingUpdates: {}, debouncers: {} };
    function recalcTotals() { let total = 0; $('#cart-items .cart-item').each(function () { const $item = $(this); const price = Number($item.data('item-price')) || 0; const qty = Number($item.find('.qty').text()) || 0; const line = price * qty; total += line; $item.find('.line-total').text(formatVnd(line)); }); $('#cart-total').text(formatVnd(total)); const count = $('#cart-items .cart-item').length; $('#cart-items-count').text(count); $('.btn-submit').prop('disabled', count === 0); toggleEmptyState(count === 0); }
    function toggleEmptyState(isEmpty) { const $empty = $('#cart-items .empty-cart'); if (isEmpty) { if ($empty.length === 0) { $('#cart-items').append('<div class="h-full flex flex-col items-center justify-center text-gray-400 opacity-60 empty-cart"><i class="fas fa-shopping-basket text-6xl mb-4"></i><p>Chưa có món nào</p></div>'); } } else { $empty.remove(); } }
    function getDebouncer(itemId) { if (!state.debouncers[itemId]) { state.debouncers[itemId] = debounce(function (finalQty, $item, revertQty) { $.post(`/client/cart/${state.orderId}/items/${itemId}`, { quantity: finalQty, note: '' }).done(function (res) { if (!res || res.success !== true) { throw new Error('Update failed'); } if (res.item && typeof res.item.quantity === 'number') { $item.find('.qty').text(res.item.quantity); } if (typeof res.total !== 'undefined') { $('#cart-total').text(formatVnd(res.total)); } else { recalcTotals(); } }).fail(function () { if (revertQty != null) { $item.find('.qty').text(revertQty); recalcTotals(); window.toast.error('Lỗi cập nhật số lượng.'); } }).always(function () { delete state.pendingUpdates[itemId]; }); }, 500); } return state.debouncers[itemId]; }
    function handleQtyChange($item, delta) { const itemId = $item.data('item-id'); const $qtyEl = $item.find('.qty'); const current = Number($qtyEl.text()) || 0; const prev = state.pendingUpdates[itemId]?.revert ?? current; const next = Math.max(1, current + delta); $qtyEl.text(next); recalcTotals(); state.pendingUpdates[itemId] = { qty: next, revert: prev }; const debounced = getDebouncer(itemId); debounced(next, $item, prev); }
    function removeItem($item) { const itemId = $item.data('item-id'); const $container = $('#cart-items'); const $clone = $item.clone(true); $item.fadeOut(150, function () { $item.remove(); recalcTotals(); $.ajax({ url: `/client/cart/${state.orderId}/items/${itemId}`, type: 'DELETE' }).done(function (res) { if (!res || res.success !== true) { throw new Error('Delete failed'); } if (typeof res.total !== 'undefined') { $('#cart-total').text(formatVnd(res.total)); } else { recalcTotals(); } window.toast.success('Đã xóa món.'); }).fail(function () { $clone.hide(); $container.prepend($clone); $clone.fadeIn(150); recalcTotals(); window.toast.error('Lỗi xóa món, đã khôi phục.'); }); }); }
    function bindEvents() { const $root = $('#cart-root'); state.orderId = $root.data('order-id'); $('#cart-items').on('click', '.btn-increase', function (e) { e.preventDefault(); const $item = $(this).closest('.cart-item'); handleQtyChange($item, +1); }); $('#cart-items').on('click', '.btn-decrease', function (e) { e.preventDefault(); const $item = $(this).closest('.cart-item'); handleQtyChange($item, -1); }); $('#cart-items').on('click', '.btn-remove', async function (e) { e.preventDefault(); const $item = $(this).closest('.cart-item'); const ok = await window.ui.confirm('Bạn có chắc muốn xóa món này không?'); if (ok) { removeItem($item); } }); $('.btn-submit').on('click', async function (e) { e.preventDefault(); if (!state.orderId) { window.toast.error('Không tìm thấy thông tin đơn hàng!'); return; } const ok = await window.ui.confirm('Bạn có chắc muốn gửi món xuống bếp không?'); if (!ok) return; const $btn = $(this); const original = $btn.html(); $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang gửi...'); $.post(`/client/cart/${state.orderId}/submit`, {}).done(function () { window.toast.success('Đã gửi thực đơn xuống bếp!'); window.location.href = `/client/order/${state.orderId}/track`; }).fail(function (xhr) { window.toast.error('Lỗi: ' + (xhr.responseText || 'Không rõ')); $btn.prop('disabled', false).html(original); }); }); }
    $(function () { bindEvents(); });
})(jQuery);
