using System.Net;
using System.Net.Http.Headers;

namespace WaiterApp.Services;

public class ApiClient
{
    public HttpClient Http { get; }

    public ApiClient(string baseAddress)
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        };

        Http = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseAddress),
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Optional default headers
        Http.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
        Http.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        Http.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
    }

    public void SetBearerToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            Http.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            Http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
