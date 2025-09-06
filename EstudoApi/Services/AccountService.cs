using System;
using System.Threading.Tasks;
using System.Linq;
using EstudoApi.Interfaces.Repositories;
using EstudoApi.Domain.CQRS.Commands.Account;
using EstudoApi.Domain.Services;

namespace EstudoApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repository;
        public AccountService(IAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetAccountBalanceResult> ConsultarSaldo(int numeroConta)
        {
            var account = await _repository.GetAccount(numeroConta);
            if (account == null)
                return new GetAccountBalanceResult { Found = false };

            decimal saldo = 0;
            if (account.Transactions != null && account.Transactions.Count > 0)
                saldo = account.Transactions.Sum(t => t.Amount);

            return new GetAccountBalanceResult
            {
                Found = true,
                Ativo = account.Ativo,
                Balance = saldo,
                NumeroConta = account.Id,
                NomeTitular = account.Cpf, // Substitua por nome se existir campo
                DataConsulta = DateTime.Now
            };
        }
    }
}
