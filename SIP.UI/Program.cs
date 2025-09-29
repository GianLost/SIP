using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SIP.UI;
using SIP.UI.Domain.Services.Protocols;
using SIP.UI.Domain.Services.Sectors;
using SIP.UI.Domain.Services.Users;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7083/") });

builder.Services.AddScoped<SectorService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProtocolService>();

await builder.Build().RunAsync();