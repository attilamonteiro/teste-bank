using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EstudoApi.Domain.CQRS.Commands.ContaBancaria;
using MediatR;
using EstudoApi.Domain.Interfaces.Repositories;

namespace EstudoApi.Controllers
{
    [ApiController]
    [Route("api/conta")]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IContaBancariaRepository _repository;

        public ContaCorrenteController(IMediator mediator, IContaBancariaRepository repository)
        {
            _mediator = mediator;
            _repository = repository;
        }

        /// <summary>
        /// Consulta o saldo da conta corrente.
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
                    return BadRequest(new { mensagem = "Apenas contas ativas podem consultar saldo.", tipo = "INACTIVE_ACCOUNT" });

                var saldo = conta.CalcularSaldo();
                if (saldo < 0)
                    return BadRequest(new { mensagem = "Dados inconsistentes: saldo negativo.", tipo = "INCONSISTENT_DATA" });

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
                Console.WriteLine($"[ERROR] Erro na consulta de saldo: {ex.Message}");
                return StatusCode(500, new
                {
                    mensagem = "Erro interno do servidor.",
                    tipo = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Realiza uma movimentação na conta corrente (crédito ou débito).
        /// </summary>
        [HttpPost("movimentar")]
        [Authorize]
        public async Task<IActionResult> Movimentar([FromBody] AccountMovementCommand command)
        {
            try
            {
                // Extrai o accountId do token JWT
                var tokenAccountId = GetNumeroContaFromToken();
                if (!tokenAccountId.HasValue)
                    return StatusCode(403, new { mensagem = "Token inválido ou expirado.", tipo = "USER_UNAUTHORIZED" });

                // Se numeroConta não foi informado, usa o do token
                if (!command.NumeroConta.HasValue)
                {
                    command.NumeroConta = tokenAccountId.Value;
                }
                else
                {
                    // Se a conta é diferente do usuário logado, apenas crédito é permitido
                    if (command.NumeroConta.Value != tokenAccountId.Value && command.Tipo != "C")
                    {
                        return StatusCode(403, new
                        {
                            mensagem = "Apenas créditos podem ser realizados em contas de terceiros.",
                            tipo = "OPERATION_NOT_ALLOWED"
                        });
                    }
                }

                // Usar novo comando unificado
                var contaBancariaCommand = new ContaBancariaMovementCommand
                {
                    RequisicaoId = command.RequisicaoId,
                    NumeroConta = command.NumeroConta,
                    Valor = command.Valor,
                    Tipo = command.Tipo,
                    Descricao = command.Descricao
                };

                var result = await _mediator.Send(contaBancariaCommand);

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
                Console.WriteLine($"[ERROR] Erro na movimentação: {ex.Message}");
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

        // DTO para compatibilidade
        public class AccountMovementCommand
        {
            public string RequisicaoId { get; set; } = string.Empty;
            public int? NumeroConta { get; set; }
            public decimal Valor { get; set; }
            public string Tipo { get; set; } = string.Empty;
            public string? Descricao { get; set; }
        }
    }
}
