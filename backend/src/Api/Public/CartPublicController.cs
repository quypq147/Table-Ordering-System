using Application.Dtos;
using Application.Public.Cart;
using Application.Orders.Commands; // CẦN THIẾT: Cho ChangeItemQuantityCommand
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Public;

[ApiController]
[Route("api/public/cart-public")]
public sealed class CartPublicController : ControllerBase
{
	private readonly ISender _sender;

	public CartPublicController(ISender sender)
	{
		_sender = sender;
	}

	/// <summary>
	/// Lấy giỏ hàng theo OrderId.
	/// </summary>
	[HttpGet("{orderId:guid}")]
	public async Task<ActionResult<CartDto>> GetCartById(
		Guid orderId,
		CancellationToken ct)
	{
		var cart = await _sender.Send(new GetCartByIdQuery(orderId), ct);

		if (cart is null)
			return NotFound();

		return Ok(cart);
	}

	// =========================================================================
	// PHƯƠNG THỨC CẬP NHẬT (PATCH) [ĐÃ THÊM]
	// =========================================================================

	/// <summary>
	/// Cập nhật số lượng món trong giỏ hàng theo OrderItemId (US13).
	/// Endpoint: PATCH api/public/cart-public/{orderId}/items/{orderItemId}
	/// Body: { "quantity": 5 }
	/// </summary>
	[HttpPatch("{orderId:guid}/items/{orderItemId:int}")]
	public async Task<ActionResult<OrderDto>> UpdateCartItemQuantity(
		Guid orderId,
		int orderItemId,
		[FromBody] UpdateCartItemQuantityRequest body, // Sẽ dùng DTO đã thêm
		CancellationToken ct)
	{
		if (body is null || body.Quantity <= 0)
		{
			return BadRequest("Số lượng không hợp lệ.");
		}

		var order = await _sender.Send(
			new ChangeItemQuantityCommand(orderId, orderItemId, body.Quantity),
			ct);

		return Ok(order);
	}

	/// <summary>
	/// Cập nhật ghi chú món trong giỏ hàng theo OrderItemId (US13).
	/// Endpoint: PATCH api/public/cart-public/{orderId}/items/{orderItemId}/note
	/// Body: { "note": "Ghi chú mới" }
	/// </summary>
	[HttpPatch("{orderId:guid}/items/{orderItemId:int}/note")]
	public async Task<ActionResult<OrderDto>> UpdateCartItemNote(
		Guid orderId,
		int orderItemId,
		[FromBody] ChangeCartItemNoteRequest body, // Sẽ dùng DTO đã thêm
		CancellationToken ct)
	{
		var order = await _sender.Send(
			new ChangeCartItemNoteCommand(orderId, orderItemId, body?.Note),
			ct);

		return Ok(order);
	}

	// =========================================================================
	// PHƯƠNG THỨC XÓA (DELETE) [ĐÃ SỬA ĐỂ KHỚP FRONTEND]
	// =========================================================================

	/// <summary>
	/// Xóa toàn bộ giỏ hàng (US14).
	/// Endpoint: DELETE api/public/cart-public/{orderId}/items (KHỚP clearAll())
	/// </summary>
	[HttpDelete("{orderId:guid}/items")]
	public async Task<ActionResult<OrderDto>> ClearCart(
		Guid orderId,
		CancellationToken ct)
	{
		var order = await _sender.Send(new ClearCartCommand(orderId), ct);
		return Ok(order);
	}

	/// <summary>
	/// Xóa một món trong giỏ hàng theo OrderItemId.
	/// Endpoint: DELETE api/public/cart-public/{orderId}/items/{orderItemId} (KHỚP removeItem())
	/// </summary>
	[HttpDelete("{orderId:guid}/items/{orderItemId:int}")]
	public async Task<ActionResult<OrderDto>> RemoveCartItem(
		Guid orderId,
		int orderItemId, // Lấy OrderItemId từ URL
		CancellationToken ct)
	{
		// Kiểm tra cơ bản
		if (orderItemId <= 0)
		{
			return BadRequest("OrderItemId không hợp lệ.");
		}

		var order = await _sender.Send(
			new RemoveCartItemCommand(orderId, orderItemId),
			ct);

		return Ok(order);
	}

	// =========================================================================
	// PHƯƠNG THỨC KHÁC
	// =========================================================================

	/// <summary>
	/// Gửi / xác nhận đơn hàng (US11).
	/// Body: { "customerNote": "Ghi chú" }
	/// </summary>
	[HttpPost("{orderId:guid}/submit")]
	public async Task<ActionResult<OrderDto>> SubmitOrder(
		Guid orderId,
		[FromBody] SubmitOrderRequest body,
		CancellationToken ct)
	{
		var order = await _sender.Send(
			new SubmitCartCommand(orderId, body?.CustomerNote),
			ct);

		return Ok(order);
	}
}