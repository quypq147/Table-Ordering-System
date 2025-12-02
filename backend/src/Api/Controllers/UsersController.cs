using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _users;
    private readonly RoleManager<AppRole> _roles;

    public UsersController(UserManager<AppUser> users, RoleManager<AppRole> roles)
    {
        _users = users; _roles = roles;
    }

    public sealed record UserDto(Guid Id, string? UserName, string? Email, string? FullName, bool IsActive, IEnumerable<string> Roles);
    public sealed record CreateDto(string UserName, string Email, string Password, string? FullName, IEnumerable<string>? Roles);
    public sealed record UpdateDto(string? Email, string? FullName, bool? IsActive);
    public sealed record UpdateRolesDto(IEnumerable<string> Roles);
    public sealed record ResetPasswordDto([property: Required, MinLength(6)] string NewPassword);
    public sealed record LockDto(DateTimeOffset? UntilUtc);

    [HttpGet]
    public async Task<IEnumerable<UserDto>> List()
    {
        var all = _users.Users.ToList();
        var list = new List<UserDto>();
        foreach (var u in all)
        {
            var r = await _users.GetRolesAsync(u);
            list.Add(new UserDto(u.Id, u.UserName, u.Email, u.FullName, u.IsActive, r));
        }
        return list;
    }

    [HttpGet("paged")]
    public async Task<IActionResult> ListPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 200) pageSize = 20;

        var query = _users.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(u => (u.UserName ?? "").Contains(term) || (u.Email ?? "").Contains(term) || (u.FullName ?? "").Contains(term));
        }

        var total = query.Count();
        var pageItems = query
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<UserDto>(pageItems.Count);
        foreach (var u in pageItems)
        {
            var r = await _users.GetRolesAsync(u);
            items.Add(new UserDto(u.Id, u.UserName, u.Email, u.FullName, u.IsActive, r));
        }

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("roles")]
    public ActionResult<IEnumerable<string>> GetRoles()
    {
        var roles = _roles.Roles.Select(r => r.Name!).OrderBy(n => n).ToArray();
        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDto dto)
    {
        var u = new AppUser { UserName = dto.UserName, Email = dto.Email, FullName = dto.FullName, IsActive = true };
        var result = await _users.CreateAsync(u, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        if (dto.Roles is { } rs && rs.Any())
        {
            foreach (var role in rs)
                if (!await _roles.RoleExistsAsync(role))
                    await _roles.CreateAsync(new AppRole { Name = role });
            await _users.AddToRolesAsync(u, rs);
        }
        return CreatedAtAction(nameof(GetById), new { id = u.Id }, new { u.Id });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var u = await _users.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();
        var r = await _users.GetRolesAsync(u);
        return new UserDto(u.Id, u.UserName, u.Email, u.FullName, u.IsActive, r);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDto dto)
    {
        var u = await _users.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();

        if (dto.Email is not null) u.Email = dto.Email;
        if (dto.FullName is not null) u.FullName = dto.FullName;
        if (dto.IsActive.HasValue) u.IsActive = dto.IsActive.Value;

        var result = await _users.UpdateAsync(u);
        return result.Succeeded ? NoContent() : BadRequest(result.Errors);
    }

    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> UpdateRoles(Guid id, [FromBody] UpdateRolesDto dto)
    {
        var u = await _users.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();

        var current = await _users.GetRolesAsync(u);
        var target = dto.Roles?.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? Array.Empty<string>();

        foreach (var role in target)
            if (!await _roles.RoleExistsAsync(role))
                await _roles.CreateAsync(new AppRole { Name = role });

        await _users.RemoveFromRolesAsync(u, current);
        await _users.AddToRolesAsync(u, target);

        return NoContent();
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordDto dto)
    {
        var u = await _users.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();

        var token = await _users.GeneratePasswordResetTokenAsync(u);
        var res = await _users.ResetPasswordAsync(u, token, dto.NewPassword);
        if (!res.Succeeded) return BadRequest(res.Errors);
        return NoContent();
    }

    [HttpPost("{id:guid}/lock")]
    public async Task<IActionResult> Lock(Guid id, [FromBody] LockDto body)
    {
        var u = await _users.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();

        var until = body.UntilUtc ?? DateTimeOffset.MaxValue;
        await _users.SetLockoutEnabledAsync(u, true);
        var res = await _users.SetLockoutEndDateAsync(u, until);
        return res.Succeeded ? NoContent() : BadRequest(res.Errors);
    }

    [HttpPost("{id:guid}/unlock")]
    public async Task<IActionResult> Unlock(Guid id)
    {
        var u = await _users.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();

        await _users.SetLockoutEnabledAsync(u, true);
        var res = await _users.SetLockoutEndDateAsync(u, null);
        return res.Succeeded ? NoContent() : BadRequest(res.Errors);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var u = await _users.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();
        u.IsActive = true;
        var res = await _users.UpdateAsync(u);
        return res.Succeeded ? NoContent() : BadRequest(res.Errors);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var u = await _users.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();
        u.IsActive = false;
        var res = await _users.UpdateAsync(u);
        return res.Succeeded ? NoContent() : BadRequest(res.Errors);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var u = await _users.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();
        var result = await _users.DeleteAsync(u);
        return result.Succeeded ? NoContent() : BadRequest(result.Errors);
    }
}