using MediatR;
using EstudoApi.Banking.Transfer.Commands;
using Microsoft.Extensions.Logging;
using EstudoApi.Infrastructure.Repositories;
using EstudoApi.Banking.Transfer.Services;
using Microsoft.AspNetCore.Http;

namespace EstudoApi.Banking.Transfer.Handlers
{
    // Handler fallback que replica a lógica completa enquanto o original não é carregado
    public class BankingTransferCommandHandler : IRequestHandler<TransferCommand, TransferResult>
    {
        private readonly ITransferenciaRepository _transferenciaRepository;
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly ITransferContaCorrenteApiService _contaCorrenteApiService;
        private readonly ILogger<BankingTransferCommandHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BankingTransferCommandHandler(
            ITransferenciaRepository transferenciaRepository,
            IIdempotenciaRepository idempotenciaRepository,
            ITransferContaCorrenteApiService contaCorrenteApiService,
            ILogger<BankingTransferCommandHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _transferenciaRepository = transferenciaRepository;
            _idempotenciaRepository = idempotenciaRepository;
            _contaCorrenteApiService = contaCorrenteApiService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TransferResult> Handle(TransferCommand command, CancellationToken cancellationToken)
        {
            var transferId = Guid.NewGuid().ToString();
            _logger.LogWarning("[FALLBACK] Processando transferência. RequisicaoId: {RequisicaoId} TransferId: {TransferId}", command.RequisicaoId, transferId);
            try
            {
                _logger.LogInformation("[FALLBACK] Passo 1: Validação");
                var validation = Validators.TransferValidator.ValidateCommand(command);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Validação falhou. Req: {Req} Erro: {Erro}", command.RequisicaoId, validation.ErrorMessage);
                    return TransferResult.CreateFailure(validation.ErrorMessage!, validation.ErrorCode!);
                }
                _logger.LogInformation("[FALLBACK] Validação OK");

                _logger.LogInformation("[FALLBACK] Validação OK");

                _logger.LogInformation("[FALLBACK] Passo 2: Verificando idempotência");
                var existingTransfers = await _transferenciaRepository.GetByContaAsync(command.ContaOrigem.ToString());
                var duplicateTransfer = existingTransfers.FirstOrDefault(t =>
                    t.IdContaCorrenteDestino == command.ContaDestino.ToString() &&
                    t.Valor == command.Valor &&
                    t.DataMovimento == DateTime.UtcNow.ToString("dd/MM/yyyy"));

                if (duplicateTransfer != null)
                {
                    _logger.LogInformation("Transferência já existe. Req: {Req}", command.RequisicaoId);
                    return TransferResult.CreateSuccess(duplicateTransfer.IdTransferencia, command.Valor, DateTime.UtcNow);
                }
                _logger.LogInformation("[FALLBACK] Idempotência OK");

                _logger.LogInformation("[FALLBACK] Passo 3: Criando registro");
                var transfer = new Domain.Models.Transferencia(command.ContaOrigem.ToString(), command.ContaDestino.ToString(), command.Valor)
                {
                    IdTransferencia = transferId
                };
                await _transferenciaRepository.AddAsync(transfer);
                _logger.LogInformation("Transfer registrada. TransferId: {TransferId}", transferId);

                _logger.LogInformation("[FALLBACK] Passo 4: Extraindo token");
                var token = ExtractBearerToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Token JWT não encontrado. Req: {Req}", command.RequisicaoId);
                    return TransferResult.CreateFailure("Token JWT não encontrado", "MISSING_TOKEN");
                }
                _logger.LogInformation("[FALLBACK] Token OK");

                _logger.LogInformation("[FALLBACK] Passo 5: Débito");

                _logger.LogInformation("[FALLBACK] Passo 5: Débito");
                var debitoCommand = new Domain.CQRS.Commands.Account.AccountMovementCommand
                {
                    RequisicaoId = $"{command.RequisicaoId}-DEBITO",
                    NumeroConta = command.ContaOrigem,
                    Valor = command.Valor,
                    Tipo = "D"
                };
                var debitoResult = await _contaCorrenteApiService.RealizarMovimentacao(token, debitoCommand);
                if (!debitoResult.IsSuccess)
                {
                    _logger.LogError("Falha no débito. Req: {Req} Erro: {Erro}", command.RequisicaoId, debitoResult.Error);
                    return TransferResult.CreateFailure(debitoResult.Error!, debitoResult.ErrorCode!);
                }
                _logger.LogInformation("[FALLBACK] Débito OK");

                _logger.LogInformation("[FALLBACK] Passo 6: Crédito");

                var creditoCommand = new Domain.CQRS.Commands.Account.AccountMovementCommand
                {
                    RequisicaoId = $"{command.RequisicaoId}-CREDITO",
                    NumeroConta = command.ContaDestino,
                    Valor = command.Valor,
                    Tipo = "C"
                };
                var creditoResult = await _contaCorrenteApiService.RealizarMovimentacao(token, creditoCommand);
                if (!creditoResult.IsSuccess)
                {
                    _logger.LogError("Falha no crédito. Iniciando estorno. Req: {Req} Erro: {Erro}", command.RequisicaoId, creditoResult.Error);
                    var estornoCommand = new Domain.CQRS.Commands.Account.AccountMovementCommand
                    {
                        RequisicaoId = $"{command.RequisicaoId}-ESTORNO",
                        NumeroConta = command.ContaOrigem,
                        Valor = command.Valor,
                        Tipo = "C"
                    };
                    var estornoResult = await _contaCorrenteApiService.RealizarMovimentacao(token, estornoCommand);
                    if (estornoResult.IsSuccess)
                        _logger.LogInformation("Estorno realizado. Req: {Req}", command.RequisicaoId);
                    else
                        _logger.LogError("Falha no estorno. Req: {Req} Erro: {Erro}", command.RequisicaoId, estornoResult.Error);
                    return TransferResult.CreateFailure(creditoResult.Error!, creditoResult.ErrorCode!);
                }

                _logger.LogInformation("Transferência concluída. Req: {Req} TransferId: {TransferId}", command.RequisicaoId, transferId);
                return TransferResult.CreateSuccess(transferId, command.Valor, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado durante transferência. Req: {Req}", command.RequisicaoId);
                return TransferResult.CreateFailure("Erro interno durante transferência", "INTERNAL_ERROR");
            }
        }

        private string? ExtractBearerToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var authorization = httpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (authorization?.StartsWith("Bearer ") == true)
                return authorization.Substring("Bearer ".Length).Trim();
            return null;
        }
    }
}
