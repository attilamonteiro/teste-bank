using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EstudoApi.Domain.CQRS.Commands.Account;
using EstudoApi.Interfaces.Repositories;

namespace EstudoApi.Domain.CQRS.Handlers.Account
{
    public class GetAccountBalanceQueryHandler : IRequestHandler<GetAccountBalanceQuery, GetAccountBalanceResult>
    {
        private readonly IAccountRepository _repository;
        public GetAccountBalanceQueryHandler(IAccountRepository repository)
        {
            _repository = repository;
        }
        public async Task<GetAccountBalanceResult> Handle(GetAccountBalanceQuery query, CancellationToken cancellationToken)
        {
            var account = await _repository.GetAccount(query.NumeroConta);
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
