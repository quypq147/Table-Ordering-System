using Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Application.Services;

public sealed class ShortFriendlyOrderCodeGenerator(IApplicationDbContext db) : IOrderCodeGenerator
{
    private static string SanitizePrefix(string tableCode, int maxLen = 4)
    {
        var s = (tableCode ?? string.Empty).ToUpperInvariant();
        s = Regex.Replace(s, "[^A-Z0-9]", "");
        if (s.Length == 0) s = "T";
        return s.Length > maxLen ? s[..maxLen] : s;
    }

    public async Task<string> GenerateAsync(Guid tableId, string tableCode, CancellationToken ct)
    {
        var prefix = SanitizePrefix(tableCode);  // ví dụ "T01"
        // Thử 6 lần với base36 3 kí tự (0..Z) → 46.656 khả năng
        for (var i = 0; i < 6; i++)
        {
            var n = RandomNumberGenerator.GetInt32(36 * 36 * 36);
            var suffix = Base36(n, 3);           // ví dụ "7F2"
            var code = $"{prefix}-{suffix}";
            if (!await db.Orders.AnyAsync(o => o.Code == code, ct)) return code;
        }
        // Fallback có ngày + base36 4 kí tự (siêu hiếm khi đụng tới)
        var ymd = DateTime.UtcNow.ToString("yyMMdd");
        var extra = RandomNumberGenerator.GetInt32(36 * 36 * 36 * 36);
        return $"{ymd}-{prefix}-{Base36(extra, 4)}";
    }

    private static string Base36(int value, int pad)
    {
        const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        Span<char> buf = stackalloc char[10];
        var i = buf.Length;
        int v = value;
        do { buf[--i] = alphabet[v % 36]; v /= 36; } while (v > 0);
        var s = new string(buf[i..]);
        return s.Length >= pad ? s : s.PadLeft(pad, '0');
    }
}

