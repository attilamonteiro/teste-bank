using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EstudoApi.Domain.CQRS.Commands.Account;
using EstudoApi.Interfaces.Repositories;

namespace EstudoApi.Domain.CQRS.Handlers.Account
{
    public class InactivateAccountCommandHandler : IRequestHandler<InactivateAccountCommand, (bool Success, string? Error, string? Tipo)>
    {
        private readonly IAccountRepository _repository;
        public InactivateAccountCommandHandler(IAccountRepository repository)
        {
            _repository = repository;
        }
        public async Task<(bool Success, string? Error, string? Tipo)> Handle(InactivateAccountCommand command, CancellationToken cancellationToken)
        {
            var account = await _repository.GetAccount(command.AccountId);
            if (account == null)
                return (false, "Apenas contas correntes cadastradas podem ser inativadas.", "INVALID_ACCOUNT");
            if (!account.Ativo)
                return (false, "Conta já está inativa.", "INACTIVE_ACCOUNT");
            if (!account.ValidarSenha(command.Senha))
                return (false, "Senha inválida.", "USER_UNAUTHORIZED");
            account.Inativar();
            await _repository.UpdateAccount(account);
            return (true, null, null);
        }
    }
}
