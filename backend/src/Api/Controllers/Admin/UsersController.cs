using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public sealed class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _users;
    private readonly RoleManager<AppRole> _roles;

    public UsersController(UserManager<AppUser> users, RoleManager<AppRole> roles)
    {
        _users = users; _roles = roles;
    }

    public sealed class UpdateRolesDto
    {
        [Required]
        public List<string> Roles { get; set; } = new();
    }

    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> SetRoles(Guid id, [FromBody] UpdateRolesDto body)
    {
        var user = await _users.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var allRoles = _roles.Roles.Select(r => r.Name!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var desired = body.Roles.Where(r => allRoles.Contains(r))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

        var current = await _users.GetRolesAsync(user);
        var toRemove = current.Where(r => !desired.Contains(r, StringComparer.OrdinalIgnoreCase)).ToArray();
        var toAdd = desired.Where(r => !current.Contains(r, StringComparer.OrdinalIgnoreCase)).ToArray();

        if (toRemove.Length > 0)
        {
            var rm = await _users.RemoveFromRolesAsync(user, toRemove);
            if (!rm.Succeeded) return Problem(string.Join("; ", rm.Errors.Select(e => e.Description)));
        }
        if (toAdd.Length > 0)
        {
            var ad = await _users.AddToRolesAsync(user, toAdd);
            if (!ad.Succeeded) return Problem(string.Join("; ", ad.Errors.Select(e => e.Description)));
        }

        return NoContent();
    }
}
