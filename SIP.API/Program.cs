using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.Interfaces.Sectors;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Domain.Interfaces.Users.Configurations;
using SIP.API.Domain.Services.Sectors;
using SIP.API.Domain.Services.Users;
using SIP.API.Domain.Services.Users.Configurations;
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

builder.Services.AddScoped<IUser, UserService>();
builder.Services.AddScoped<IUserConfiguration, UserConfigurationService>();

builder.Services.AddScoped<ISector, SectorService>();

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

var app = builder.Build();

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
