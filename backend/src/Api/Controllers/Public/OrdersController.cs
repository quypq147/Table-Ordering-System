using Application.Common.CQRS; // ISender for custom CQRS
using Application.Orders.Commands; // CancelOrderCommand
using Application.Public.Orders;
using Application.Dtos; // Return OrderDto for payment endpoints
using Microsoft.AspNetCore.Mvc;

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

    // Changed to return OrderDto JSON instead of 204 NoContent to avoid client JSON parsing errors
    [HttpPost("{orderId:guid}/request-cash")]
    public async Task<ActionResult<OrderDto>> RequestCash(Guid orderId, CancellationToken ct)
    {
        var dto = await _sender.Send(new RequestCashPaymentCommand(orderId), ct);
        return Ok(dto);
    }

    // Changed to return OrderDto JSON instead of 204 NoContent to avoid client JSON parsing errors
    [HttpPost("{orderId:guid}/mock-transfer")]
    public async Task<ActionResult<OrderDto>> MockTransfer(Guid orderId, CancellationToken ct)
    {
        var dto = await _sender.Send(new MockTransferPaymentCommand(orderId), ct);
        return Ok(dto);
    }
}
