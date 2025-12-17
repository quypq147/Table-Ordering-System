using System.Net;
using System.Net.Http.Headers;

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
    public async Task<bool> PayOrderAsync(Guid orderId, PaymentMethod method)
    {
        // TODO: Khi backend đã sẵn sàng (US18), hãy uncomment dòng dưới để gọi API thật
        // var response = await Http.PostAsJsonAsync($"api/orders/{orderId}/pay", new { Method = method.ToString() });
        // return response.IsSuccessStatusCode;

        // HIỆN TẠI: Giả lập thanh toán luôn thành công theo yêu cầu
        await Task.Delay(500); // Giả lập độ trễ mạng
        return true;
    }
}
