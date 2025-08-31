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
            _mediator = mediator;
        }

        /// <summary>
        /// Cadastra uma nova conta corrente.
        /// </summary>
        /// <remarks>
        /// Exemplo de request:
        ///
        ///     {
        ///         "cpf": "12345678901",
        ///         "senha": "minhasenha123"
        ///     }
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 400)]
        [Swashbuckle.AspNetCore.Filters.SwaggerRequestExample(typeof(CreateAccountCommand), typeof(EstudoApi.SwaggerExamples.CreateAccountCommandExample))]
        public async Task<IActionResult> CadastrarConta([FromBody] CreateAccountCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.Success)
                return Created(string.Empty, new { numeroConta = result.NumeroConta });
            return BadRequest(new { mensagem = result.Error, tipo = result.Tipo });
        }

        [HttpPost("login")]
        [Swashbuckle.AspNetCore.Filters.SwaggerRequestExample(typeof(LoginAccountCommand), typeof(EstudoApi.SwaggerExamples.LoginAccountCommandExample))]
        public async Task<IActionResult> Login([FromBody] LoginAccountCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.Success)
                return Ok(new { token = result.Token });
            return Unauthorized(new { mensagem = result.Error, tipo = result.Tipo });
        }

        [HttpPost("inativar")]
        public async Task<IActionResult> InativarConta([FromBody] InactivateAccountCommand command)
        {
            // Recupera o ID da conta do token JWT
            var accountIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out var accountId))
                return Forbid();
            command.AccountId = accountId;
            var result = await _mediator.Send(command);
            if (result.Success)
                return NoContent();
            if (result.Tipo == "USER_UNAUTHORIZED")
                return StatusCode(403, new { mensagem = result.Error, tipo = result.Tipo });
            if (result.Tipo == "INVALID_ACCOUNT" || result.Tipo == "INACTIVE_ACCOUNT")
                return BadRequest(new { mensagem = result.Error, tipo = result.Tipo });
            return BadRequest(new { mensagem = result.Error, tipo = result.Tipo });
        }

        [HttpPost("movimentacao")]
        [Swashbuckle.AspNetCore.Filters.SwaggerRequestExample(typeof(AccountMovementCommand), typeof(EstudoApi.SwaggerExamples.AccountMovementCommandExample))]
        public async Task<IActionResult> MovimentarConta([FromBody] AccountMovementCommand command)
        {
            // Recupera o número da conta do token se não informado
            if (command.NumeroConta == null)
            {
                var accountIdClaim = User.FindFirst("accountId")?.Value;
                if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out var accountId))
                    return Forbid();
                command.NumeroConta = accountId;
            }
            var result = await _mediator.Send(command);
            if (result.Success)
                return NoContent();
            if (result.Tipo == "INVALID_ACCOUNT" || result.Tipo == "INACTIVE_ACCOUNT" || result.Tipo == "INVALID_VALUE" || result.Tipo == "INVALID_TYPE")
                return BadRequest(new { mensagem = result.Error, tipo = result.Tipo });
            return BadRequest(new { mensagem = result.Error, tipo = result.Tipo });
        }

        [HttpGet("saldo")]
        public async Task<IActionResult> ConsultarSaldo()
        {
            // Recupera o número da conta do token JWT
            var accountIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out var accountId))
                return Forbid();

            // Buscar conta
            var result = await _mediator.Send(new GetAccountBalanceQuery { NumeroConta = accountId });
            if (!result.Found)
                return NotFound(new { mensagem = "Conta não encontrada.", tipo = "INVALID_ACCOUNT" });
            if (!result.Ativo)
                return BadRequest(new { mensagem = "Apenas contas ativas podem consultar saldo.", tipo = "INACTIVE_ACCOUNT" });
            if (result.Balance < 0)
                return BadRequest(new { mensagem = "Dados inconsistentes: saldo negativo.", tipo = "INCONSISTENT_DATA" });
            return Ok(new
            {
                numeroConta = result.NumeroConta,
                nomeTitular = result.NomeTitular,
                dataConsulta = result.DataConsulta.ToString("yyyy-MM-ddTHH:mm:ss"),
                saldo = result.Balance.ToString("F2")
            });
        }
    }
}
