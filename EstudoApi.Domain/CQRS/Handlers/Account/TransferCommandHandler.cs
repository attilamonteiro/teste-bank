using MediatR;
using EstudoApi.Interfaces.Repositories;

namespace EstudoApi.Domain.CQRS.Handlers.Account
{
    public class TransferCommandHandler : IRequestHandler<Commands.Account.TransferCommand, (bool Success, string? Error, string? Tipo)>
    {
        private readonly IAccountRepository _repository;

        public TransferCommandHandler(IAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<(bool Success, string? Error, string? Tipo)> Handle(Commands.Account.TransferCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Validações básicas
                if (command.Valor <= 0)
                    return (false, "Valor deve ser positivo.", "INVALID_VALUE");

                if (command.ContaOrigem == command.ContaDestino)
                    return (false, "Conta de origem e destino não podem ser iguais.", "SAME_ACCOUNT");

                if (string.IsNullOrWhiteSpace(command.RequisicaoId))
                    return (false, "RequisicaoId é obrigatório.", "MISSING_REQUEST_ID");

                // Buscar contas
                var contaOrigem = await _repository.GetAccount(command.ContaOrigem);
                var contaDestino = await _repository.GetAccount(command.ContaDestino);

                // Validar conta de origem
                if (contaOrigem == null)
                    return (false, "Conta de origem não encontrada.", "ORIGIN_ACCOUNT_NOT_FOUND");

                if (!contaOrigem.Ativo)
                    return (false, "Conta de origem está inativa.", "INACTIVE_ORIGIN_ACCOUNT");

                // Validar conta de destino
                if (contaDestino == null)
                    return (false, "Conta de destino não encontrada.", "DESTINATION_ACCOUNT_NOT_FOUND");

                if (!contaDestino.Ativo)
                    return (false, "Conta de destino está inativa.", "INACTIVE_DESTINATION_ACCOUNT");

                // TODO: Verificar saldo suficiente usando repositório de movimentos
                // O modelo Account não tem propriedade Saldo direta
                // Implementar consulta ao repositório de movimentos para verificar saldo
                // var saldo = await _movimentoRepository.GetSaldoAtualAsync(contaOrigem.IdContaCorrente);
                // if (saldo < command.Valor)
                //     return (false, "Saldo insuficiente para transferência.", "INSUFFICIENT_BALANCE");

                // Executar operações de transferência
                var descricaoOrigem = $"Transferência enviada para conta {command.ContaDestino} - {command.Descricao ?? "Sem descrição"} - Req: {command.RequisicaoId}";
                var descricaoDestino = $"Transferência recebida da conta {command.ContaOrigem} - {command.Descricao ?? "Sem descrição"} - Req: {command.RequisicaoId}";

                // Débito na conta origem
                var debitoSucesso = contaOrigem.Withdraw(command.Valor, descricaoOrigem);
                if (!debitoSucesso)
                    return (false, "Erro ao debitar da conta de origem.", "DEBIT_ERROR");

                // Crédito na conta destino
                contaDestino.Deposit(command.Valor, descricaoDestino);

                // Persistir as alterações
                await _repository.UpdateAccount(contaOrigem);
                await _repository.UpdateAccount(contaDestino);

                return (true, null, null);
            }
            catch (Exception ex)
            {
                // Log do erro (idealmente usar ILogger)
                Console.WriteLine($"[ERROR] Erro na transferência: {ex.Message}");
                return (false, "Erro interno durante a transferência.", "TRANSFER_ERROR");
            }
        }
    }
}
