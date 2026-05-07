using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.JSInterop;
using SIP.UI.Domain.DTOs.Users.Auth;
using SIP.UI.Domain.Interfaces.Auth;
using SIP.UI.Models.Auth;

namespace SIP.UI.Domain.Services.Auth;

public class AuthService(IJSRuntime jsRuntime, HttpClient http) : IAuthService
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly HttpClient _http = http;
    private string? _token;
    private CurrentUser? _currentUser;
    private const string TokenStorageKey = "sip-token";

    public event Action? AuthenticationStateChanged;

    public CurrentUser? CurrentUser => _currentUser;

    public async Task InitializeAsync()
    {
        if (_token != null)
            return;

        _token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenStorageKey);

        if (string.IsNullOrWhiteSpace(_token))
            return;

        if (JwtParser.IsJwtExpired(_token))
        {
            await LogoutAsync();
            return;
        }

        SetAuthorizationHeader(_token);
        _currentUser = GetUserFromToken(_token);
    }

    public async Task<bool> LoginAsync(string login, string password)
    {
        try
        {
            var request = new LoginRequestDTO
            {
                Login = login,
                Password = password
            };

            HttpResponseMessage response = await _http.PostAsJsonAsync("sip_api/auth/login", request);

            if (!response.IsSuccessStatusCode)
                return false;

            AuthResponseDTO? authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();

            if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.AccessToken))
                return false;

            _token = authResponse.AccessToken;

            _currentUser = new CurrentUser
            {
                Id = authResponse.User.Id,
                Name = authResponse.User.Name,
                Login = authResponse.User.Login,
                Email = authResponse.User.Email,
                Role = authResponse.Role
            };

            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenStorageKey, _token);

            SetAuthorizationHeader(_token);

            AuthenticationStateChanged?.Invoke();

            return true;
        }
        catch
        {
            await LogoutAsync();
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        _token = null;
        _currentUser = null;
        _http.DefaultRequestHeaders.Authorization = null;
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenStorageKey);
        AuthenticationStateChanged?.Invoke();
    }

    public Task<string?> GetTokenAsync() => Task.FromResult(_token);

    private void SetAuthorizationHeader(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static CurrentUser? GetUserFromToken(string token)
    {
        IEnumerable<Claim> claims = JwtParser.ParseClaimsFromJwt(token);
        string? id = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        string? name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        string? login = claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        string? email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        string? role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(id))
            return null;

        return new CurrentUser
        {
            Id = Guid.TryParse(id, out var parsedId) ? parsedId : Guid.Empty,
            Name = name ?? string.Empty,
            Login = login ?? string.Empty,
            Email = email ?? string.Empty,
            Role = role ?? string.Empty
        };
    }
}
