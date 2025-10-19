using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Enums;
using SIP.API.Domain.Interfaces.Hashes.Passwords;
using SIP.API.Domain.Interfaces.Protocols;
using SIP.API.Infrastructure.Database;

namespace SIP.API.Controllers.Seeds;

/// <summary>
/// Controlador responsável por realizar a importação de dados iniciais (seeds) 
/// no banco de dados do sistema SIP.  
/// Essa funcionalidade é destinada a ambientes de desenvolvimento e homologação, 
/// permitindo popular rapidamente as tabelas de setores, usuários e protocolos
/// a partir de um arquivo <c>seed.json</c>.
/// </summary>
/// <remarks>
/// O arquivo <c>seed.json</c> deve estar localizado na raiz do projeto e seguir a estrutura
/// compatível com as classes <see cref="SeedData"/>, <see cref="SectorSeed"/>, 
/// <see cref="UserSeed"/> e <see cref="ProtocolSeed"/>.  
/// 
/// O método <see cref="ImportSeed"/>:
/// - Cria os setores (secretarias);
/// - Associa usuários a setores de forma aleatória;
/// - Gera registros de protocolos fictícios.
/// </remarks>
[Route("sip_api/seeds")]
[ApiController]
public class SeedController(ApplicationContext context, ICryptPassword crypt, IProtocol protocolService) : ControllerBase
{
    private readonly ApplicationContext _context = context;

    private readonly ICryptPassword _crypt = crypt;
    private readonly IProtocol _protocolService = protocolService;

    // Add this static readonly field to cache the JsonSerializerOptions instance
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Importa dados de teste (seeds) a partir de um arquivo <c>seed.json</c> localizado na raiz do projeto.
    /// </summary>
    /// <remarks>
    /// Este método realiza as seguintes operações:
    /// <list type="number">
    /// <item>Cria registros de setores (secretarias);</item>
    /// <item>Cria registros de usuários, vinculando-os a setores de forma aleatória;</item>
    /// <item>Gera registros de protocolos com base nos usuários e setores criados.</item>
    /// </list>
    /// 
    /// O arquivo <c>seed.json</c> deve possuir estrutura compatível com o modelo <see cref="SeedData"/>.
    /// 
    /// ⚠️ Este endpoint é voltado exclusivamente para **uso interno** em ambientes de teste.
    /// </remarks>
    /// <response code="200">Seed importado com sucesso.</response>
    /// <response code="400">Falha ao ler ou deserializar o arquivo de seed.</response>
    /// <response code="500">Erro interno ao importar dados para o banco.</response>
    [HttpPost("import")]
    public async Task<IActionResult> ImportSeed()
    {
        // Lê o conteúdo do arquivo seed.json utilizado para popular o banco de dados
        var json = await System.IO.File.ReadAllTextAsync("Controllers/Seeds/seed.json", Encoding.UTF8);

        // Desserializa o conteúdo JSON para o modelo SeedData usando a instância cacheada
        var seedData = JsonSerializer.Deserialize<SeedData>(json, _jsonOptions);

        // 1. Crie as secretarias (setores) sem Id
        var sectorEntities = seedData?.Sectors?.Select(s => new Sector
        {
            Name = s.Name!,
            Acronym = s.Acronym!,
            Phone = s.Phone!,
            CreatedAt = DateTime.Parse(s.CreatedAt!),
            UpdatedAt = s.UpdatedAt != null ? DateTime.Parse(s.UpdatedAt) : null
        }).ToList() ?? [];

        // 2. Adiciona todos os setores ao contexto e os salva
        _context.Sectors.AddRange(sectorEntities);
        await _context.SaveChangesAsync();

        // 3. Recupera os Ids dos setores já persistidos
        var sectorIds = _context.Sectors.Select(s => s.Id).ToList();

        // 4. Embaralha os Ids para sortear
        var random = new Random();
        var shuffledSectorIds = sectorIds.OrderBy(x => random.Next()).ToList();

        // 5. Cria os usuários, preenchendo SectorId dinamicamente
        int sectorIndex = 0;
        var userEntities = seedData?.Users?.Select(u =>
        {
            var user = new User
            {
                Name = u.FullName!,
                Login = u.Login!,
                Masp = u.Masp,
                Email = u.Email!,
                PasswordHash = _crypt.Hash(u.PasswordHash!),
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

        // 6. Adiciona todos os usuários ao contexto e os salva
        _context.Users.AddRange(userEntities);
        await _context.SaveChangesAsync();

        // 7. Obtém os IDs dos usuários
        var userIds = _context.Users.Select(u => u.Id).ToList();

        // 8. Busca o último número de protocolo (UMA ÚNICA VEZ) antes do loop
        var lastProtocolNumber = await _protocolService.GetLastProtocolNumberAsync();

        // 9. Obtém o próximo número sequencial de protocolo
        int nextSequence = _protocolService.GetNextSequence(lastProtocolNumber);

        // 10. Cria os protocolos em memória, incrementando o número sequencial localmente
        var protocolEntities = new List<Protocol>();
        foreach (var p in seedData?.Protocols!)
        {
            string number = _protocolService.FormatProtocolNumber(nextSequence);
            int formatNuumber = Convert.ToInt32(number);

            protocolEntities.Add(new Protocol
            {
                Number = formatNuumber,
                Subject = p.Subject,
                Description = p.Description,
                Status = Enum.Parse<ProtocolStatus>(p.Status!),
                IsArchived = p.IsArchived,
                CreatedAt = DateTime.Parse(p.CreatedAt),
                UpdatedAt = p.UpdatedAt != null ? DateTime.Parse(p.UpdatedAt) : null,
                CreatedById = userIds[random.Next(userIds.Count)],
                OriginSectorId = sectorIds[random.Next(sectorIds.Count)],
                DestinationUserId = userIds.Count > 0 ? userIds[random.Next(userIds.Count)] : Guid.Empty,
                DestinationSectorId = sectorIds[random.Next(sectorIds.Count)]
            });
            nextSequence++; // Incrementa o número para o próximo protocolo
        }

        // 11. Adiciona todos os protocolos ao contexto e os salva
        _context.Protocols.AddRange(protocolEntities);
        await _context.SaveChangesAsync();

        // Retorna uma mensagem de sucesso
        return Ok("Seed importado com sucesso!");
    }
}

/// <summary>
/// Modelo raiz da estrutura de dados utilizada no arquivo de seed.
/// Contém coleções de setores, usuários e protocolos.
/// </summary>
public class SeedData
{
    public List<SectorSeed>? Sectors { get; set; }
    public List<UserSeed>? Users { get; set; }
    public List<ProtocolSeed>? Protocols { get; set; }
}

/// <summary>
/// Representa a estrutura de um setor (secretaria) no arquivo de seed.
/// </summary>
public class SectorSeed
{
    public string? Name { get; set; }
    public string? Acronym { get; set; }
    public string? Phone { get; set; }
    public string? CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }
}

/// <summary>
/// Representa a estrutura de um usuário no arquivo de seed.
/// </summary>
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

/// <summary>
/// Representa a estrutura de um protocolo no arquivo de seed.
/// </summary>
public class ProtocolSeed
{
    public string Subject { get; set; } = string.Empty;
    public string Description { get;} = 
        "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
    public string? Status { get; set; }
    public bool IsArchived { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
}