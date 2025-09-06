using MediatR;

namespace EstudoApi.Banking.Transfer.Commands
{
    /// <summary>
    /// Comando para realizar transferência entre contas correntes
    /// </summary>
    public class TransferCommand : IRequest<TransferResult>
    {
        /// <summary>
        /// Identificação única da requisição para controle de idempotência
        /// </summary>
        public string RequisicaoId { get; set; } = string.Empty;

        /// <summary>
        /// Número da conta de origem (obrigatório)
        /// </summary>
        public int ContaOrigem { get; set; }

        /// <summary>
        /// Número da conta de destino (obrigatório)
        /// </summary>
        public int ContaDestino { get; set; }

        /// <summary>
        /// Valor da transferência (deve ser positivo)
        /// </summary>
        public decimal Valor { get; set; }

        /// <summary>
        /// Descrição opcional da transferência
        /// </summary>
        public string? Descricao { get; set; }
    }

    /// <summary>
    /// Resultado da operação de transferência
    /// </summary>
    public class TransferResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? ErrorCode { get; set; }
        public string? TransferId { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public decimal? ProcessedAmount { get; set; }

        /// <summary>
        /// Cria um resultado de sucesso
        /// </summary>
        public static TransferResult CreateSuccess(string transferId, decimal amount, DateTime processedAt)
        {
            return new TransferResult
            {
                Success = true,
                TransferId = transferId,
                ProcessedAmount = amount,
                ProcessedAt = processedAt
            };
        }

        /// <summary>
        /// Cria um resultado de falha
        /// </summary>
        public static TransferResult CreateFailure(string error, string errorCode)
        {
            return new TransferResult
            {
                Success = false,
                Error = error,
                ErrorCode = errorCode
            };
        }
    }
}
