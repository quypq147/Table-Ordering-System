using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        [HttpPost]
        [Route("api/kds/tickets/{id:guid}/{action}")]
        public async Task<IActionResult> ChangeStatus(Guid id, string action)
        {
            _logger.LogInformation("Proxying KDS action '{Action}' for ticket {TicketId}", action, id);

            // Proxy the request to backend API
            var res = await api.PostAsync($"/api/kds/tickets/{id}/{action}");
            var content = await res.Content.ReadAsStringAsync();

            // Add diagnostic headers to the response so browser can confirm proxy target and status
            Response.Headers["X-Proxy-To"] = api is not null ? (api.GetType().Name + ":" + (Request.Scheme + ":" + Request.Host.Value)) : "BackendApiClient";
            Response.Headers["X-Proxy-Status"] = ((int)res.StatusCode).ToString();

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("Proxy call returned {Status} with body: {Body}", res.StatusCode, content);
                return StatusCode((int)res.StatusCode, content);
            }

            _logger.LogInformation("Proxy call succeeded for ticket {TicketId} action {Action}", id, action);

            // Return backend JSON untouched
            return Content(content, "application/json");
        }
    }
}
