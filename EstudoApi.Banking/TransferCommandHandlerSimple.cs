using MediatR;
using EstudoApi.Banking.Transfer.Commands;
using EstudoApi.Infrastructure.Repositories;
using EstudoApi.Banking.Transfer.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace EstudoApi.Banking.Transfer
{
    // Handler simples temporário para diagnosticar problema de registro
    public class TransferCommandHandlerSimple : IRequestHandler<TransferCommand, TransferResult>
    {
        private readonly ITransferenciaRepository _transferenciaRepository;
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly ITransferContaCorrenteApiService _contaCorrenteApiService;
        private readonly ILogger<TransferCommandHandlerSimple> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransferCommandHandlerSimple(
            ITransferenciaRepository transferenciaRepository,
            IIdempotenciaRepository idempotenciaRepository,
            ITransferContaCorrenteApiService contaCorrenteApiService,
            ILogger<TransferCommandHandlerSimple> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _transferenciaRepository = transferenciaRepository;
            _idempotenciaRepository = idempotenciaRepository;
            _contaCorrenteApiService = contaCorrenteApiService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TransferResult> Handle(TransferCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[SIMPLE HANDLER] Iniciando processamento simples da transferência {Req}", request.RequisicaoId);
            // Apenas validação mínima
            if (request.Valor <= 0)
            {
                return TransferResult.CreateFailure("Valor inválido", "INVALID_AMOUNT");
            }
            // Simular registro e retorno
            var transferId = Guid.NewGuid().ToString();
            return TransferResult.CreateSuccess(transferId, request.Valor, DateTime.UtcNow);
        }
    }
}
