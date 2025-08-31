using System.Net.Mime;
using EstudoApi.Contracts;
using EstudoApi.Infrastructure.Auth;
using EstudoApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EstudoApi.Controllers;

[ApiController]
[Route("api/auth")]
[Produces(MediaTypeNames.Application.Json)]

public class AuthController : ControllerBase
{
    private readonly Infrastructure.CQRS.Handlers.LoginUserCommandHandler _loginHandler;
    private readonly UserManager<AppUser> _userManager;

    public AuthController(Infrastructure.CQRS.Handlers.LoginUserCommandHandler loginHandler, UserManager<AppUser> userManager)
    {
        _loginHandler = loginHandler;
        _userManager = userManager;
    }

    [HttpPost("login")]
    [Swashbuckle.AspNetCore.Filters.SwaggerRequestExample(typeof(LoginRequest), typeof(EstudoApi.SwaggerExamples.LoginRequestExample))]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var command = new Domain.CQRS.Commands.LoginUserCommand { Email = req.Email, Password = req.Senha };
        var result = await _loginHandler.Handle(command);
        if (result == null)
            return Unauthorized(new { error = "Credenciais inválidas." });

        var (token, expiresAt) = result.Value;
        return Ok(new AuthResponse(token, expiresAt));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Senha) ||
            string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Cpf))
            return BadRequest(new { error = "Campos obrigatórios ausentes." });

        var existsByEmail = await _userManager.FindByEmailAsync(req.Email);
        if (existsByEmail != null) return Conflict(new { error = "E-mail já cadastrado." });

        var existsByCpf = _userManager.Users.FirstOrDefault(u => u.Cpf == req.Cpf);
        if (existsByCpf != null) return Conflict(new { error = "CPF já cadastrado." });

        var user = new AppUser
        {
            UserName = req.Email,
            Email = req.Email,
            Nome = req.Nome,
            Cpf = req.Cpf
        };

        var result = await _userManager.CreateAsync(user, req.Senha);
        if (!result.Succeeded)
            return BadRequest(new { error = "Falha ao criar usuário.", details = result.Errors });

        return CreatedAtAction(nameof(Register), new { id = user.Id }, new { id = user.Id });
    }
}
