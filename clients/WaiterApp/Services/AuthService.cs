using System.Diagnostics;
using System.Net.Http.Json;

namespace WaiterApp.Services;

public class AuthService
{
    private readonly ApiClient _apiClient;
    private string? _token;

    // Constructor used by App to create a singleton: new(AuthServiceClient: ApiClient)
    public AuthService(ApiClient authServiceClient)
    {
        _apiClient = authServiceClient ?? throw new ArgumentNullException(nameof(authServiceClient));
    }

    public string? Token => _token;

    // Attempts login; on success sets bearer token on the ApiClient and returns (true, null).
    // On failure returns (false, errorMessage).
    public async Task<(bool Success, string? Error)> LoginAsync(string userNameOrEmail, string password)
    {
        try
        {
            var client = _apiClient.Http;

            var body = new
            {
                UserNameOrEmail = userNameOrEmail,
                Password = password
            };

            var resp = await client.PostAsJsonAsync(WaiterApiEndpoints.Auth.Login, body);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? resp.ReasonPhrase : err);
            }

            // Expecting response like: { token: "...", refreshToken: "..." }
            var dto = await resp.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (dto == null || string.IsNullOrWhiteSpace(dto.Token))
            {
                return (false, "Empty token returned from server.");
            }

            _token = dto.Token;
            _apiClient.SetBearerToken(_token);
            return (true, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Debug.WriteLine(ex.ToString());
            return (false, ex.Message);
        }
    }

    public void Logout()
    {
        _token = null;
        _apiClient.SetBearerToken(null);
    }

    private class LoginResponseDto
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
