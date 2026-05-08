using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using SIP.UI;
using SIP.UI.Domain.Services.Auth;
using SIP.UI.Domain.Services.Protocols;
using SIP.UI.Domain.Services.Sectors;
using SIP.UI.Domain.Services.Users;
using SIP.UI.Domain.Interfaces.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices(config =>
{
    // Ajusta o delay dos snackbars para uma experiência mais fluida
    config.SnackbarConfiguration.ShowTransitionDuration = 300; // ms para aparecer
    config.SnackbarConfiguration.VisibleStateDuration = 2500; // ms visível
    config.SnackbarConfiguration.HideTransitionDuration = 300; // ms para desaparecer
});
builder.Services.AddAuthorizationCore();

/* URL de comunicação com a API */
builder.Services.AddScoped(sp => 
    new HttpClient 
    { 
        BaseAddress = new Uri("http://localhost:5126/") 
    }
);

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

builder.Services.AddScoped<SectorService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProtocolService>();

await builder.Build().RunAsync();