using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace WaiterApp.Services;

public enum PaymentMethod
{
    Cash,
    Transfer
}

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

    public void SetBaseAddress(string baseAddress)
    {
        if (string.IsNullOrWhiteSpace(baseAddress)) return;
        if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var uri)) return;
        Http.BaseAddress = uri;
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
    // --- THÊM MỚI: Hàm xử lý thanh toán ---
    public async Task<bool> PayOrderAsync(Guid orderId, decimal amount, string currency = "VND")
    {
        try
        {
            // Tạo body đúng với PayDto bên Backend (Amount, Currency)
            var body = new { Amount = amount, Currency = currency };

            // Gọi API thật
            var response = await Http.PostAsJsonAsync($"api/orders/{orderId}/pay", body);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
