using System.Net.Http.Headers;

namespace WaiterApp.Services;

public class ApiClient
{
    public HttpClient Http { get; }

    public ApiClient(string baseAddress)
    {
        Http = new HttpClient
        {
            BaseAddress = new Uri(baseAddress)
        };
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
