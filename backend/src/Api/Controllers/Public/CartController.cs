using Application.Public.Cart;
using Application.Orders.Commands; // ChangeItemQuantityCommand, RemoveItemCommand
using Application.Orders.Queries; // GetOrderByIdQuery
using Application.Dtos;
using Application.Common.CQRS; // custom CQRS ISender
using MediatR; // MediatR ISender for public flows
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Public;

[ApiController]
[Route("api/public/cart")]
public class CartController : ControllerBase
{
    private readonly Application.Common.CQRS.ISender _cqrs; // for custom ICommand/IQuery
    private readonly MediatR.ISender _mediator; // for MediatR IRequest

    public CartController(Application.Common.CQRS.ISender cqrs, MediatR.ISender mediator)
    { _cqrs = cqrs; _mediator = mediator; }

    public sealed record StartDto(string TableCode);

    // POST /api/public/cart/start
    [HttpPost("start")]
    public async Task<ActionResult<object>> Start([FromBody] StartDto body, CancellationToken ct)
    {
        var orderId = await _mediator.Send(new StartCartByTableCodeCommand(body.TableCode), ct);
        return Ok(new { OrderId = orderId });
    }

    // GET /api/public/cart/{orderId}
    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderDto>> Get(Guid orderId, CancellationToken ct)
    {
        var dto = await _cqrs.Send(new GetOrderByIdQuery(orderId), ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    public sealed record AddItemDto(Guid MenuItemId, int Quantity, string? Note);

    // POST /api/public/cart/{orderId}/items
    [HttpPost("{orderId:guid}/items")]
    public async Task<ActionResult> AddItem(Guid orderId, [FromBody] AddItemDto body, CancellationToken ct)
    {
        await _mediator.Send(new AddCartItemByMenuItemIdCommand(orderId, body.MenuItemId, body.Quantity, body.Note), ct);
        return NoContent();
    }

    public sealed record ChangeQtyDto(int NewQuantity);

    // PATCH /api/public/cart/{orderId}/items/{orderItemId}
    [HttpPatch("{orderId:guid}/items/{orderItemId:int}")]
    public async Task<ActionResult> ChangeQty(Guid orderId, int orderItemId, [FromBody] ChangeQtyDto body, CancellationToken ct)
    {
        await _cqrs.Send(new ChangeItemQuantityCommand(orderId, orderItemId, body.NewQuantity), ct);
        return NoContent();
    }

    public sealed record ChangeNoteDto(string? Note);

    // PATCH /api/public/cart/{orderId}/items/{orderItemId}/note
    [HttpPatch("{orderId:guid}/items/{orderItemId:int}/note")]
    public async Task<ActionResult> ChangeNote(Guid orderId, int orderItemId, [FromBody] ChangeNoteDto body, CancellationToken ct)
    {
        await _cqrs.Send(new ChangeCartItemNoteCommand(orderId, orderItemId, body.Note), ct);
        return NoContent();
    }

    public sealed record RemoveItemDto(Guid MenuItemId);

    // DELETE /api/public/cart/{orderId}/items
    [HttpDelete("{orderId:guid}/items")]
    public async Task<ActionResult> RemoveItem(Guid orderId, [FromBody] RemoveItemDto body, CancellationToken ct)
    {
        await _cqrs.Send(new RemoveItemCommand(orderId, body.MenuItemId), ct);
        return NoContent();
    }

    // DELETE /api/public/cart/{orderId}/all
    [HttpDelete("{orderId:guid}/all")]
    public async Task<ActionResult<OrderDto>> Clear(Guid orderId, CancellationToken ct)
    {
        var dto = await _cqrs.Send(new ClearCartCommand(orderId), ct);
        return Ok(dto);
    }

    // POST /api/public/cart/{orderId}/submit
    [HttpPost("{orderId:guid}/submit")]
    public async Task<ActionResult<OrderDto>> Submit(Guid orderId, CancellationToken ct)
    {
        var dto = await _cqrs.Send(new SubmitOrderCommand(orderId), ct);
        return Ok(dto);
    }
}