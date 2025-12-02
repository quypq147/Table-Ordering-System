using Application.Abstractions;
using Application.Dtos;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Categories.Commands;

public sealed class CreateCategoryHandler : ICommandHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _db;

    public CreateCategoryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CategoryDto> Handle(CreateCategoryCommand c, CancellationToken ct)
    {
        // 1) Generate code from name (A-Z0-9 with dash, no diacritics)
        var baseCode = ToCode(c.Name);
        if (string.IsNullOrEmpty(baseCode)) baseCode = "CAT";

        // 2) Ensure uniqueness
        var code = baseCode;
        var i = 1;
        while (await _db.Categories.AnyAsync(x => x.Code == code, ct))
            code = $"{baseCode}-{i++}";

        // 3) Create entity (ctor accepts id, code, name, description, sortOrder)
        var category = new Category(Guid.NewGuid(), code, c.Name, c.Description, c.SortOrder);

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);

        return new CategoryDto(category.Id, category.Name, category.Description, category.IsActive, category.SortOrder);
    }

    // Helpers
    private static string ToCode(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var s = input.Trim().ToUpperInvariant();

        // Remove diacritics
        var normalized = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }
        var noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

        // Keep A-Z0-9, replace other groups by "-"
        var cleaned = Regex.Replace(noDiacritics, @"[^A-Z0-9]+", "-");
        cleaned = Regex.Replace(cleaned, @"-+", "-").Trim('-');

        // Limit to 64 chars (matches EF config)
        return cleaned.Length > 64 ? cleaned[..64].Trim('-') : cleaned;
    }
}