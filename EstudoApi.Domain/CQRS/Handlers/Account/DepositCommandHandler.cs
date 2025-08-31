using System.Threading.Tasks;
using EstudoApi.Domain.CQRS.Commands.Account;
using EstudoApi.Interfaces.Repositories;

namespace EstudoApi.Domain.CQRS.Handlers.Account
{
    public class DepositCommandHandler
    {
        private readonly IAccountRepository _repository;
        public DepositCommandHandler(IAccountRepository repository)
        {
            _repository = repository;
        }
        public async Task<bool> Handle(DepositCommand command)
        {
            var account = await _repository.GetAccount(command.AccountId);
            if (account == null) return false;
            account.Deposit(command.Amount, command.Description);
            await _repository.UpdateAccount(account);
            return true;
        }
    }
}
