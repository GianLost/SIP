using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SIP.API.Domain.DTOs.Users.Auth;
using SIP.API.Domain.DTOs.Users.Responses;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Helpers.KeysHelper;
using SIP.API.Domain.Interfaces.Users;

namespace SIP.API.Controllers.Auth;

[Route("sip_api/auth")]
[ApiController]
public class AuthController(IAuthenticationService authenticationService, IOptions<JwtSettings> jwtSettings, ILogger<AuthController> logger) : ControllerBase
{
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Login e senha são obrigatórios." });

        User? user = await _authenticationService.AuthenticateAsync(request.Login, request.Password);
        if (user == null)
            return Unauthorized(new { error = "Login ou senha inválidos." });

        string token = BuildToken(user);

        var response = new AuthResponseDTO
        {
            AccessToken = token,
            ExpiresIn = _jwtSettings.AccessTokenExpirationMinutes * 60,
            Role = user.Role.ToString(),
            User = new UserResponseDTO
            {
                Id = user.Id,
                Status = user.IsActive,
                Masp = user.Masp,
                Name = user.Name,
                Login = user.Login,
                Email = user.Email,
                SectorId = user.SectorId
            }
        };

        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? name = User.FindFirstValue(ClaimTypes.Name);
        string? email = User.FindFirstValue(ClaimTypes.Email);
        string? role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { error = "Usuário não autenticado." });

        return Ok(new
        {
            Id = userId,
            Name = name,
            Email = email,
            Role = role
        });
    }

    private string BuildToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Login),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
