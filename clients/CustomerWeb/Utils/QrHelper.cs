namespace CustomerWeb.Utils;

public static class QrHelper
{
    public static string BuildTableQrUrl(string websiteUrl, Guid tableId, Guid? currentSessionId)
    {
        var baseUrl = websiteUrl?.TrimEnd('/') ?? string.Empty;
        var sid = currentSessionId is Guid s ? s.ToString() : string.Empty;
        var query = string.IsNullOrEmpty(sid) ? string.Empty : $"?session={sid}";
        return $"{baseUrl}/table/{tableId}{query}";
    }
}
