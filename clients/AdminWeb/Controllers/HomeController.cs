using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AdminWeb.Models;

namespace AdminWeb.Controllers;

public class HomeController(BackendApiClient api) : Controller
{
    public async Task<IActionResult> Index()
    {
        // demo: lấy 10 đơn gần nhất
        var orders = await api.GetOrdersAsync(page: 1, pageSize: 10);
        return View(orders.Items);
    }
}
