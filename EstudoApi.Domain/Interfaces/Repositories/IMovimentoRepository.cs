using EstudoApi.Domain.Models;

namespace EstudoApi.Domain.Interfaces.Repositories
{
    public interface IMovimentoRepository
    {
        Task<Movimento> AddAsync(Movimento movimento);
        Task<IEnumerable<Movimento>> GetByContaAsync(string contaCorrenteId);
        Task<IEnumerable<Movimento>> GetByContaAndPeriodoAsync(string contaCorrenteId, DateTime dataInicio, DateTime dataFim);
        Task<decimal> GetSaldoAtualAsync(string contaCorrenteId);
        Task<decimal> GetSaldoEmDataAsync(string contaCorrenteId, DateTime data);
        Task<IEnumerable<Movimento>> GetUltimosMovimentosAsync(string contaCorrenteId, int quantidade = 10);
        Task<Movimento?> GetByIdAsync(string id);
        Task AddCreditoAsync(string contaCorrenteId, decimal valor, string? descricao = null);
        Task AddDebitoAsync(string contaCorrenteId, decimal valor, string? descricao = null);
    }
}
