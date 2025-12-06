using Application.Common.CQRS;
using Application.Orders.Commands;
using Application.Public.Orders;
using Application.Dtos; // Import OrderDto
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
	public async Task<ActionResult<OrderDto>> Cancel(Guid orderId, CancellationToken ct)
	{
		// Gửi lệnh hủy đơn hàng và mong đợi nó trả về OrderDto đã được cập nhật
		// (hoặc một OrderDto mới sau khi đã được xử lý hủy).
		var orderDto = await _sender.Send(new CancelOrderCommand(orderId), ct);

		// Thay vì return NoContent() (204) gây lỗi JSON parsing ở Client,
		// chúng ta trả về 200 OK với OrderDto (JSON)
		return Ok(orderDto);
	}

	// Các phương thức khác giữ nguyên
	[HttpPost("{orderId:guid}/request-cash")]
	public async Task<ActionResult<OrderDto>> RequestCash(Guid orderId, CancellationToken ct)
	{
		var dto = await _sender.Send(new RequestCashPaymentCommand(orderId), ct);
		return Ok(dto);
	}

	[HttpPost("{orderId:guid}/mock-transfer")]
	public async Task<ActionResult<OrderDto>> MockTransfer(Guid orderId, CancellationToken ct)
	{
		var dto = await _sender.Send(new MockTransferPaymentCommand(orderId), ct);
		return Ok(dto);
	}
}