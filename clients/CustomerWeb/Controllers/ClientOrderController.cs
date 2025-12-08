using Microsoft.AspNetCore.Mvc;
using CustomerWeb.Services;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerWeb.Controllers;

public class ClientOrderController : Controller
{
    private readonly IBackendApiClient _backend;
    public ClientOrderController(IBackendApiClient backend)
    {
        _backend = backend;
    }

    [HttpGet("/client/order/{id:guid}")]
    public IActionResult Details(Guid id)
    {
        ViewBag.OrderId = id;
        return View("~/Views/Client/OrderDetails.cshtml");
    }

    [HttpGet("/client/order/{id:guid}/payment-success")]
    public IActionResult PaymentSuccess(Guid id)
    {
        ViewBag.OrderId = id;
        return View("~/Views/Client/PaymentSuccess.cshtml");
    }

    [HttpGet("/client/order/{id:guid}/payment-failed")]
    public IActionResult PaymentFailed(Guid id)
    {
        ViewBag.OrderId = id;
        return View("~/Views/Client/PaymentFailed.cshtml");
    }

    [HttpGet("/client/order/{orderId:guid}/track")]
    public async Task<IActionResult> Track(Guid orderId, CancellationToken ct)
    {
        if (orderId == Guid.Empty) return BadRequest("Invalid orderId");
        var cart = await _backend.GetCartAsync(orderId, ct);
        if (cart is null) return NotFound();
        ViewBag.OrderId = orderId;
        return View("~/Views/Client/TrackOrder.cshtml", cart);
    }
}
