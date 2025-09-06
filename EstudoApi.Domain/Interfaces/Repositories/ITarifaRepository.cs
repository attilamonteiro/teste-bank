using EstudoApi.Domain.Models;

namespace EstudoApi.Domain.Interfaces.Repositories
{
    public interface ITarifaRepository
    {
        Task<Tarifa> AddAsync(Tarifa tarifa);
        Task<Tarifa?> GetByIdAsync(string id);
        Task<IEnumerable<Tarifa>> GetByContaAsync(string contaCorrenteId);
        Task<IEnumerable<Tarifa>> GetByPeriodoAsync(string contaCorrenteId, DateTime dataInicio, DateTime dataFim);
        Task<decimal> GetTotalTarifasAsync(string contaCorrenteId, DateTime? dataInicio = null);
        Task<decimal> GetTotalTarifasMensalAsync(string contaCorrenteId, int ano, int mes);
        Task<IEnumerable<Tarifa>> GetTarifasRecentesAsync(string contaCorrenteId, int quantidade = 10);
        Task<Tarifa> CobrarTarifaTransferenciaAsync(string contaCorrenteId, decimal valorTarifa);
        Task<IEnumerable<Tarifa>> GetAllAsync();
        Task DeleteAsync(string id);
        Task<Dictionary<string, decimal>> GetResumoTarifasPorContaAsync(DateTime dataInicio, DateTime dataFim);
    }
}
