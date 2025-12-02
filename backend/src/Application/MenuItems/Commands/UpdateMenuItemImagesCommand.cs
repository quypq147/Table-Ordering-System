using Application.Abstractions;
using Application.Dtos;
using Application.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Application.MenuItems.Commands;

public sealed record UpdateMenuItemImagesCommand(
 Guid Id,
 Stream? Avatar,
 string? AvatarFileName,
 string? AvatarContentType,
 Stream? Background,
 string? BackgroundFileName,
 string? BackgroundContentType
) : ICommand<MenuItemDto>;

public sealed class UpdateMenuItemImagesHandler : ICommandHandler<UpdateMenuItemImagesCommand, MenuItemDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorage _files;
    public UpdateMenuItemImagesHandler(IApplicationDbContext db, IFileStorage files)
    {
        _db = db; _files = files;
    }

    public async Task<MenuItemDto> Handle(UpdateMenuItemImagesCommand c, CancellationToken ct)
    {
        var m = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
        ?? throw new InvalidOperationException("Menu item not found");

        if (c.Avatar is not null && !string.IsNullOrWhiteSpace(c.AvatarFileName))
        {
            var url = await _files.SaveMenuItemImageAsync(m.Id, c.Avatar, c.AvatarContentType ?? "application/octet-stream", c.AvatarFileName!, "avatar", ct);
            m.SetAvatarImage(url);
        }
        if (c.Background is not null && !string.IsNullOrWhiteSpace(c.BackgroundFileName))
        {
            var url = await _files.SaveMenuItemImageAsync(m.Id, c.Background, c.BackgroundContentType ?? "application/octet-stream", c.BackgroundFileName!, "background", ct);
            m.SetBackgroundImage(url);
        }

        await _db.SaveChangesAsync(ct);
        return MenuItemMapper.ToDto(m);
    }
}
