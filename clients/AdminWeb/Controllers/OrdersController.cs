using AdminWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminWeb.Controllers
{
    [Authorize(Policy = "RequireSignedIn")]
    public class OrdersController(IBackendApiClient api) : Controller
    {
        public async Task<IActionResult> Index(int page = 1)
        {
            var data = await api.GetOrdersAsync(page, 20);
            return View(data);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            var order = await api.GetOrderAsync(id);
            if (order is null) return NotFound();
            return View(order);
        }

        // Map form 'status' to backend endpoints; domain uses discrete POST endpoints.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, string status)
        {
            HttpResponseMessage res;
            try
            {
                res = status switch
                {
                    "Submitted" => await api.PostAsync($"/api/orders/{id}/submit"),
                    "InProgress" => await api.PostAsync($"/api/orders/{id}/in-progress"),
                    "Ready" => await api.PostAsync($"/api/orders/{id}/ready"),
                    "Served" => await api.PostAsync($"/api/orders/{id}/served"),
                    "Cancelled" => await api.PostAsync($"/api/orders/{id}/cancel"),
                    // For Paid, fetch the latest order to get accurate total amount and send proper body
                    "Paid" => await PostPayAsync(id),
                    _ => throw new InvalidOperationException("Trạng thái không hỗ trợ trực tiếp.")
                };
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return RedirectToAction(nameof(Detail), new { id });
            }

            if (!res.IsSuccessStatusCode)
                TempData["Error"] = "Cập nhật trạng thái thất bại";
            else
                TempData["Success"] = "Đã cập nhật trạng thái";
            return RedirectToAction(nameof(Detail), new { id });
        }

        private async Task<HttpResponseMessage> PostPayAsync(Guid id)
        {
            var order = await api.GetOrderAsync(id);
            if (order is null)
                throw new InvalidOperationException("Không tìm thấy đơn hàng để thanh toán.");

            // Expect order.Total to be available from backend DTO; fallback to Subtotal if needed
            var amount = order.Total > 0 ? order.Total : order.Subtotal;
            if (amount <= 0)
                throw new InvalidOperationException("Số tiền thanh toán không hợp lệ.");

            return await api.PostAsync($"/api/orders/{id}/pay", new { amount, currency = "VND" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id, string? redirect)
        {
            var res = await api.CancelOrderAsync(id);
            if (!res.IsSuccessStatusCode)
                TempData["Error"] = "Huỷ đơn thất bại";
            else
                TempData["Success"] = "Đã huỷ đơn";

            if (string.Equals(redirect, "tables", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Tables");

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseSession(Guid id)
        {
            var res = await api.CloseSessionAsync(id);
            if (!res.IsSuccessStatusCode)
                TempData["Error"] = "Đóng phiên thất bại";
            else
                TempData["Success"] = "Đã đóng phiên";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }

}
