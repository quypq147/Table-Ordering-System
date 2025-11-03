using System.Text.Json;
using AdminWeb.Dtos;
using AdminWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class MenuController(IBackendApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await api.GetMenuAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var cats = await api.GetCategoriesAsync(onlyActive: true);

        // Lọc danh mục hợp lệ (Id != Guid.Empty)
        var validCats = cats.Where(c => c.Id != Guid.Empty).ToList();
        if (validCats.Count == 0)
        {
            TempData["Error"] = "Danh mục đang bị lỗi Id rỗng. Hãy tạo lại danh mục hoặc sửa API trả về Id.";
            return RedirectToAction("Index", "Categories");
        }

        // Chỉ chọn mặc định khi có Id hợp lệ
        var vm = new CreateMenuItemRequest(
            CategoryId: validCats[0].Id,
            Name: "",
            Sku: "",
            Price: 0,
            Currency: "VND"
        );

        ViewBag.Categories = new SelectList(validCats, nameof(CategoryDto.Id), nameof(CategoryDto.Name), vm.CategoryId);
        ViewBag.Currencies = new SelectList(new[] { "VND" }, vm.Currency);

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMenuItemRequest req)
    {
        // reload dropdown
        async Task LoadDropdownsAsync()
        {
            var categories = await api.GetCategoriesAsync(onlyActive: true);
            ViewBag.Categories = new SelectList(categories, nameof(CategoryDto.Id), nameof(CategoryDto.Name), req.CategoryId);
            ViewBag.Currencies = new SelectList(new[] { "VND" }, req.Currency);
        }

        // NEW: block Guid.Empty + category not existing (deleted/changed between GET-POST)
        if (req.CategoryId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(req.CategoryId), "Vui lòng chọn danh mục hợp lệ.");
            await LoadDropdownsAsync();
            return View(req);
        }
        else
        {
            var categories = await api.GetCategoriesAsync(onlyActive: true);
            if (!categories.Any(c => c.Id == req.CategoryId))
            {
                ModelState.AddModelError(nameof(req.CategoryId), "Danh mục đã bị thay đổi hoặc không còn tồn tại.");
                ViewBag.Categories = new SelectList(categories, nameof(CategoryDto.Id), nameof(CategoryDto.Name), null);
                ViewBag.Currencies = new SelectList(new[] { "VND" }, req.Currency);
                return View(req);
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(req);
        }

        var res = await api.CreateMenuItemAsync(req);
        if (!res.IsSuccessStatusCode)
        {
            // try to parse RFC9110 problem+errors to put errors to correct fields
            var text = await res.Content.ReadAsStringAsync();
            TryAddProblemErrorsToModelState(text);

            if (ModelState.ErrorCount == 0)
            {
                ModelState.AddModelError("", string.IsNullOrWhiteSpace(text) ? "Tạo món thất bại" : text);
            }

            await LoadDropdownsAsync();
            return View(req);
        }

        TempData["Success"] = "Đã tạo món thành công.";
        return RedirectToAction(nameof(Index));
    }

    // Parse problem details: {"errors":{"Currency":["The Currency field is required."], ...}}
    private void TryAddProblemErrorsToModelState(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("errors", out var errors))
            {
                foreach (var prop in errors.EnumerateObject())
                {
                    var field = prop.Name;
                    foreach (var msg in prop.Value.EnumerateArray())
                    {
                        var message = msg.GetString();
                        if (!string.IsNullOrWhiteSpace(message))
                            ModelState.AddModelError(field, message);
                    }
                }
            }
        }
        catch
        {
            // ignore parse error, will add general error above
        }
    }
}


