using Microsoft.AspNetCore.Mvc;
using SIP.API.Domain.ModelView.Home;

namespace SIP.API.Controllers;

[Route("/")]
[ApiController]
public class HomeController : ControllerBase
{
    /// <summary>
    /// Returns basic information about the API or its status.
    /// </summary>
    /// <remarks>
    /// This endpoint can be used to verify that the API is running and accessible.
    /// </remarks>
    /// <returns>
    /// Returns a <see cref="Home"/> object with basic API information.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(Home), StatusCodes.Status200OK)]
    public IActionResult GetHome()
    {
        HttpRequest request = HttpContext.Request;
        string scheme = request.Scheme;
        string host = request.Host.Value;

        string baseUrl = $"{scheme}://{host}";

        Home response = new(documentationUrl: $"{baseUrl}/swagger");

        return Ok(response);
    }
}