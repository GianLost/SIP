using Microsoft.EntityFrameworkCore;
using Serilog;
using SIP.API.Domain.Interfaces.Hashes.Passwords;
using SIP.API.Domain.Interfaces.Protocols;
using SIP.API.Domain.Interfaces.Sectors;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Domain.Interfaces.Users.Configurations;
using SIP.API.Domain.Services.Hashes.Passwords;
using SIP.API.Domain.Services.Protocols;
using SIP.API.Domain.Services.Sectors;
using SIP.API.Domain.Services.Users;
using SIP.API.Domain.Services.Users.Configurations;
using SIP.API.Infrastructure.Caching;
using SIP.API.Infrastructure.Database;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy => policy
            .WithOrigins("https://localhost:7236") // address of my Blazor WebAssembly
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddMemoryCache();

// Configura��o global do Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()

    // Log geral (tudo)
    .WriteTo.File("logs/general-.txt", rollingInterval: RollingInterval.Day)

    // Logs de Controllers
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("SourceContext") &&
                                     e.Properties["SourceContext"].ToString().Contains("Controllers"))
        .WriteTo.File("logs/controllers-.txt", rollingInterval: RollingInterval.Day)
    )

    // Logs de Entities
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("SourceContext") &&
                                     e.Properties["SourceContext"].ToString().Contains("Entities"))
        .WriteTo.File("logs/entities-.txt", rollingInterval: RollingInterval.Day)
    )

    // Logs de DTOs
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("SourceContext") &&
                                     e.Properties["SourceContext"].ToString().Contains("DTO"))
        .WriteTo.File("logs/dtos-.txt", rollingInterval: RollingInterval.Day)
    )

    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddSingleton<EntityCacheManager>();

builder.Services.AddScoped<ICryptPassword, CryptPassword>();


builder.Services.AddScoped<IUser, UserService>();
builder.Services.AddScoped<IUserConfiguration, UserConfigurationService>();

builder.Services.AddScoped<ISector, SectorService>();
builder.Services.AddScoped<IProtocol, ProtocolService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SIP API",
        Version = "v1",
        Description = "Integrated management API for 'SIP - INTERNAL PROTOCOL SYSTEM'."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    s.IncludeXmlComments(xmlPath);
});

builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql")
    ));
});

WebApplication app = builder.Build();

app.UseCors("AllowBlazorClient");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
