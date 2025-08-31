using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EstudoApi.Domain.CQRS.Commands.Account;
using EstudoApi.Interfaces.Repositories;

namespace EstudoApi.Domain.CQRS.Handlers.Account
{
    public class AccountMovementCommandHandler : IRequestHandler<AccountMovementCommand, (bool Success, string? Error, string? Tipo)>
    {
        private readonly IAccountRepository _repository;
        public AccountMovementCommandHandler(IAccountRepository repository)
        {
            _repository = repository;
        }
        public async Task<(bool Success, string? Error, string? Tipo)> Handle(AccountMovementCommand command, CancellationToken cancellationToken)
        {
            // Validação de valor
            if (command.Valor <= 0)
                return (false, "Apenas valores positivos podem ser recebidos.", "INVALID_VALUE");
            if (command.Tipo != "C" && command.Tipo != "D")
                return (false, "Apenas os tipos 'débito' ou 'crédito' podem ser aceitos.", "INVALID_TYPE");

            // Buscar conta
            var numeroConta = command.NumeroConta;
            if (numeroConta == null)
                return (false, "Conta não informada.", "INVALID_ACCOUNT");
            var account = await _repository.GetAccount(numeroConta.Value);
            if (account == null)
                return (false, "Apenas contas correntes cadastradas podem receber movimentação.", "INVALID_ACCOUNT");
            if (!account.Ativo)
                return (false, "Apenas contas correntes ativas podem receber movimentação.", "INACTIVE_ACCOUNT");

            // Lógica de movimentação
            if (command.Tipo == "C")
            {
                account.Deposit(command.Valor, $"Crédito via API, req: {command.RequisicaoId}");
            }
            else if (command.Tipo == "D")
            {
                var success = account.Withdraw(command.Valor, $"Débito via API, req: {command.RequisicaoId}");
                if (!success)
                    return (false, "Saldo insuficiente para débito.", "INVALID_VALUE");
            }
            await _repository.UpdateAccount(account);
            return (true, null, null);
        }
    }
}
