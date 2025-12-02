using Microsoft.AspNetCore.Mvc;

namespace CustomerWeb.Controllers;

public class ClientOrderController : Controller
{
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
}
