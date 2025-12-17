using CustomerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CustomerWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index([FromQuery] Guid? session)
        {
            if (session is Guid sid && sid != Guid.Empty)
            {
                Response.Cookies.Append("sessionId", sid.ToString(), new CookieOptions
                {
                    HttpOnly = false,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(12)
                });
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost("/client/leave-table")]
        public IActionResult LeaveTable()
        {
            try
            {
                // Clear known cookies/session keys
                Response.Cookies.Delete("sessionId");
                Response.Cookies.Delete("tableCode");
                Response.Cookies.Delete("orderId");
            }
            catch { }

            // Optionally clear server session
            try { HttpContext.Session?.Clear(); } catch { }

            // Redirect to QR help or welcome
            return Redirect("/client/qr-help");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
