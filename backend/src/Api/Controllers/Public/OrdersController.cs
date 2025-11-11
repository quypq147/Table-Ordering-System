using Application.Public.Orders;
using Application.Orders.Commands; // CancelOrderCommand
using Microsoft.AspNetCore.Mvc;
using Application.Common.CQRS; // ISender for custom CQRS

namespace Api.Controllers.Public;

[ApiController]
[Route("api/public/orders")]
public class OrdersController : ControllerBase
{
 private readonly ISender _sender;
 public OrdersController(ISender sender) => _sender = sender;

 public sealed record VoucherReq(string Code);

 [HttpPost("{orderId:guid}/voucher/preview")]
 public async Task<ActionResult<VoucherPreviewDto>> PreviewVoucher(Guid orderId, [FromBody] VoucherReq body, CancellationToken ct)
 => Ok(await _sender.Send(new PreviewVoucherQuery(orderId, body.Code), ct));

 [HttpPost("{orderId:guid}/cancel")]
 public async Task<ActionResult> Cancel(Guid orderId, CancellationToken ct)
 {
 await _sender.Send(new CancelOrderCommand(orderId), ct);
 return NoContent();
 }

 [HttpPost("{orderId:guid}/request-cash")]
 public async Task<ActionResult> RequestCash(Guid orderId, CancellationToken ct)
 {
 await _sender.Send(new RequestCashPaymentCommand(orderId), ct);
 return NoContent();
 }

 [HttpPost("{orderId:guid}/mock-transfer")]
 public async Task<ActionResult> MockTransfer(Guid orderId, CancellationToken ct)
 {
 await _sender.Send(new MockTransferPaymentCommand(orderId), ct);
 return NoContent();
 }
}
