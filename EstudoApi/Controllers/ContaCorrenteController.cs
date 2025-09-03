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

    }
}
