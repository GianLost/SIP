using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SIP.UI.Domain.Interfaces.Users.Auth;

namespace SIP.UI.Domain.Services.Users.Auth;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService _authService;

    public JwtAuthenticationStateProvider(IAuthService authService)
    {
        _authService = authService;
        _authService.AuthenticationStateChanged += NotifyUserAuthentication;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await _authService.InitializeAsync();

        string? token = await _authService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        IEnumerable<Claim> claims = JwtParser.ParseClaimsFromJwt(token);

        ClaimsIdentity identity = new(claims, "jwt");

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyUserAuthentication()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}