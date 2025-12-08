using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TableOrdering.Contracts;

namespace KdsWeb.Controllers
{
    public class BoardController(BackendApiClient api, IConfiguration cfg, ILogger<BoardController> logger) : Controller
    {
        private readonly ILogger _logger = logger;

        public async Task<IActionResult> Index()
        {
            var tickets = await api.GetTicketsAsync();
            return View(tickets);
        }

        // Xem chi tiết đơn từ phiếu bếp
        [HttpGet]
        public async Task<IActionResult> Details(Guid orderId)
        {
            if (orderId == Guid.Empty)
                return BadRequest("Thiếu OrderId");

            var order = await api.GetOrderAsync(orderId);
            if (order is null) return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(Guid id, string op)
        {
            _logger.LogInformation("Proxying KDS action '{Action}' for ticket {TicketId}", op, id);

            // Proxy the request to backend API (backend route uses {op} instead of reserved 'action')
            var res = await api.PostAsync($"/api/kds/tickets/{id}/{op}");
            var content = await res.Content.ReadAsStringAsync();

            Response.Headers["X-Proxy-To"] = api is not null ? (api.GetType().Name + ":" + (Request.Scheme + ":" + Request.Host.Value)) : "BackendApiClient";
            Response.Headers["X-Proxy-Status"] = ((int)res.StatusCode).ToString();

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("Proxy call returned {Status} with body: {Body}", res.StatusCode, content);
                return StatusCode((int)res.StatusCode, content);
            }

            _logger.LogInformation("Proxy call succeeded for ticket {TicketId} action {Action}", id, op);
            return Content(content, "application/json");
        }
    }
}
