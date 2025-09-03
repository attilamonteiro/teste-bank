using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EstudoApi.Controllers;

[ApiController]
[Route("api/auth")]
[Produces(MediaTypeNames.Application.Json)]

public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("conta/cadastrar")]
    public async Task<IActionResult> CadastrarContaCorrente([FromBody] EstudoApi.Domain.CQRS.Commands.Account.CreateAccountCommand command)
    {
        // Validação simples de CPF (pode ser substituída por uma validação mais robusta)
        if (string.IsNullOrWhiteSpace(command.Cpf) || command.Cpf.Length != 11 || !command.Cpf.All(char.IsDigit))
        {
            return BadRequest(new { mensagem = "CPF inválido.", tipo = "INVALID_DOCUMENT" });
        }

        var result = await _mediator.Send(command);
        if (result.Success && result.NumeroConta.HasValue)
            return Created(string.Empty, new { numeroConta = result.NumeroConta });
        return BadRequest(new { mensagem = result.Error ?? "Erro ao cadastrar conta.", tipo = result.Tipo ?? "UNKNOWN_ERROR" });
    }

    [HttpPost("conta/login")]
    public async Task<IActionResult> LoginContaCorrente([FromBody] EstudoApi.Domain.CQRS.Commands.Account.LoginAccountCommand command)
    {
        if ((command.NumeroConta == null || command.NumeroConta <= 0) && string.IsNullOrWhiteSpace(command.Cpf))
        {
            return Unauthorized(new { mensagem = "Informe o número da conta ou CPF.", tipo = "USER_UNAUTHORIZED" });
        }
        if (string.IsNullOrWhiteSpace(command.Senha))
        {
            return Unauthorized(new { mensagem = "Senha obrigatória.", tipo = "USER_UNAUTHORIZED" });
        }
        var result = await _mediator.Send(command);
        if (result.Success && !string.IsNullOrEmpty(result.Token))
            return Ok(new { token = result.Token });
        return Unauthorized(new { mensagem = result.Error ?? "Credenciais inválidas.", tipo = result.Tipo ?? "USER_UNAUTHORIZED" });
    }

}
