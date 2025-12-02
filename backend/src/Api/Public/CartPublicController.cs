using Application.Dtos;
using Application.Public.Cart;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Public;

[ApiController]
// NOTE: changed route to avoid duplicate with Api.Controllers.Public.CartController
[Route("api/public/cart-public")]
public sealed class CartPublicController : ControllerBase
{
    private readonly ISender _sender;

    public CartPublicController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// L?y gi? hàng theo OrderId.
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

    /// <summary>
    /// Xóa toàn b? gi? hàng (US14).
    /// </summary>
    [HttpDelete("{orderId:guid}/items/all")]
    public async Task<ActionResult<OrderDto>> ClearCart(
        Guid orderId,
        CancellationToken ct)
    {
        var order = await _sender.Send(new ClearCartCommand(orderId), ct);
        return Ok(order);
    }

    /// <summary>
    /// Xóa m?t món trong gi? hàng theo OrderItemId.
    /// Body: { "orderItemId": 123 }
    /// </summary>
    [HttpDelete("{orderId:guid}/items")]
    public async Task<ActionResult<OrderDto>> RemoveCartItem(
        Guid orderId,
        [FromBody] RemoveCartItemRequest body,
        CancellationToken ct)
    {
        if (body is null || body.OrderItemId <= 0)
        {
            return BadRequest("Thi?u orderItemId");
        }

        var order = await _sender.Send(
            new RemoveCartItemCommand(orderId, body.OrderItemId),
            ct);

        return Ok(order);
    }

    /// <summary>
    /// G?i / xác nh?n ??n hàng (US11).
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
