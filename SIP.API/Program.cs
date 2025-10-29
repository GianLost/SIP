using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Reflection;
using Serilog;
using SIP.API.Infrastructure.Caching;
using SIP.API.Infrastructure.Database;
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
using SIP.API.Domain.Helpers.ApplicationHelper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy => policy
            .WithOrigins("https://localhost:7236", "https://localhost:7083") // Blazor + Swagger (API)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddMemoryCache();

var culture = new CultureInfo("pt-BR");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "SIP_API") // nome fixo do sistema

    // =============================
    // Log para o terminal
    // =============================
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )

    // =============================
    // Log geral (tudo)
    // =============================
    .WriteTo.File(
      path: "Logs/General/general-.txt", 
      rollingInterval: RollingInterval.Day,
      outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
      formatProvider: culture)

    // =============================
    // Controllers
    // =============================
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Properties.ContainsKey("SourceContext") && e.Properties["SourceContext"].ToString().Contains("Controllers")).WriteTo.File(
      path: "Logs/Controllers/controllers-.txt", 
      rollingInterval: RollingInterval.Day, 
      outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", 
      formatProvider: culture))

    // =============================
    // Entities
    // =============================
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Properties.ContainsKey("SourceContext") && e.Properties["SourceContext"].ToString().Contains("Entities")).WriteTo.File(
      path: "Logs/Entities/entities-.txt", 
      rollingInterval: RollingInterval.Day, 
      outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", 
      formatProvider: culture))

    // =============================
    // DTOs
    // =============================
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Properties.ContainsKey("SourceContext") && e.Properties["SourceContext"].ToString().Contains("DTO")).WriteTo.File(
      path: "Logs/DTOs/dtos-.txt", 
      rollingInterval: RollingInterval.Day,
      outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
      formatProvider: culture)

    // =============================
    // Users
    // =============================
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Properties.ContainsKey("SourceContext") && e.Properties["SourceContext"].ToString().Contains("User")).WriteTo.File(
      path: "Logs/Users/users-.txt", 
      rollingInterval: RollingInterval.Day,
      outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
      formatProvider: culture))


    // =============================
    // Sectors
    // =============================
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Properties.ContainsKey("SourceContext") && e.Properties["SourceContext"].ToString().Contains("Sector")).WriteTo.File(
      path: "Logs/Sectors/sectors-.txt", 
      rollingInterval: RollingInterval.Day,
      outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
      formatProvider: culture))

    // =============================
    // Protocols
    // =============================
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Properties.ContainsKey("SourceContext") && e.Properties["SourceContext"].ToString().Contains("Protocol")).WriteTo.File(
      path: "Logs/Protocols/protocols-.txt", 
      rollingInterval: RollingInterval.Day,
      outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
      formatProvider: culture))).CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddSingleton<EntityCacheManager>();

builder.Services.AddScoped<IUser, UserService>();
builder.Services.AddScoped<ISector, SectorService>();
builder.Services.AddScoped<IProtocol, ProtocolService>();

builder.Services.AddScoped<ICrypt, CryptPassword>();
builder.Services.AddScoped<IUserConfiguration, UserConfigurationService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configuração do Swagger com informações da API e inclusão dos comentários XML
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SIP API",
        Version = ApplicationInfo.Version,
        Description = "Integrated management API for 'SIP - INTERNAL PROTOCOL SYSTEM'."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    s.IncludeXmlComments(xmlPath);
});

// Configuração do DbContext com MySQL
builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")
    ));
});

WebApplication app = builder.Build();

// Habilita o CORS para permitir requisições do cliente Blazor
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
