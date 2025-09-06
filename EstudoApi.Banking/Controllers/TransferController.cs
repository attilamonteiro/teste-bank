using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using EstudoApi.Banking.Transfer.Commands;

namespace EstudoApi.Banking.Controllers
{
    /// <summary>
    /// Controller para operações de transferência bancária
    /// </summary>
    [ApiController]
    [Route("api/banking/transfer")]
    [Produces("application/json")]
    public class TransferController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TransferController> _logger;

        public TransferController(IMediator mediator, ILogger<TransferController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Realiza uma transferência entre contas correntes
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        ///
        ///     POST /api/banking/transfer
        ///     {
        ///         "requisicaoId": "req-transfer-123456",
        ///         "contaOrigem": 1001,
        ///         "contaDestino": 1002,
        ///         "valor": 250.00,
        ///         "descricao": "Pagamento de aluguel"
        ///     }
        ///
        /// **Campos:**
        /// - `requisicaoId`: ID único para idempotência (obrigatório)
        /// - `contaOrigem`: Conta de origem (obrigatória)
        /// - `contaDestino`: Conta de destino (obrigatória)
        /// - `valor`: Valor da transferência (obrigatório, positivo)
        /// - `descricao`: Descrição opcional
        /// 
        /// **Regras de Negócio:**
        /// - Usuário só pode transferir de sua própria conta
        /// - Ambas as contas devem existir e estar ativas
        /// - Conta origem deve ter saldo suficiente
        /// - Valor máximo por transferência: R$ 1.000.000,00
        /// </remarks>
        /// <param name="command">Dados da transferência</param>
        /// <response code="200">Transferência realizada com sucesso</response>
        /// <response code="400">Dados inválidos ou regra de negócio violada</response>
        /// <response code="401">Token JWT inválido</response>
        /// <response code="403">Usuário não autorizado para a operação</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(TransferSuccessResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 403)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Transfer([FromBody] TransferCommand command)
        {
            try
            {
                _logger.LogInformation("Iniciando transferência. RequisicaoId: {RequisicaoId}", command.RequisicaoId);

                // Extrai o accountId do token JWT usando claims
                var accountIdClaim = User.FindFirst("accountId")?.Value;
                if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out int tokenAccountId))
                {
                    _logger.LogWarning("Token JWT inválido ou accountId não encontrado");
                    return StatusCode(403, new ErrorResponse
                    {
                        mensagem = "Token inválido ou conta não identificada.",
                        tipo = "USER_UNAUTHORIZED"
                    });
                }

                // Validar que o usuário só pode transferir de sua própria conta
                if (command.ContaOrigem != tokenAccountId)
                {
                    _logger.LogWarning("Tentativa de transferência não autorizada. Usuario: {UserId}, ContaOrigem: {ContaOrigem}",
                        tokenAccountId, command.ContaOrigem);

                    return StatusCode(403, new ErrorResponse
                    {
                        mensagem = "Usuário só pode transferir de sua própria conta.",
                        tipo = "UNAUTHORIZED_ORIGIN_ACCOUNT"
                    });
                }

                // Executar a transferência via MediatR
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    _logger.LogInformation("Transferência realizada com sucesso. TransferId: {TransferId}", result.TransferId);

                    return Ok(new TransferSuccessResponse
                    {
                        transferId = result.TransferId!,
                        valor = result.ProcessedAmount!.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                        dataProcessamento = result.ProcessedAt!.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        status = "CONCLUIDA"
                    });
                }

                _logger.LogWarning("Transferência rejeitada. Erro: {Error}, Codigo: {ErrorCode}", result.Error, result.ErrorCode);

                return BadRequest(new ErrorResponse
                {
                    mensagem = result.Error!,
                    tipo = result.ErrorCode!
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante transferência. RequisicaoId: {RequisicaoId}", command.RequisicaoId);

                return StatusCode(500, new ErrorResponse
                {
                    mensagem = "Erro interno do servidor.",
                    tipo = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// Health check da API Banking
        /// </summary>
        [HttpGet("/health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", service = "EstudoApi.Banking", timestamp = DateTime.UtcNow });
        }
    }

    #region DTOs

    /// <summary>
    /// Resposta de sucesso da transferência
    /// </summary>
    public class TransferSuccessResponse
    {
        public string transferId { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string dataProcessamento { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta de erro padronizada
    /// </summary>
    public class ErrorResponse
    {
        public string mensagem { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
    }

    #endregion
}
