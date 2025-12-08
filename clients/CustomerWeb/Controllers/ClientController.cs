namespace CustomerWeb.Controllers;

using CustomerWeb.Models;
using CustomerWeb.Services;
using Microsoft.AspNetCore.Mvc;
using TableOrdering.Contracts;

public class ClientController : Controller
{
    private readonly IBackendApiClient _apiClient;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IBackendApiClient apiClient, ILogger<ClientController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

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

    // Action chính hiển thị trang Menu với dữ liệu thật
    // /client/menu?tableCode=T01&categoryId=...
    [HttpGet("/client/menu")]
    public async Task<IActionResult> Menu([FromQuery] string tableCode, [FromQuery] Guid? categoryId)
    {
        if (string.IsNullOrWhiteSpace(tableCode)) return BadRequest("Thiếu mã bàn.");
        tableCode = tableCode.Trim();

        Guid orderId;
        try
        {
            orderId = await _apiClient.StartCartAsync(tableCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khởi tạo giỏ hàng");
            return RedirectToAction("Index", "Home");
        }

        var categoriesTask = _apiClient.GetPublicCategoriesAsync();
        var cartTask = _apiClient.GetCartAsync(orderId);
        await Task.WhenAll(categoriesTask, cartTask);

        var categories = categoriesTask.Result ?? Array.Empty<CategoryDto>();
        var currentCart = cartTask.Result;

        var selectedCatId = categoryId ?? categories.FirstOrDefault()?.Id;
        var menuItems = selectedCatId.HasValue
            ? await _apiClient.GetMenuByCategoryAsync(selectedCatId.Value)
            : Array.Empty<MenuItemDto>();

        var model = new MenuIndexVm
        {
            TableCode = tableCode,
            CurrentCategoryId = selectedCatId,
            Categories = categories,
            MenuItems = menuItems,
            CurrentCart = currentCart
        };

        // Lưu orderCode (orderId) vào ViewBag để sử dụng trong view nếu cần
        ViewBag.OrderCode = orderId;
        return View(model);
    }

    // --- CÁC ACTION AJAX ---
    [HttpPost]
    public async Task<IActionResult> AddToCart(Guid orderCode, Guid menuItemId, int quantity, string? note)
    {
        try
        {
            await _apiClient.AddCartItemAsync(orderCode, menuItemId, quantity, note);
            var updatedCart = await _apiClient.GetCartAsync(orderCode);
            return PartialView("~/Views/Shared/_CartPartial.cshtml", updatedCart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddToCart failed");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCartItem(Guid orderCode, int cartItemId, int quantity, string? note)
    {
        try
        {
            await _apiClient.UpdateCartItemAsync(orderCode, cartItemId, quantity, note);
            var updatedCart = await _apiClient.GetCartAsync(orderCode);
            return PartialView("~/Views/Shared/_CartPartial.cshtml", updatedCart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateCartItem failed");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SubmitOrder(Guid orderCode)
    {
        try
        {
            await _apiClient.SubmitCartAsync(orderCode);
            var updatedCart = await _apiClient.GetCartAsync(orderCode);
            return PartialView("~/Views/Shared/_CartPartial.cshtml", updatedCart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SubmitOrder failed");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCartPartial(string tableCode)
    {
        if (string.IsNullOrWhiteSpace(tableCode)) return BadRequest("Thiếu mã bàn.");
        var orderId = await _apiClient.StartCartAsync(tableCode.Trim());
        var cart = await _apiClient.GetCartAsync(orderId);
        return PartialView("~/Views/Shared/_CartPartial.cshtml", cart);
    }

    [HttpGet("/client/qr-help")]
    public IActionResult ScanHelp() => View();

    // Trang quét QR trực tiếp bằng camera
    [HttpGet("/client/qr")]
    public IActionResult Scan() => View();
}

