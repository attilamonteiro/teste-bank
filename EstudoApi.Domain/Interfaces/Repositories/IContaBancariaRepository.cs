using EstudoApi.Domain.Models;

namespace EstudoApi.Domain.Interfaces.Repositories
{
    public interface IContaBancariaRepository
    {
        Task<ContaBancaria?> GetByIdAsync(string id);
        Task<ContaBancaria?> GetByNumeroAsync(int numero);
        Task<ContaBancaria?> GetByCpfAsync(string cpf);
        Task<IEnumerable<ContaBancaria>> GetAllAsync();
        Task<ContaBancaria> AddAsync(ContaBancaria conta);
        Task UpdateAsync(ContaBancaria conta);
        Task DeleteAsync(string id);
        Task<bool> ExisteContaAsync(int numero);
        Task<bool> ExisteCpfAsync(string cpf);
        Task<bool> ValidarSenhaAsync(int numero, string senha);
        Task<decimal> GetSaldoAsync(string contaId);
        Task<Movimento> AddMovimentoAsync(Movimento movimento);
        Task<IEnumerable<Movimento>> GetMovimentosAsync(string contaId);
    }
}
