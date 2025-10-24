using AdminWeb.Dtos;
using Microsoft.AspNetCore.Mvc;

public class MenuController(BackendApiClient api, IConfiguration cfg) : Controller
{
    // Hardcode restaurant id DEMO (lấy từ claims hoặc config)
    private Guid RestaurantId => Guid.Parse(cfg["Demo:RestaurantId"] ?? "00000000-0000-0000-0000-000000000001");

    public async Task<IActionResult> Index()
    {
        var items = await api.GetMenuAsync(RestaurantId);
        return View(items);
    }

    public IActionResult Create() => View(new CreateMenuItemRequest(RestaurantId, Guid.Empty, "", "", 0));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMenuItemRequest req)
    {
        var res = await api.CreateMenuItemAsync(req);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Tạo món thất bại");
            return View(req);
        }
        return RedirectToAction(nameof(Index));
    }
}

