    using Application.Common.CQRS;
using Application.Dtos;                         // OrderDto
using Application.Orders.Commands;              // Start/Submit/Mark*/Pay/Cancel/AddItem/ChangeQty/RemoveItem
using Application.Orders.Queries;               // Queries
using Microsoft.AspNetCore.Mvc;
namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    public OrdersController(ISender sender) => _sender = sender;

    // ===== Queries =====

    [HttpGet("summaries")]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> Summaries([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        => Ok(await _sender.Send(new ListOrderSummariesQuery(page, pageSize)));

    // GET /api/orders/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
        => Ok(await _sender.Send(new GetOrderByIdQuery(id))); // includes Items

    // GET /api/orders?tableId=T01&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> ListByTable(
        [FromQuery] Guid tableId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _sender.Send(new ListOrdersByTableQuery(tableId, page, pageSize)));

    // GET /api/orders/table/{tableId}
    // Endpoint dành riêng cho WaiterApp: trả thẳng ra danh sách OrderItemDto
    [HttpGet("table/{tableId}")]
    public async Task<ActionResult<IReadOnlyList<OrderItemDto>>> ListItemsByTable(
        Guid tableId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var orders = await _sender.Send(new ListOrdersByTableQuery(tableId, page, pageSize));
        // đơn giản: gộp tất cả items của các order (thông thường 1 bàn chỉ có 1 order đang mở)
        var items = orders.SelectMany(o => o.Items).ToList();
        return Ok(items);
    }

    // ===== Commands: Order lifecycle ===== (unchanged below)
    public sealed record StartDto(Guid OrderId, Guid TableId);
    [HttpPost("start")]
    public async Task<ActionResult<OrderDto>> Start([FromBody] StartDto body)
        => Ok(await _sender.Send(new StartOrderCommand(body.OrderId, body.TableId)));

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<OrderDto>> Submit(Guid id)
        => Ok(await _sender.Send(new SubmitOrderCommand(id)));

    [HttpPost("{id}/in-progress")]
    public async Task<ActionResult<OrderDto>> MarkInProgress(Guid id)
        => Ok(await _sender.Send(new MarkInProgressCommand(id)));

    [HttpPost("{id}/ready")]
    public async Task<ActionResult<OrderDto>> MarkReady(Guid id)
        => Ok(await _sender.Send(new MarkReadyCommand(id)));

    [HttpPost("{id}/served")]
    public async Task<ActionResult<OrderDto>> MarkServed(Guid id)
        => Ok(await _sender.Send(new MarkServedCommand(id)));

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id)
        => Ok(await _sender.Send(new CancelOrderCommand(id)));

    public sealed record PayDto(decimal Amount, string Currency);
    [HttpPost("{id}/pay")]
    public async Task<ActionResult<OrderDto>> Pay(Guid id, [FromBody] PayDto body)
        => Ok(await _sender.Send(new PayOrderCommand(id, body.Amount, body.Currency)));

    public sealed record AddItemDto(Guid MenuItemId, string Name, decimal Price, string Currency, int Quantity);
    [HttpPost("{id}/items")]
    public async Task<ActionResult<OrderDto>> AddItem(Guid id, [FromBody] AddItemDto body)
        => Ok(await _sender.Send(
            new AddItemCommand(id, body.MenuItemId, body.Name, body.Price, body.Currency, body.Quantity)));

    public sealed record ChangeQtyDto(int NewQuantity);
    [HttpPatch("{id}/items/{orderItemId:int}")]
    public async Task<ActionResult<OrderDto>> ChangeItemQuantity(Guid id, int orderItemId, [FromBody] ChangeQtyDto body)
        => Ok(await _sender.Send(new ChangeItemQuantityCommand(id, orderItemId, body.NewQuantity)));

    public sealed record RemoveItemDto(int OrderItemId);
    [HttpDelete("{id}/items")]
    public async Task<ActionResult<OrderDto>> RemoveItem(Guid id, [FromBody] RemoveItemDto body)
        => Ok(await _sender.Send(new RemoveItemCommand(id, body.OrderItemId)));

    [HttpGet("all")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> ListAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _sender.Send(new ListOrdersQuery(page, pageSize)));
}


