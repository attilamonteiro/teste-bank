using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EstudoApi.Domain.CQRS.Commands.Account;
using MediatR;

namespace EstudoApi.Controllers
{
    [ApiController]
    [Route("api/conta")]
    public class ContaCorrenteController : ControllerBase
    {

        private readonly IMediator _mediator;
        public ContaCorrenteController(IMediator mediator)
        {
            Console.WriteLine("[DEBUG] ContaCorrenteController instanciado");
            _mediator = mediator;
        }


        /// <summary>
        /// Consulta o saldo da conta corrente.
        /// </summary>
        /// <remarks>
        /// Exemplo de resposta:
        ///
        ///     {
        ///         "numeroConta": 123456,
        ///         "nomeTitular": "João da Silva",
        ///         "dataConsulta": "2023-10-10T10:00:00",
        ///         "saldo": "1000.00"
        ///     }
        /// </remarks>
        [HttpGet("saldo")]
        [Authorize]
        public async Task<IActionResult> ConsultarSaldo()
        {
            // Extrai o accountId do token JWT usando helper
            var accountId = EstudoApi.Helpers.JwtClaimHelper.ExtrairNumeroConta(User);
            if (!accountId.HasValue)
                return StatusCode(403, new { mensagem = "Token inválido ou expirado.", tipo = "USER_UNAUTHORIZED" });

            var result = await _mediator.Send(new EstudoApi.Domain.CQRS.Commands.Account.GetAccountBalanceQuery { NumeroConta = accountId.Value });
            if (!result.Found)
                return BadRequest(new { mensagem = "Apenas contas cadastradas podem consultar saldo.", tipo = "INVALID_ACCOUNT" });
            if (!result.Ativo)
                return BadRequest(new { mensagem = "Apenas contas ativas podem consultar saldo.", tipo = "INACTIVE_ACCOUNT" });
            if (result.Balance < 0)
                return BadRequest(new { mensagem = "Dados inconsistentes: saldo negativo.", tipo = "INCONSISTENT_DATA" });
            return Ok(new
            {
                numeroConta = result.NumeroConta,
                nomeTitular = result.NomeTitular,
                dataConsulta = result.DataConsulta.ToString("yyyy-MM-ddTHH:mm:ss"),
                saldo = result.Balance.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
            });
        }

        /// <summary>
        /// Realiza uma movimentação na conta corrente (crédito ou débito).
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        ///
        ///     POST /api/conta/movimentar
        ///     {
        ///         "requisicaoId": "req-123456",
        ///         "numeroConta": 1,
        ///         "valor": 100.50,
        ///         "tipo": "C"
        ///     }
        ///
        /// - `requisicaoId`: Identificação única da requisição
        /// - `numeroConta`: Número da conta (opcional, usa do token se não informado)
        /// - `valor`: Valor da movimentação (deve ser positivo)
        /// - `tipo`: Tipo de movimento ("C" = Crédito, "D" = Débito)
        /// 
        /// Regras de validação:
        /// - Apenas contas cadastradas podem receber movimentação
        /// - Apenas contas ativas podem receber movimentação
        /// - Apenas valores positivos são aceitos
        /// - Apenas tipos "C" ou "D" são aceitos
        /// - Apenas tipo "C" é aceito para conta diferente do usuário logado
        /// </remarks>
        [HttpPost("movimentar")]
        [Authorize]
        public async Task<IActionResult> Movimentar([FromBody] AccountMovementCommand command)
        {
            try
            {
                // Extrai o accountId do token JWT
                var tokenAccountId = EstudoApi.Helpers.JwtClaimHelper.ExtrairNumeroConta(User);
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
                        return BadRequest(new
                        {
                            mensagem = "Apenas o tipo 'crédito' pode ser aceito caso o número da conta seja diferente do usuário logado.",
                            tipo = "INVALID_TYPE"
                        });
                    }
                }

                // Executa a movimentação via MediatR
                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        mensagem = result.Error,
                        tipo = result.Tipo
                    });
                }

                // Retorna 204 No Content em caso de sucesso
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log do erro (idealmente usar ILogger)
                Console.WriteLine($"[ERROR] Erro na movimentação: {ex.Message}");
                return StatusCode(500, new
                {
                    mensagem = "Erro interno do servidor.",
                    tipo = "INTERNAL_ERROR"
                });
            }
        }

    }
}
