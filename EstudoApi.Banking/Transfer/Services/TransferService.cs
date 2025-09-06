using EstudoApi.Domain.Models;
using EstudoApi.Interfaces.Repositories;

namespace EstudoApi.Banking.Transfer.Services
{
    /// <summary>
    /// Serviço especializado em operações de transferência
    /// </summary>
    public interface ITransferService
    {
        Task<TransferOperationResult> ExecuteTransferAsync(
            int fromAccountId,
            int toAccountId,
            decimal amount,
            string description,
            string requestId);
    }

    public class TransferService : ITransferService
    {
        private readonly IAccountRepository _accountRepository;

        public TransferService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<TransferOperationResult> ExecuteTransferAsync(
            int fromAccountId,
            int toAccountId,
            decimal amount,
            string description,
            string requestId)
        {
            try
            {
                // Buscar contas
                var fromAccount = await _accountRepository.GetAccount(fromAccountId);
                var toAccount = await _accountRepository.GetAccount(toAccountId);

                // Validações
                var validation = ValidateTransfer(fromAccount, toAccount, amount);
                if (!validation.IsValid)
                    return TransferOperationResult.Failure(validation.ErrorMessage ?? "Erro de validação", validation.ErrorCode ?? "VALIDATION_ERROR");

                // Executar a transferência
                var transferId = Guid.NewGuid().ToString();
                var transferDescription = $"{description} - Transfer ID: {transferId} - Req: {requestId}";

                // Operação de débito na conta origem
                var debitSuccess = fromAccount!.Withdraw(amount, $"Débito - {transferDescription}");
                if (!debitSuccess)
                    return TransferOperationResult.Failure("Falha ao debitar da conta origem", "DEBIT_FAILED");

                // Operação de crédito na conta destino
                toAccount!.Deposit(amount, $"Crédito - {transferDescription}");

                // Persistir as alterações
                await _accountRepository.UpdateAccount(fromAccount);
                await _accountRepository.UpdateAccount(toAccount);

                return TransferOperationResult.Success(transferId, amount, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                // Log do erro (em produção, usar ILogger)
                Console.WriteLine($"[ERROR] Transfer Service - {ex.Message}");
                return TransferOperationResult.Failure("Erro interno durante transferência", "TRANSFER_ERROR");
            }
        }

        private static TransferValidationResult ValidateTransfer(
            Account? fromAccount,
            Account? toAccount,
            decimal amount)
        {
            if (fromAccount == null)
                return TransferValidationResult.Invalid("Conta de origem não encontrada", "ORIGIN_ACCOUNT_NOT_FOUND");

            if (toAccount == null)
                return TransferValidationResult.Invalid("Conta de destino não encontrada", "DESTINATION_ACCOUNT_NOT_FOUND");

            if (!fromAccount.Ativo)
                return TransferValidationResult.Invalid("Conta de origem está inativa", "INACTIVE_ORIGIN_ACCOUNT");

            if (!toAccount.Ativo)
                return TransferValidationResult.Invalid("Conta de destino está inativa", "INACTIVE_DESTINATION_ACCOUNT");

            // Validação de saldo deve ser feita via API ou repositório de movimentos
            // fromAccount não tem propriedade Saldo direta
            // TODO: Implementar consulta de saldo via API ou repositório
            // if (saldoOrigem < amount)
            //     return TransferValidationResult.Invalid("Saldo insuficiente na conta origem", "INSUFFICIENT_BALANCE");

            return TransferValidationResult.Valid();
        }
    }

    /// <summary>
    /// Resultado da operação de transferência
    /// </summary>
    public class TransferOperationResult
    {
        public bool IsSuccess { get; private set; }
        public string? Error { get; private set; }
        public string? ErrorCode { get; private set; }
        public string? TransferId { get; private set; }
        public decimal? Amount { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        private TransferOperationResult() { }

        public static TransferOperationResult Success(string transferId, decimal amount, DateTime processedAt)
        {
            return new TransferOperationResult
            {
                IsSuccess = true,
                TransferId = transferId,
                Amount = amount,
                ProcessedAt = processedAt
            };
        }

        public static TransferOperationResult Failure(string error, string errorCode)
        {
            return new TransferOperationResult
            {
                IsSuccess = false,
                Error = error,
                ErrorCode = errorCode
            };
        }
    }

    /// <summary>
    /// Resultado da validação de transferência
    /// </summary>
    internal class TransferValidationResult
    {
        public bool IsValid { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? ErrorCode { get; private set; }

        private TransferValidationResult() { }

        public static TransferValidationResult Valid() => new() { IsValid = true };

        public static TransferValidationResult Invalid(string message, string code) => new()
        {
            IsValid = false,
            ErrorMessage = message,
            ErrorCode = code
        };
    }
}
