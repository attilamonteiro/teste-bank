using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using EstudoApi.Domain.CQRS.Commands.ContaBancaria;
using EstudoApi.Domain.Interfaces.Repositories;
using System.Security.Claims;
using EstudoApi.Infrastructure.Auth;
using EstudoApi.Infrastructure.Identity;

namespace EstudoApi.Controllers
{
    [ApiController]
    [Route("api/conta-bancaria")]
    public class ContaBancariaController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IContaBancariaRepository _repository;
        private readonly IJwtTokenService _jwtTokenService;

        public ContaBancariaController(IMediator mediator, IContaBancariaRepository repository, IJwtTokenService jwtTokenService)
        {
            _mediator = mediator;
            _repository = repository;
            _jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Cadastra uma nova conta bancária
        /// </summary>
        [HttpPost("cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] ContaBancariaCadastroRequest request)
        {
            try
            {
                var command = new CreateContaBancariaCommand
                {
                    Cpf = request.Cpf,
                    Senha = request.Senha
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Ok(new
                    {
                        numeroConta = result.NumeroConta,
                        mensagem = "Conta criada com sucesso! Já pode fazer login e operar."
                    });
                }

                return BadRequest(new
                {
                    mensagem = result.Error,
                    tipo = result.Tipo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = $"Erro interno do servidor: {ex.Message}",
                    tipo = "INTERNAL_ERROR",
                    details = ex.ToString()
                });
            }
        }

        /// <summary>
        /// Realiza login na conta bancária
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ContaBancariaLoginRequest request)
        {
            try
            {
                var command = new LoginContaBancariaCommand
                {
                    NumeroConta = request.NumeroConta,
                    Cpf = request.Cpf,
                    Senha = request.Senha
                };

                var result = await _mediator.Send(command);

                if (result.Success && result.Conta != null)
                {
                    // Gerar token JWT usando o método específico para conta
                    var (token, expiresAt) = _jwtTokenService.CreateTokenForAccount(result.Conta.Numero);

                    return Ok(new
                    {
                        token = token,
                        expiresAt = expiresAt,
                        mensagem = "Login realizado com sucesso."
                    });
                }

                return BadRequest(new
                {
                    mensagem = result.Error,
                    tipo = result.Tipo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = $"Erro interno do servidor no login: {ex.Message}",
                    tipo = "INTERNAL_ERROR",
                    details = ex.ToString()
                });
            }
        }

        /// <summary>
        /// Consulta o saldo da conta bancária
        /// </summary>
        [HttpGet("saldo")]
        [Authorize]
        public async Task<IActionResult> ConsultarSaldo()
        {
            try
            {
                var numeroConta = GetNumeroContaFromToken();
                if (!numeroConta.HasValue)
                    return StatusCode(403, new { mensagem = "Token inválido.", tipo = "INVALID_TOKEN" });

                var conta = await _repository.GetByNumeroAsync(numeroConta.Value);
                if (conta == null)
                    return BadRequest(new { mensagem = "Conta não encontrada.", tipo = "ACCOUNT_NOT_FOUND" });

                if (!conta.Ativo)
                    return BadRequest(new { mensagem = "Conta inativa.", tipo = "INACTIVE_ACCOUNT" });

                var saldo = conta.CalcularSaldo();

                return Ok(new
                {
                    numeroConta = conta.Numero,
                    nomeTitular = conta.Nome,
                    dataConsulta = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    saldo = saldo.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro interno do servidor.",
                    tipo = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Realiza uma movimentação na conta bancária
        /// </summary>
        [HttpPost("movimentar")]
        [Authorize]
        public async Task<IActionResult> Movimentar([FromBody] ContaBancariaMovimentacaoRequest request)
        {
            try
            {
                var numeroConta = GetNumeroContaFromToken();
                if (!numeroConta.HasValue)
                    return StatusCode(403, new { mensagem = "Token inválido.", tipo = "INVALID_TOKEN" });

                var command = new ContaBancariaMovementCommand
                {
                    RequisicaoId = request.RequisicaoId,
                    NumeroConta = request.NumeroConta ?? numeroConta.Value,
                    Valor = request.Valor,
                    Tipo = request.Tipo,
                    Descricao = request.Descricao
                };

                // Se a conta é diferente do usuário logado, apenas crédito é permitido
                if (command.NumeroConta != numeroConta.Value && command.Tipo != "C")
                {
                    return StatusCode(403, new
                    {
                        mensagem = "Apenas créditos podem ser realizados em contas de terceiros.",
                        tipo = "OPERATION_NOT_ALLOWED"
                    });
                }

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    // Buscar saldo atualizado
                    var conta = await _repository.GetByNumeroAsync(command.NumeroConta.Value);
                    var saldoFinal = conta?.CalcularSaldo() ?? 0;

                    return Ok(new
                    {
                        mensagem = "Movimentação realizada com sucesso.",
                        saldoFinal = saldoFinal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                    });
                }

                return BadRequest(new
                {
                    mensagem = result.Error,
                    tipo = result.Tipo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro interno do servidor.",
                    tipo = "INTERNAL_ERROR"
                });
            }
        }

        private int? GetNumeroContaFromToken()
        {
            var numeroContaClaim = User.FindFirst("idcontacorrente")?.Value;
            if (int.TryParse(numeroContaClaim, out int numeroConta))
                return numeroConta;
            return null;
        }

        // DTOs
        public class ContaBancariaCadastroRequest
        {
            public string Cpf { get; set; } = string.Empty;
            public string Senha { get; set; } = string.Empty;
        }

        public class ContaBancariaLoginRequest
        {
            public int? NumeroConta { get; set; }
            public string? Cpf { get; set; }
            public string Senha { get; set; } = string.Empty;
        }

        public class ContaBancariaMovimentacaoRequest
        {
            public string RequisicaoId { get; set; } = string.Empty;
            public int? NumeroConta { get; set; }
            public decimal Valor { get; set; }
            public string Tipo { get; set; } = string.Empty;
            public string? Descricao { get; set; }
        }
    }
}
