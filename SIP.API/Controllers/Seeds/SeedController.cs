using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Enums;
using SIP.API.Infrastructure.Database;

namespace SIP.API.Controllers.Seeds;

[Route("sip_api/[controller]")]
[ApiController]
public class SeedController(ApplicationContext context) : ControllerBase
{
    private readonly ApplicationContext _context = context;

    [HttpPost("import")]
    public async Task<IActionResult> ImportSeed()
    {
        var json = await System.IO.File.ReadAllTextAsync("seed.json", Encoding.UTF8);

        var seedData = JsonSerializer.Deserialize<SeedData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // 1. Crie as secretarias (setores) sem Id
        var sectorEntities = seedData?.Sectors?.Select(s => new Sector
        {
            Name = s.Name!,
            Acronym = s.Acronym!,
            Phone = s.Phone!,
            CreatedAt = DateTime.Parse(s.CreatedAt!),
            UpdatedAt = s.UpdatedAt != null ? DateTime.Parse(s.UpdatedAt) : null
        }).ToList() ?? [];

        _context.Sectors.AddRange(sectorEntities);
        await _context.SaveChangesAsync();

        // 2. Recupere os Ids dos setores já persistidos
        var sectorIds = _context.Sectors.Select(s => s.Id).ToList();

        // 3. Embaralhe os Ids para sortear
        var random = new Random();
        var shuffledSectorIds = sectorIds.OrderBy(x => random.Next()).ToList();

        // 4. Crie os usuários, preenchendo SectorId dinamicamente
        int sectorIndex = 0;
        var userEntities = seedData?.Users?.Select(u =>
        {
            var user = new User
            {
                FullName = u.FullName!,
                Login = u.Login!,
                Masp = u.Masp,
                Email = u.Email!,
                PasswordHash = u.PasswordHash!,
                CreatedAt = DateTime.Parse(u.CreatedAt!),
                LastLoginAt = u.LastLoginAt != null ? DateTime.Parse(u.LastLoginAt) : null,
                UpdatedAt = u.UpdatedAt != null ? DateTime.Parse(u.UpdatedAt) : null,
                Role = Enum.Parse<UserRole>(u.Role!),
                IsActive = u.IsActive,
                SectorId = shuffledSectorIds[sectorIndex % shuffledSectorIds.Count]
            };
            sectorIndex++;
            return user;
        }).ToList() ?? [];

        _context.Users.AddRange(userEntities);
        await _context.SaveChangesAsync();

        return Ok("Seed importado com sucesso!");
    }
}

public class SeedData
{
    public List<SectorSeed>? Sectors { get; set; }
    public List<UserSeed>? Users { get; set; }
}

public class SectorSeed
{
    public string? Name { get; set; }
    public string? Acronym { get; set; }
    public string? Phone { get; set; }
    public string? CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }
}

public class UserSeed
{
    public string? FullName { get; set; }
    public string? Login { get; set; }
    public int Masp { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? CreatedAt { get; set; }
    public string? LastLoginAt { get; set; }
    public string? UpdatedAt { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; }
}