using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Enums;
using SIP.API.Domain.Interfaces.Protocols;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Domain.Services.Protocols;
using SIP.API.Infrastructure.Database;

namespace SIP.API.Controllers.Seeds;

[Route("sip_api/[controller]")]
[ApiController]
public class SeedController(ApplicationContext context, IProtocol protocolService) : ControllerBase
{
    private readonly ApplicationContext _context = context;
    private readonly IProtocol _protocolService = protocolService;

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

        //// Obter IDs de usuários
        var userIds = _context.Users.Select(u => u.Id).ToList();

        // 3. Busque o último número de protocolo UMA ÚNICA VEZ antes do loop
        var lastProtocolNumber = await _protocolService.GetLastProtocolNumberAsync();
        int nextSequence = _protocolService.GetNextSequence(lastProtocolNumber);

        // 4. Crie os protocolos em memória, incrementando o número sequencial localmente
        var protocolEntities = new List<Protocol>();
        foreach (var p in seedData?.Protocols!)
        {
            protocolEntities.Add(new Protocol
            {
                Number = _protocolService.FormatProtocolNumber(nextSequence),
                Subject = p.Subject,
                Description = p.Description,
                Status = Enum.Parse<ProtocolStatus>(p.Status!),
                IsArchived = p.IsArchived,
                CreatedAt = DateTime.Parse(p.CreatedAt),
                UpdatedAt = p.UpdatedAt != null ? DateTime.Parse(p.UpdatedAt) : null,
                CreatedById = userIds[random.Next(userIds.Count)],
                OriginSectorId = sectorIds[random.Next(sectorIds.Count)],
                DestinationUserId = userIds.Count > 0 ? userIds[random.Next(userIds.Count)] : null,
                DestinationSectorId = sectorIds[random.Next(sectorIds.Count)]
            });
            nextSequence++; // Incrementa o número para o próximo protocolo
        }

        // 5. Adicione todos os protocolos de uma vez e salve as mudanças
        _context.Protocols.AddRange(protocolEntities);
        await _context.SaveChangesAsync();

        return Ok("Seed importado com sucesso!");
    }
}

public class SeedData
{
    public List<SectorSeed>? Sectors { get; set; }
    public List<UserSeed>? Users { get; set; }
    public List<ProtocolSeed>? Protocols { get; set; }
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

public class ProtocolSeed
{
    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get;} = 
        "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
    public string? Status { get; set; }
    public bool IsArchived { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
}