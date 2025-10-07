using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SIP.API.Domain.Helpers.Messages.HomeMessages.Error;
using SIP.API.Domain.Helpers.Messages.HomeMessages.Success;
using SIP.API.Domain.ModelView.Home;

namespace SIP.API.Controllers;

/// <summary>
/// Controlador responsável por fornecer informações básicas sobre o estado e o funcionamento da API.
/// </summary>
/// <remarks>
/// Este controlador disponibiliza um endpoint que pode ser utilizado para verificar se a API está
/// em execução e acessível, além de retornar o link direto para a documentação Swagger.
/// </remarks>
[Route("/")]
[ApiController]
public class HomeController(IConfiguration configuration) : ControllerBase
{
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Endpoint rápido de verificação de disponibilidade da API.
    /// </summary>
    /// <response code="200">A API está em execução e acessível.</response>
    [HttpHead]
    public IActionResult HealthCheck()
    {
        Response.Headers.Append("X-API-Status", "Online");
        return Ok();
    }

    /// <summary>
    /// Retorna informações básicas sobre a API, incluindo o link para a documentação Swagger.
    /// </summary>
    /// <remarks>
    /// Este endpoint é útil para validar se a API está operacional e para facilitar o acesso à
    /// interface de documentação e testes interativos disponibilizada pelo Swagger.
    /// </remarks>
    /// <returns>
    /// Um objeto <see cref="Home"/> contendo informações básicas e o link da documentação da API.
    /// </returns>
    /// <response code="200">Informações da API retornadas com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(Home), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHome()
    {
        HttpRequest request = HttpContext.Request;
        string baseUrl = $"{request.Scheme}://{request.Host.Value}";

        bool isDatabaseOnline = await CheckDatabaseConnectionAsync();
        string status = isDatabaseOnline ? "Online" : "Degraded";

        string message = isDatabaseOnline
            ? HomeSuccessMessages.APIConnectionSuccess
            : HomeErrorMessages.DatabaseConnectionFail;

        Home response = new(
            ApiName: "SIP_API",
            Version: "1.0",
            Status: status,
            DocumentationUrl: $"{baseUrl}/swagger"
        )
        {
            Message = message
        };

        return Ok(response);
    }

    private async Task<bool> CheckDatabaseConnectionAsync()
    {
        try
        {
            string? connectionString = _configuration.GetConnectionString("MySql");

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