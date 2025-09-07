using EstudoApi.Domain.Models;
using EstudoApi.Domain.CQRS.Commands.Account;

namespace EstudoApi.Banking.Services
{
    public interface IContaCorrenteApiService
    {
        Task<MovementResult> RealizarMovimentacao(string token, AccountMovementCommand command);
        Task<ContaCorrente?> GetContaAsync(int numeroConta);
        Task<bool> PingAsync();
    }
}
