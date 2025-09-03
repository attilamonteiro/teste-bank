using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EstudoApi.Domain.CQRS.Commands.Account;
using EstudoApi.Interfaces.Repositories;

namespace EstudoApi.Domain.CQRS.Handlers.Account
{
    public class GetAccountBalanceQueryHandler : IRequestHandler<GetAccountBalanceQuery, GetAccountBalanceResult>
    {
        private readonly EstudoApi.Domain.Services.IAccountService _service;
        public GetAccountBalanceQueryHandler(EstudoApi.Domain.Services.IAccountService service)
        {
            _service = service;
        }
        public async Task<GetAccountBalanceResult> Handle(GetAccountBalanceQuery query, CancellationToken cancellationToken)
        {
            return await _service.ConsultarSaldo(query.NumeroConta);
        }
    }
}
