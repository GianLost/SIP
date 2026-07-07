using SIP.UI.Domain.Enums;
using SIP.UI.Models.Users.Auth;

namespace SIP.UI.Domain.Interfaces.Users.Auth;

public interface IAuthService
{
    event Action? AuthenticationStateChanged;
    // Event raised when session is about to expire. Parameter is seconds remaining.
    event Action<int>? SessionExpiring;
    // Event raised when session is renewed (keep alive called successfully).
    event Action? SessionRenewed;
    // Event raised when session is auto-renewed due to activity (silent renewal at timeout).
    event Action? SessionAutoRenewed;
    // Event raised when user activity is detected (throttled) from JS.
    event Action? UserActivityDetected;

    Task InitializeAsync();
    Task<LoginResult> LoginAsync(string login, string password);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    CurrentUser? CurrentUser { get; }
    // Keep the session alive (resets inactivity timers). Implementations may also refresh tokens.
    Task KeepAliveAsync();

    // Called by JS to notify of user activity (throttled)
    Task OnUserActivityAsync();
}