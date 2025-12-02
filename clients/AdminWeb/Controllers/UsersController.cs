using AdminWeb.Dtos;
using AdminWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminWeb.Controllers
{
    [Authorize(Policy = "RequireSignedIn")]
    public class UsersController(IBackendApiClient api) : Controller
    {
        [HttpGet("/Users")]
        public async Task<IActionResult> Index()
            => View(await api.ListUsersAsync());

        [HttpGet("/Users/Create")]
        public IActionResult Create() => View();

        [HttpPost("/Users/Create")]
        public async Task<IActionResult> Create(CreateUserVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            await api.CreateUserAsync(vm);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/Users/Edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id)
            => View(await api.GetUserAsync(id));

        [HttpPost("/Users/Edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id, UpdateUserVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            await api.UpdateUserAsync(id, vm);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/Users/Permissions/{id:guid}")]
        public async Task<IActionResult> Permissions(Guid id)
            => View(await api.GetUserAsync(id));

        [HttpPost("/Users/Permissions/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Permissions(Guid id, List<string> roles)
        {
            await api.SetRolesAsync(id, roles);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/Users/Deactivate/{id:guid}")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            await api.DeactivateUserAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
