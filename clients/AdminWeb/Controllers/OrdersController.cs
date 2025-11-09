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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, string status)
        {
            var res = await api.UpdateOrderStatusAsync(id, status);
            if (!res.IsSuccessStatusCode)
                TempData["Error"] = "Cập nhật trạng thái thất bại";
            else
                TempData["Success"] = "Đã cập nhật trạng thái";
            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var res = await api.CancelOrderAsync(id);
            if (!res.IsSuccessStatusCode)
                TempData["Error"] = "Huỷ đơn thất bại";
            else
                TempData["Success"] = "Đã huỷ đơn";
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
