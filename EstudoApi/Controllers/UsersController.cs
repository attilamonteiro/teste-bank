using System.Net.Mime;
using EstudoApi.Contracts;
using EstudoApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace EstudoApi.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces(MediaTypeNames.Application.Json)]
public class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;

    public UsersController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Cria um novo usuário.
    /// </summary>
    /// <remarks>
    /// Exemplo de request:
    ///
    ///     {
    ///         "nome": "João da Silva",
    ///         "email": "joao@email.com",
    ///         "senha": "minhasenha123",
    ///         "cpf": "12345678901"
    ///     }
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 409)]
    [Swashbuckle.AspNetCore.Filters.SwaggerRequestExample(typeof(RegisterUserRequest), typeof(EstudoApi.SwaggerExamples.RegisterUserRequestExample))]
    public async Task<IActionResult> Create([FromBody] RegisterUserRequest req)
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

        return CreatedAtAction(nameof(Create), new { id = user.Id }, new { id = user.Id });
    }
}
