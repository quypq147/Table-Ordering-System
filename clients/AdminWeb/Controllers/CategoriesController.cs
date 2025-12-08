// AdminWeb/Controllers/CategoriesController.cs
using TableOrdering.Contracts;
using AdminWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminWeb.Controllers;

public class CategoriesController(IBackendApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, bool? onlyActive, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var categories = await api.GetCategoriesAsync(search, onlyActive, page, pageSize, ct);
        ViewBag.Search = search;
        ViewBag.OnlyActive = onlyActive;
        return View(categories);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateCategoryRequest("", 0));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(req);

        var res = await api.CreateCategoryAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Tạo danh mục thất bại");
            return View(req);
        }
        TempData["Success"] = "Tạo danh mục thành công";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var cat = await api.GetCategoryAsync(id, ct);
        if (cat is null) return NotFound();

        // Chỉ cho phép đổi tên (giữ DisplayOrder nếu bạn muốn UI riêng)
        var vm = new RenameCategoryRequest(cat.Name);
        ViewBag.CategoryId = id;
        ViewBag.DisplayOrder = cat.DisplayOrder;
        ViewBag.IsActive = cat.IsActive;
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, RenameCategoryRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CategoryId = id;
            return View(req);
        }

        var res = await api.RenameCategoryAsync(id, req, ct);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Đổi tên danh mục thất bại");
            ViewBag.CategoryId = id;
            return View(req);
        }
        TempData["Success"] = "Cập nhật danh mục thành công";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var res = await api.ActivateCategoryAsync(id, ct);
        if (!res.IsSuccessStatusCode) TempData["Error"] = "Kích hoạt thất bại"; else TempData["Success"] = "Đã kích hoạt";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var res = await api.DeactivateCategoryAsync(id, ct);
        if (!res.IsSuccessStatusCode) TempData["Error"] = "Vô hiệu hoá thất bại"; else TempData["Success"] = "Đã vô hiệu hoá";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var res = await api.DeleteCategoryAsync(id, ct);
        if (!res.IsSuccessStatusCode)
        {
            TempData["Error"] = "Xóa danh mục thất bại";
        }
        else
        {
            TempData["Success"] = "Đã xóa danh mục";
        }
        return RedirectToAction(nameof(Index));
    }
}

