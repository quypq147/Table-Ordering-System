using AdminWeb.Dtos;
using AdminWeb.Services;
using Microsoft.AspNetCore.Mvc;

public class MenuController(IBackendApiClient api) : Controller
{
    
    

    public async Task<IActionResult> Index()
    {
        var items = await api.GetMenuAsync();
        return View(items);
    }

    public IActionResult Create() => View(new CreateMenuItemRequest( Guid.Empty, "", "", 0));

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

