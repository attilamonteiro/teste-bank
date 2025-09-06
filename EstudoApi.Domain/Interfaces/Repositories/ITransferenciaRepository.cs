using EstudoApi.Domain.Models;

namespace EstudoApi.Domain.Interfaces.Repositories
{
    public interface ITransferenciaRepository
    {
        Task<Transferencia> AddAsync(Transferencia transferencia);
        Task<Transferencia?> GetByIdAsync(string id);
        Task<IEnumerable<Transferencia>> GetByContaAsync(string contaCorrenteId);
        Task<IEnumerable<Transferencia>> GetTransferenciasEnviadasAsync(string contaCorrenteOrigemId);
        Task<IEnumerable<Transferencia>> GetTransferenciasRecebidasAsync(string contaCorrenteDestinoId);
        Task<IEnumerable<Transferencia>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, string? contaCorrenteId = null);
        Task<decimal> GetTotalTransferidoAsync(string contaCorrenteOrigemId, DateTime? dataInicio = null);
        Task<decimal> GetTotalRecebidoAsync(string contaCorrenteDestinoId, DateTime? dataInicio = null);
        Task<IEnumerable<Transferencia>> GetUltimasTransferenciasAsync(string contaCorrenteId, int quantidade = 10);
        Task<bool> ExisteTransferenciaAsync(string contaOrigemId, string contaDestinoId, decimal valor, DateTime dataTransferencia);
        Task<Transferencia> ProcessarTransferenciaAsync(string contaOrigemId, string contaDestinoId, decimal valor);
    }
}
