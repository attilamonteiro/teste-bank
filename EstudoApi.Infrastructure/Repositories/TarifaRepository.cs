using EstudoApi.Domain.Models;
using EstudoApi.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EstudoApi.Infrastructure.Repositories
{
    public class TarifaRepository : ITarifaRepository
    {
        private readonly AppDbContext _context;

        public TarifaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Tarifa> AddAsync(Tarifa tarifa)
        {
            await _context.Tarifas.AddAsync(tarifa);
            await _context.SaveChangesAsync();
            return tarifa;
        }

        public async Task<Tarifa?> GetByIdAsync(string id)
        {
            return await _context.Tarifas
                .Include(t => t.ContaCorrente)
                .FirstOrDefaultAsync(t => t.IdTarifa == id);
        }

        public async Task<IEnumerable<Tarifa>> GetByContaAsync(string contaCorrenteId)
        {
            return await _context.Tarifas
                .Where(t => t.IdContaCorrente == contaCorrenteId)
                .Include(t => t.ContaCorrente)
                .OrderByDescending(t => t.DataMovimento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tarifa>> GetByPeriodoAsync(
            string contaCorrenteId,
            DateTime dataInicio,
            DateTime dataFim)
        {
            var dataInicioStr = dataInicio.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var dataFimStr = dataFim.ToString("yyyy-MM-dd HH:mm:ss.fff");

            return await _context.Tarifas
                .Where(t => t.IdContaCorrente == contaCorrenteId &&
                           string.Compare(t.DataMovimento, dataInicioStr) >= 0 &&
                           string.Compare(t.DataMovimento, dataFimStr) <= 0)
                .Include(t => t.ContaCorrente)
                .OrderByDescending(t => t.DataMovimento)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalTarifasAsync(string contaCorrenteId, DateTime? dataInicio = null)
        {
            var query = _context.Tarifas
                .Where(t => t.IdContaCorrente == contaCorrenteId);

            if (dataInicio.HasValue)
            {
                var dataInicioStr = dataInicio.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
                query = query.Where(t => string.Compare(t.DataMovimento, dataInicioStr) >= 0);
            }

            return await query.SumAsync(t => t.Valor);
        }

        public async Task<decimal> GetTotalTarifasMensalAsync(string contaCorrenteId, int ano, int mes)
        {
            var dataInicio = new DateTime(ano, mes, 1);
            var dataFim = dataInicio.AddMonths(1).AddDays(-1);

            return await GetTotalTarifasAsync(contaCorrenteId, dataInicio);
        }

        public async Task<IEnumerable<Tarifa>> GetTarifasRecentesAsync(string contaCorrenteId, int quantidade = 10)
        {
            return await _context.Tarifas
                .Where(t => t.IdContaCorrente == contaCorrenteId)
                .Include(t => t.ContaCorrente)
                .OrderByDescending(t => t.DataMovimento)
                .Take(quantidade)
                .ToListAsync();
        }

        public async Task<Tarifa> CobrarTarifaTransferenciaAsync(string contaCorrenteId, decimal valorTarifa)
        {
            var tarifa = new Tarifa
            {
                IdTarifa = Guid.NewGuid().ToString(),
                IdContaCorrente = contaCorrenteId,
                DataMovimento = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                Valor = valorTarifa
            };

            // Criar também um movimento de débito correspondente
            var movimento = new Movimento
            {
                IdMovimento = Guid.NewGuid().ToString(),
                IdContaCorrente = contaCorrenteId,
                DataMovimento = tarifa.DataMovimento,
                TipoMovimento = "D",
                Valor = valorTarifa
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Tarifas.AddAsync(tarifa);
                await _context.Movimentos.AddAsync(movimento);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return tarifa;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Tarifa>> GetAllAsync()
        {
            return await _context.Tarifas
                .Include(t => t.ContaCorrente)
                .OrderByDescending(t => t.DataMovimento)
                .ToListAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var tarifa = await GetByIdAsync(id);
            if (tarifa != null)
            {
                _context.Tarifas.Remove(tarifa);
                await _context.SaveChangesAsync();
            }
        }

        // Métodos para relatórios de tarifas
        public async Task<Dictionary<string, decimal>> GetResumoTarifasPorContaAsync(DateTime dataInicio, DateTime dataFim)
        {
            var dataInicioStr = dataInicio.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var dataFimStr = dataFim.ToString("yyyy-MM-dd HH:mm:ss.fff");

            var resumo = await _context.Tarifas
                .Where(t => string.Compare(t.DataMovimento, dataInicioStr) >= 0 &&
                           string.Compare(t.DataMovimento, dataFimStr) <= 0)
                .GroupBy(t => t.IdContaCorrente)
                .Select(g => new { ContaId = g.Key, Total = g.Sum(t => t.Valor) })
                .ToDictionaryAsync(x => x.ContaId, x => x.Total);

            return resumo;
        }
    }

    // Interface para o repositório
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
