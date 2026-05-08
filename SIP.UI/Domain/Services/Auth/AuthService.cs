using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using SIP.UI.Domain.DTOs.Users.Auth;
using SIP.UI.Domain.Interfaces.Auth;
using SIP.UI.Models.Auth;

namespace SIP.UI.Domain.Services.Auth;

public class AuthService(IJSRuntime jsRuntime, HttpClient http, NavigationManager navigation) : IAuthService
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly HttpClient _http = http;
    private readonly NavigationManager _navigation = navigation;
    private string? _token;
    private CurrentUser? _currentUser;
    private const string TokenStorageKey = "sip-token";
    private DotNetObjectReference<AuthService>? _dotNetRef;
    // Session timeout: 20 seconds for testing (change back to 20 * 60 for production)
    private const int InactivityTimeoutMs = 20 * 1000;
    // Warning before logout: 10 seconds
    private const int WarningTimeoutMs = 10 * 1000;

    public event Action? AuthenticationStateChanged;

    // Event raised when session is renewed (keep alive called successfully).
    public event Action? SessionRenewed;

    // Event raised when session is auto-renewed due to activity (silent renewal at timeout).
    public event Action? SessionAutoRenewed;

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
        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await _jsRuntime.InvokeVoidAsync("idleTimer.start", _dotNetRef, InactivityTimeoutMs, WarningTimeoutMs);
        }
        catch
        {
            // Ignore JS interop errors; authentication still works without inactivity tracking
        }
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

            try
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                await _jsRuntime.InvokeVoidAsync("idleTimer.start", _dotNetRef, InactivityTimeoutMs, WarningTimeoutMs);
            }
            catch
            {
                // ignore
            }

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
        try
        {
            await _jsRuntime.InvokeVoidAsync("idleTimer.stop");
        }
        catch
        {
            // ignore
        }

        _dotNetRef?.Dispose();
        _dotNetRef = null;

        AuthenticationStateChanged?.Invoke();

        // Ensure navigation to login page after logout
        try
        {
            _navigation.NavigateTo("/login", true);
        }
        catch
        {
            // ignore navigation errors in non-browser contexts
        }
    }

    // Called from JS when inactivity timeout is reached
    [JSInvokable]
    public async Task OnInactivityTimeout()
    {
        await LogoutAsync();
    }

    // Event raised when a warning should be shown before session expires.
    public event Action<int>? SessionExpiring;

    // Called from JS when warning time is reached. Receives seconds remaining until logout.
    [JSInvokable]
    public Task OnInactivityWarning(int secondsRemaining)
    {
        SessionExpiring?.Invoke(secondsRemaining);
        return Task.CompletedTask;
    }

    // Called from JS when session auto-renews due to activity during session period.
    [JSInvokable]
    public Task OnActivityRenewal()
    {
        SessionAutoRenewed?.Invoke();
        return Task.CompletedTask;
    }

    // Called by UI to keep the session alive (resets JS timers). Optionally here you could
    // call an API to refresh the JWT if your backend supports refresh tokens.
    public async Task KeepAliveAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("idleTimer.reset");
            // Dispatch event to notify components that session was renewed
            SessionRenewed?.Invoke();
        }
        catch
        {
            // ignore
        }
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
