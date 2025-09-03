using System.Threading.Tasks;
using EstudoApi.Domain.CQRS.Commands.Account;

namespace EstudoApi.Domain.Services
{
    public interface IAccountService
    {
        Task<GetAccountBalanceResult> ConsultarSaldo(int numeroConta);
    }
}
