using System.Net.Mime;
using EstudoApi.Contracts;
using EstudoApi.Infrastructure.Auth;
using EstudoApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EstudoApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces(MediaTypeNames.Application.Json)]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwt;

    public AuthController(UserManager<AppUser> userManager, IJwtTokenService jwt)
    {
        _userManager = userManager;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user is null) return Unauthorized(new { error = "Credenciais inválidas." });

        var ok = await _userManager.CheckPasswordAsync(user, req.Senha);
        if (!ok) return Unauthorized(new { error = "Credenciais inválidas." });

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expires) = _jwt.CreateToken(user, roles);

        return Ok(new AuthResponse(token, expires));
    }
}
