namespace CustomerWeb.Controllers;

using Microsoft.AspNetCore.Mvc;

public class ClientController : Controller
{
    // /c?code=T01 hoặc /c?c=T01
    [HttpGet("/c")]
    public IActionResult CheckIn([FromQuery(Name = "code")] string? code,
                                 [FromQuery(Name = "c")] string? c)
    {
        var raw = string.IsNullOrWhiteSpace(code) ? c : code;
        if (string.IsNullOrWhiteSpace(raw))
            return RedirectToAction(nameof(ScanHelp));

        var tableCode = Uri.UnescapeDataString(raw).Trim();
        return RedirectToAction(nameof(Menu), new { tableCode });
    }

    // /c/T01
    [HttpGet("/c/{code}")]
    public IActionResult CheckInRoute([FromRoute] string code)
        => RedirectToAction(nameof(Menu), new { tableCode = code });

    // /client/menu?tableCode=T01
    [HttpGet("/client/menu")]
    public IActionResult Menu([FromQuery] string tableCode)
    {
        if (string.IsNullOrWhiteSpace(tableCode)) return BadRequest("Thiếu mã bàn.");
        ViewBag.TableCode = tableCode.Trim();
        return View();
    }

    [HttpGet("/client/qr-help")]
    public IActionResult ScanHelp() => View();

    // Trang quét QR trực tiếp bằng camera
    [HttpGet("/client/qr")]
    public IActionResult Scan() => View();
}

