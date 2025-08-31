using System.Threading.Tasks;
using EstudoApi.Domain.CQRS.Commands.Account;
using EstudoApi.Interfaces.Repositories;

namespace EstudoApi.Domain.CQRS.Handlers.Account
{
    public class WithdrawCommandHandler
    {
        private readonly IAccountRepository _repository;
        public WithdrawCommandHandler(IAccountRepository repository)
        {
            _repository = repository;
        }
        public async Task<bool> Handle(WithdrawCommand command)
        {
            var account = await _repository.GetAccount(command.AccountId);
            if (account == null) return false;
            var success = account.Withdraw(command.Amount, command.Description);
            if (!success) return false;
            await _repository.UpdateAccount(account);
            return true;
        }
    }
}
