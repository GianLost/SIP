using SIP.UI.Models.Auth;

namespace SIP.UI.Domain.Interfaces.Auth;

public interface IAuthService
{
    event Action? AuthenticationStateChanged;

    Task InitializeAsync();
    Task<bool> LoginAsync(string login, string password);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    CurrentUser? CurrentUser { get; }
}
