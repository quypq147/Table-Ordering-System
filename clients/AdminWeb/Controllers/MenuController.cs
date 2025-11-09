using System.Text.Json;
using TableOrdering.Contracts;
using AdminWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

[Authorize(Policy = "RequireSignedIn")]
public class MenuController(IBackendApiClient api, IWebHostEnvironment env) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, Guid? categoryId, bool? onlyActive)
    {
        // Load categories for filter dropdown
        var categories = await api.GetCategoriesAsync();
        ViewBag.Categories = categories;
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.OnlyActive = onlyActive ?? false;

        // Fetch filtered menu
        var items = await api.GetMenuAsync(search, categoryId, onlyActive);
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
            Currency: "VND",
            AvatarImageUrl: null,
            BackgroundImageUrl: null
        );

        ViewBag.Categories = new SelectList(validCats, nameof(CategoryDto.Id), nameof(CategoryDto.Name), vm.CategoryId);
        ViewBag.Currencies = new SelectList(new[] { "VND" }, vm.Currency);

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMenuItemRequest req, IFormFile? avatarFile, IFormFile? backgroundFile)
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

        // Validate and handle uploads (upload to Backend server, not AdminWeb)
        string? avatarUrl = null;
        string? bgUrl = null;
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        const long maxSize = 5L * 1024 * 1024; // 5MB

        async Task<string?> UploadToServerAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) return null;
            if (file.Length > maxSize)
            {
                ModelState.AddModelError("", $"Tệp {file.FileName} vượt quá dung lượng tối đa 5MB.");
                return null;
            }
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext) || !allowed.Contains(ext))
            {
                ModelState.AddModelError("", $"Định dạng tệp không hỗ trợ: {ext}. Chỉ chấp nhận JPG, PNG, WEBP, GIF.");
                return null;
            }

            try
            {
                // Upload to backend API
                await using var stream = file.OpenReadStream();
                var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
                var url = await api.UploadImageAsync(stream, file.FileName, contentType, folder);
                return url;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Tải ảnh lên máy chủ thất bại: {ex.Message}");
                return null;
            }
        }

        if (avatarFile != null)
        {
            avatarUrl = await UploadToServerAsync(avatarFile, folder: "menu");
        }
        if (backgroundFile != null)
        {
            bgUrl = await UploadToServerAsync(backgroundFile, folder: "menu");
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(req);
        }

        if (avatarUrl != null) req = req with { AvatarImageUrl = avatarUrl };
        if (bgUrl != null) req = req with { BackgroundImageUrl = bgUrl };

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


