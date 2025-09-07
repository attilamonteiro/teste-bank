using EstudoApi.Domain.Models;

namespace EstudoApi.Domain.Interfaces.Repositories
{
    public interface IContaCorrenteRepository
    {
        Task<ContaCorrente?> GetByNumeroAsync(int numero);
        Task<ContaCorrente?> GetByIdAsync(string id);
        Task<decimal> GetSaldoAsync(string contaCorrenteId);
        Task<IEnumerable<ContaCorrente>> GetAllAsync();
        Task<ContaCorrente> AddAsync(ContaCorrente conta);
        Task UpdateAsync(ContaCorrente conta);
        Task DeleteAsync(string id);
        Task<bool> ExisteContaAsync(int numero);
        Task<bool> ValidarSenhaAsync(int numero, string senha);
        Task<Movimento> AddMovimentoAsync(Movimento movimento);
    }
}
