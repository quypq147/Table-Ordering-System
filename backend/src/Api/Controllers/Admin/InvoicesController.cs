using Application.Invoices.Queries;
using Application.Invoices.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin;

[ApiController]
[Route("api/admin/invoices")]
public class InvoicesController : ControllerBase
{
    public InvoicesController()
    {
    }

    [HttpGet("by-order/{orderId}")]
    public async Task<IActionResult> GetByOrder(Guid orderId)
    {
        var sender = HttpContext.RequestServices.GetService(typeof(Application.Common.CQRS.ISender)) as Application.Common.CQRS.ISender;
        if (sender is null) return StatusCode(500, "ISender not configured");

        var dto = await sender.Send(new GetInvoiceByOrderIdQuery(orderId));
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    [HttpPost("generate/{orderId}")]
    public async Task<IActionResult> Generate(Guid orderId)
    {
        var sender = HttpContext.RequestServices.GetService(typeof(Application.Common.CQRS.ISender)) as Application.Common.CQRS.ISender;
        if (sender is null) return StatusCode(500, "ISender not configured");

        var result = await sender.Send(new GenerateInvoiceForOrderCommand(orderId));
        return result ? Accepted() : NotFound();
    }
}
