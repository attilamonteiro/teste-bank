using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using EstudoApi.Domain.Interfaces.Repositories;
using EstudoApi.Infrastructure.Identity;
using EstudoApi.Infrastructure.Auth;

namespace EstudoApi.Controllers;

[ApiController]
[Route("api/auth")]
[Produces(MediaTypeNames.Application.Json)]

public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IContaBancariaRepository _contaBancariaRepository;

    public AuthController(IMediator mediator, IJwtTokenService jwtTokenService, IContaBancariaRepository contaBancariaRepository)
    {
        _mediator = mediator;
        _jwtTokenService = jwtTokenService;
        _contaBancariaRepository = contaBancariaRepository;
    }

    [HttpPost("conta/cadastrar")]
    public async Task<IActionResult> CadastrarContaCorrente([FromBody] CadastroRequest request)
    {
        // Validação simples de CPF
        if (string.IsNullOrWhiteSpace(request.Cpf) || request.Cpf.Length != 11 || !request.Cpf.All(char.IsDigit))
        {
            return BadRequest(new { mensagem = "CPF inválido.", tipo = "INVALID_DOCUMENT" });
        }

        // Usar novo comando unificado
        var command = new EstudoApi.Domain.CQRS.Commands.ContaBancaria.CreateContaBancariaCommand
        {
            Cpf = request.Cpf,
            Senha = request.Senha
        };

        var result = await _mediator.Send(command);
        if (result.Success && result.NumeroConta.HasValue)
            return Created(string.Empty, new { numeroConta = result.NumeroConta });
        return BadRequest(new { mensagem = result.Error ?? "Erro ao cadastrar conta.", tipo = result.Tipo ?? "UNKNOWN_ERROR" });
    }

    [HttpPost("conta/login")]
    public async Task<IActionResult> LoginContaCorrente([FromBody] LoginRequest request)
    {
        if ((request.NumeroConta == null || request.NumeroConta <= 0) && string.IsNullOrWhiteSpace(request.Cpf))
        {
            return Unauthorized(new { mensagem = "Informe o número da conta ou CPF.", tipo = "USER_UNAUTHORIZED" });
        }
        if (string.IsNullOrWhiteSpace(request.Senha))
        {
            return Unauthorized(new { mensagem = "Senha obrigatória.", tipo = "USER_UNAUTHORIZED" });
        }

        // Usar novo comando unificado
        var command = new EstudoApi.Domain.CQRS.Commands.ContaBancaria.LoginContaBancariaCommand
        {
            NumeroConta = request.NumeroConta,
            Cpf = request.Cpf,
            Senha = request.Senha
        };

        var result = await _mediator.Send(command);
        if (result.Success)
        {
            // Buscar conta para gerar token
            EstudoApi.Domain.Models.ContaBancaria? conta = null;
            if (request.NumeroConta.HasValue)
            {
                conta = await _contaBancariaRepository.GetByNumeroAsync(request.NumeroConta.Value);
            }
            else if (!string.IsNullOrWhiteSpace(request.Cpf))
            {
                conta = await _contaBancariaRepository.GetByCpfAsync(request.Cpf);
            }

            if (conta != null)
            {
                // Gerar token usando o serviço existente
                var tokenData = new AppUser 
                {
                    Id = conta.Id.ToString(),
                    Nome = conta.Nome,
                    Cpf = conta.Cpf
                };

                var (token, expiresAt) = _jwtTokenService.CreateToken(tokenData);
                return Ok(new { token, expiresAt });
            }
        }

        return Unauthorized(new { mensagem = result.Error ?? "Credenciais inválidas.", tipo = result.Tipo ?? "USER_UNAUTHORIZED" });
    }

    // DTOs para compatibilidade
    public class CadastroRequest
    {
        public string? Nome { get; set; }
        public string Cpf { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class LoginRequest
    {
        public int? NumeroConta { get; set; }
        public string? Cpf { get; set; }
        public string Senha { get; set; } = string.Empty;
    }
}
