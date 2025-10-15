using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SIP.API.Domain.Helpers.ApplicationHelper;
using SIP.API.Domain.Helpers.Messages.HomeMessages.Error;
using SIP.API.Domain.Helpers.Messages.HomeMessages.info;
using SIP.API.Domain.Helpers.Messages.HomeMessages.Success;
using SIP.API.Domain.ModelView.Health;
using SIP.API.Domain.ModelView.Home;

namespace SIP.API.Controllers;

/// <summary>
/// Controlador responsável por fornecer informações de status e disponibilidade da API.
/// </summary>
/// <remarks>
/// Este controlador disponibiliza endpoints para verificação rápida (*liveness*), 
/// avaliação de prontidão (*readiness*) e consulta de informações gerais da API.
/// </remarks>
[Route("/")]
[ApiController]
public class HomeController(IConfiguration configuration) : ControllerBase
{
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Verifica se a API está em execução e capaz de responder a requisições HTTP.
    /// </summary>
    /// <remarks>
    /// Este endpoint é classificado como um *liveness probe*.
    /// Ele confirma se o processo da API está ativo, mas não realiza checagens
    /// em dependências externas, como banco de dados ou cache.
    /// É ideal para uso em balanceadores de carga e orquestradores de contêineres.
    /// </remarks>
    /// <response code="200">A API está em execução e acessível.</response>
    [HttpHead]
    public IActionResult HealthCheck()
    {
        Response.Headers.Append("X-API-Status", "Online");
        return Ok();
    }

    /// <summary>
    /// Avalia a prontidão operacional da API e de suas dependências externas.
    /// </summary>
    /// <remarks>
    /// Este endpoint é classificado como um *readiness probe*.
    /// Ele verifica a integridade da API e suas dependências críticas,
    /// como a conexão com o banco de dados MySQL.
    /// É utilizado por sistemas de monitoramento e automação de implantação
    /// para determinar se o serviço está pronto para receber tráfego.
    /// </remarks>
    /// <returns>
    /// Um objeto <see cref="HealthStatusResponse"/> contendo o status geral da API,
    /// as dependências avaliadas e o horário da verificação.
    /// </returns>
    /// <response code="200">A API e todas as dependências estão operacionais.</response>
    /// <response code="503">A API está ativa, mas uma ou mais dependências estão inacessíveis.</response>
    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealthStatus()
    {
        var checks = await CheckDependenciesAsync();

        bool allHealthy = checks.All(c => c.Value);

        var response = new HealthStatusResponse
        {
            Status = allHealthy ? "Healthy" : "Degraded",
            Dependencies = checks.ToDictionary(c => c.Key, c => c.Value ? "Healthy" : "Degraded"),
            Message = allHealthy
                ? HomeInfoMessages.AllServicesOperational
                : HomeInfoMessages.DependenciesUnavailable,
            Timestamp = DateTime.UtcNow
        };

        Response.Headers.CacheControl = "no-store";

        return allHealthy
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    /// <summary>
    /// Retorna informações gerais sobre a API, incluindo nome, versão e link da documentação Swagger.
    /// </summary>
    /// <remarks>
    /// Este endpoint fornece uma visão geral da API, permitindo validar rapidamente
    /// o acesso ao serviço e consultar o link da documentação interativa.
    /// Também executa uma verificação simples da disponibilidade do banco de dados.
    /// </remarks>
    /// <returns>
    /// Um objeto <see cref="Home"/> contendo o nome da API, versão, status atual
    /// e a URL da documentação.
    /// </returns>
    /// <response code="200">As informações da API foram retornadas com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(Home), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHome()
    {
        HttpRequest request = HttpContext.Request;
        string baseUrl = $"{request.Scheme}://{request.Host.Value}";

        bool isDatabaseOnline = await CheckDatabaseConnectionAsync();
        string status = isDatabaseOnline ? "Healthy" : "Degraded";

        string message = isDatabaseOnline
            ? HomeSuccessMessages.APIConnectionSuccess
            : HomeErrorMessages.DatabaseConnectionFail;

        Home response = new(
            Status: status,
            DocumentationUrl: $"{baseUrl}/swagger"
        )
        {
            Message = message
        };

        return Ok(response);
    }

    /// <summary>
    /// Executa verificações de integridade das dependências configuradas na aplicação.
    /// </summary>
    /// <remarks>
    /// Atualmente, apenas o banco de dados MySQL é avaliado.
    /// Este método pode ser expandido para incluir novas dependências,
    /// como cache distribuído, filas de mensageria ou APIs externas.
    /// </remarks>
    /// <returns>
    /// Um dicionário contendo o nome de cada dependência e o resultado de sua verificação.
    /// </returns>
    private async Task<Dictionary<string, bool>> CheckDependenciesAsync()
    {
        var results = new Dictionary<string, bool>
        {
            ["MySQL"] = await CheckDatabaseConnectionAsync()
        };

        return results;
    }

    /// <summary>
    /// Verifica a disponibilidade de conexão com o banco de dados MySQL configurado.
    /// </summary>
    /// <remarks>
    /// Tenta abrir uma conexão utilizando a string de conexão definida em <c>appsettings.Development.json</c>.
    /// Retorna <c>true</c> se a conexão for estabelecida com sucesso; caso contrário, retorna <c>false</c>.
    /// </remarks>
    /// <returns>
    /// Um valor booleano indicando se o banco de dados está acessível.
    /// </returns>
    private async Task<bool> CheckDatabaseConnectionAsync()
    {
        try
        {
            string? connectionString = _configuration.GetConnectionString("MySql");

            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}