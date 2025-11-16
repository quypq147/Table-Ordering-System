using System.Net.Http.Json;

namespace WaiterApp.Services;

public class AuthService
{
    private readonly ApiClient _apiClient;

    public string? Token { get; private set; }

    public AuthService(ApiClient AuthServiceClient)
    {
        _apiClient = AuthServiceClient;
    }

    private record LoginResponse(string Token);

    public async Task<bool> LoginAsync(string username, string password)
    {
        var body = new { username, password };

        var response = await _apiClient.Http.PostAsJsonAsync("api/auth/login", body);
        if (!response.IsSuccessStatusCode)
            return false;

        var json = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Token = json?.Token;

        if (Token is null) return false;

        _apiClient.SetBearerToken(Token);
        await SecureStorage.SetAsync("jwt_token", Token);

        return true;
    }
}
