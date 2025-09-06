using EstudoApi.Domain.Models;

namespace EstudoApi.Domain.Interfaces.Repositories
{
    public interface IIdempotenciaRepository
    {
        Task<Idempotencia?> GetByChaveAsync(string chaveIdempotencia);
        Task<Idempotencia> AddAsync(Idempotencia idempotencia);
        Task UpdateAsync(Idempotencia idempotencia);
        Task<bool> ExisteChaveAsync(string chaveIdempotencia);
        Task<Idempotencia> CriarOuObterAsync(string chaveIdempotencia, string requisicao);
        Task AtualizarResultadoAsync(string chaveIdempotencia, string resultado);
        Task<IEnumerable<Idempotencia>> GetAllAsync();
        Task DeleteAsync(string chaveIdempotencia);
        Task<int> LimparAntigasAsync(DateTime dataLimite);
    }
}
