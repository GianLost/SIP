using SIP.UI.Models.Auth;

namespace SIP.UI.Domain.Interfaces.Auth;

public interface IAuthService
{
    event Action? AuthenticationStateChanged;
    // Event raised when session is about to expire. Parameter is seconds remaining.
    event Action<int>? SessionExpiring;
    // Event raised when session is renewed (keep alive called successfully).
    event Action? SessionRenewed;
    // Event raised when session is auto-renewed due to activity (silent renewal at timeout).
    event Action? SessionAutoRenewed;

    Task InitializeAsync();
    Task<bool> LoginAsync(string login, string password);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    CurrentUser? CurrentUser { get; }
    // Keep the session alive (resets inactivity timers). Implementations may also refresh tokens.
    Task KeepAliveAsync();
}
