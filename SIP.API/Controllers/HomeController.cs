using Microsoft.AspNetCore.Authorization;
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
        var request = HttpContext.Request;
        var scheme = request.Scheme;
        var host = request.Host.Value;

        var baseUrl = $"{scheme}://{host}";

        var response = new Home(documentationUrl: $"{baseUrl}/swagger");

        return Ok(response);
    }
}