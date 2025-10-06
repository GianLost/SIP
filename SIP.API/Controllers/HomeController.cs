using Microsoft.AspNetCore.Mvc;
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
public class HomeController : ControllerBase
{
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
    public IActionResult GetHome()
    {
        HttpRequest request = HttpContext.Request;
        string scheme = request.Scheme;
        string host = request.Host.Value;

        string baseUrl = $"{scheme}://{host}";

        Home response = new(DocumentationUrl: $"{baseUrl}/swagger");

        return Ok(response);
    }
}