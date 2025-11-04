using Microsoft.AspNetCore.Mvc;

namespace CustomerWeb.Controllers;

public class CartController : Controller
{
    // /client/cart/{orderId}
    [HttpGet("/client/cart/{orderId:guid}")]
    public IActionResult Index(Guid orderId)
    {
        if (orderId == Guid.Empty) return BadRequest("Missing orderId.");
        ViewBag.OrderId = orderId;
        return View();
    }
}