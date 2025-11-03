using Application.Abstractions;                 // ISender, ICommand/Query marker
using Application.Dtos;                         // OrderDto
using Application.Orders.Commands;              // Start/Submit/Mark*/Pay/Cancel/AddItem/ChangeQty/RemoveItem
using Application.Orders.Queries;               // GetOrderByIdQuery, ListOrdersByTableQuery
using Microsoft.AspNetCore.Mvc;
using Application.Common.CQRS;
namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    public OrdersController(ISender sender) => _sender = sender;

    // ===== Queries =====

    // GET /api/orders/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
        => Ok(await _sender.Send(new GetOrderByIdQuery(id))); // includes Items
    // (Query handler đã có) :contentReference[oaicite:15]{index=15}

    // GET /api/orders?tableId=T01&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> ListByTable([FromQuery] Guid tableId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _sender.Send(new ListOrdersByTableQuery(tableId, page, pageSize)));
    // (Query handler đã có) :contentReference[oaicite:16]{index=16}

    // ===== Commands: Order lifecycle =====

    // POST /api/orders/start
    public sealed record StartDto(Guid OrderId, Guid TableId);
    [HttpPost("start")]
    public async Task<ActionResult<OrderDto>> Start([FromBody] StartDto body)
        => Ok(await _sender.Send(new StartOrderCommand(body.OrderId, body.TableId)));  // :contentReference[oaicite:17]{index=17}

    // POST /api/orders/{id}/submit
    [HttpPost("{id}/submit")]
    public async Task<ActionResult<OrderDto>> Submit(Guid id)
        => Ok(await _sender.Send(new SubmitOrderCommand(id)));                          // :contentReference[oaicite:18]{index=18}

    // POST /api/orders/{id}/in-progress
    [HttpPost("{id}/in-progress")]
    public async Task<ActionResult<OrderDto>> MarkInProgress(Guid id)
        => Ok(await _sender.Send(new MarkInProgressCommand(id)));                       // :contentReference[oaicite:19]{index=19}

    // POST /api/orders/{id}/ready
    [HttpPost("{id}/ready")]
    public async Task<ActionResult<OrderDto>> MarkReady(Guid id)
        => Ok(await _sender.Send(new MarkReadyCommand(id)));                            // :contentReference[oaicite:20]{index=20}

    // POST /api/orders/{id}/served
    [HttpPost("{id}/served")]
    public async Task<ActionResult<OrderDto>> MarkServed(Guid id)
        => Ok(await _sender.Send(new MarkServedCommand(id)));                           // :contentReference[oaicite:21]{index=21}

    // POST /api/orders/{id}/cancel
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id)
        => Ok(await _sender.Send(new CancelOrderCommand(id)));                          // :contentReference[oaicite:22]{index=22}

    // POST /api/orders/{id}/pay
    public sealed record PayDto(decimal Amount, string Currency);
    [HttpPost("{id}/pay")]
    public async Task<ActionResult<OrderDto>> Pay(Guid id, [FromBody] PayDto body)
        => Ok(await _sender.Send(new PayOrderCommand(id, body.Amount, body.Currency))); // :contentReference[oaicite:23]{index=23}

    // ===== Commands: Order items =====

    // POST /api/orders/{id}/items
    // NOTE: AddItemCommand hiện yêu cầu Name/Price/Currency từ client (nên đổi về tra giá từ MenuItem ở Handler)
    public sealed record AddItemDto(Guid MenuItemId, string Name, decimal Price, string Currency, int Quantity);
    [HttpPost("{id}/items")]
    public async Task<ActionResult<OrderDto>> AddItem(Guid id, [FromBody] AddItemDto body)
        => Ok(await _sender.Send(
            new AddItemCommand(id, body.MenuItemId, body.Name, body.Price, body.Currency, body.Quantity))); // :contentReference[oaicite:24]{index=24}

    // PATCH /api/orders/{id}/items/{orderItemId}
    public sealed record ChangeQtyDto(int NewQuantity);
    [HttpPatch("{id}/items/{orderItemId:int}")]
    public async Task<ActionResult<OrderDto>> ChangeItemQuantity(Guid id, int orderItemId, [FromBody] ChangeQtyDto body)
        => Ok(await _sender.Send(new ChangeItemQuantityCommand(id, orderItemId, body.NewQuantity)));        // :contentReference[oaicite:25]{index=25}

    // DELETE /api/orders/{id}/items
    // Lưu ý: RemoveItemCommand nhận MenuItemId -> nếu có nhiều dòng trùng MenuItemId sẽ mơ hồ
    public sealed record RemoveItemDto(Guid MenuItemId);
    [HttpDelete("{id}/items")]
    public async Task<ActionResult<OrderDto>> RemoveItem(Guid id, [FromBody] RemoveItemDto body)
        => Ok(await _sender.Send(new RemoveItemCommand(id, body.MenuItemId)));  // :contentReference[oaicite:26]{index=26}

    // GET /api/orders/all?page=1&pageSize=20
    [HttpGet("all")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> ListAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _sender.Send(new ListOrdersQuery(page, pageSize)));
}


