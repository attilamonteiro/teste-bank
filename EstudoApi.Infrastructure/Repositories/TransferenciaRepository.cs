using EstudoApi.Domain.Models;
using EstudoApi.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EstudoApi.Infrastructure.Repositories
{
    public class TransferenciaRepository : ITransferenciaRepository
    {
        private readonly AppDbContext _context;

        public TransferenciaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Transferencia> AddAsync(Transferencia transferencia)
        {
            await _context.Transferencias.AddAsync(transferencia);
            await _context.SaveChangesAsync();
            return transferencia;
        }

        public async Task<Transferencia?> GetByIdAsync(string id)
        {
            return await _context.Transferencias
                .Include(t => t.ContaOrigem)
                .Include(t => t.ContaDestino)
                .FirstOrDefaultAsync(t => t.IdTransferencia == id);
        }

        public async Task<IEnumerable<Transferencia>> GetByContaAsync(string contaCorrenteId)
        {
            return await _context.Transferencias
                .Where(t => t.IdContaCorrenteOrigem == contaCorrenteId ||
                           t.IdContaCorrenteDestino == contaCorrenteId)
                .Include(t => t.ContaOrigem)
                .Include(t => t.ContaDestino)
                .OrderByDescending(t => t.DataMovimento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transferencia>> GetTransferenciasEnviadasAsync(string contaCorrenteOrigemId)
        {
            return await _context.Transferencias
                .Where(t => t.IdContaCorrenteOrigem == contaCorrenteOrigemId)
                .Include(t => t.ContaOrigem)
                .Include(t => t.ContaDestino)
                .OrderByDescending(t => t.DataMovimento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transferencia>> GetTransferenciasRecebidasAsync(string contaCorrenteDestinoId)
        {
            return await _context.Transferencias
                .Where(t => t.IdContaCorrenteDestino == contaCorrenteDestinoId)
                .Include(t => t.ContaOrigem)
                .Include(t => t.ContaDestino)
                .OrderByDescending(t => t.DataMovimento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transferencia>> GetByPeriodoAsync(
            DateTime dataInicio,
            DateTime dataFim,
            string? contaCorrenteId = null)
        {
            var dataInicioStr = dataInicio.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var dataFimStr = dataFim.ToString("yyyy-MM-dd HH:mm:ss.fff");

            var query = _context.Transferencias.AsQueryable();

            if (!string.IsNullOrEmpty(contaCorrenteId))
            {
                query = query.Where(t => t.IdContaCorrenteOrigem == contaCorrenteId ||
                                        t.IdContaCorrenteDestino == contaCorrenteId);
            }

            return await query
                .Where(t => string.Compare(t.DataMovimento, dataInicioStr) >= 0 &&
                           string.Compare(t.DataMovimento, dataFimStr) <= 0)
                .Include(t => t.ContaOrigem)
                .Include(t => t.ContaDestino)
                .OrderByDescending(t => t.DataMovimento)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalTransferidoAsync(string contaCorrenteOrigemId, DateTime? dataInicio = null)
        {
            var query = _context.Transferencias
                .Where(t => t.IdContaCorrenteOrigem == contaCorrenteOrigemId);

            if (dataInicio.HasValue)
            {
                var dataInicioStr = dataInicio.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
                query = query.Where(t => string.Compare(t.DataMovimento, dataInicioStr) >= 0);
            }

            return await query.SumAsync(t => t.Valor);
        }

        public async Task<decimal> GetTotalRecebidoAsync(string contaCorrenteDestinoId, DateTime? dataInicio = null)
        {
            var query = _context.Transferencias
                .Where(t => t.IdContaCorrenteDestino == contaCorrenteDestinoId);

            if (dataInicio.HasValue)
            {
                var dataInicioStr = dataInicio.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
                query = query.Where(t => string.Compare(t.DataMovimento, dataInicioStr) >= 0);
            }

            return await query.SumAsync(t => t.Valor);
        }

        public async Task<IEnumerable<Transferencia>> GetUltimasTransferenciasAsync(
            string contaCorrenteId,
            int quantidade = 10)
        {
            return await _context.Transferencias
                .Where(t => t.IdContaCorrenteOrigem == contaCorrenteId ||
                           t.IdContaCorrenteDestino == contaCorrenteId)
                .Include(t => t.ContaOrigem)
                .Include(t => t.ContaDestino)
                .OrderByDescending(t => t.DataMovimento)
                .Take(quantidade)
                .ToListAsync();
        }

        public async Task<bool> ExisteTransferenciaAsync(
            string contaOrigemId,
            string contaDestinoId,
            decimal valor,
            DateTime dataTransferencia)
        {
            var dataStr = dataTransferencia.ToString("yyyy-MM-dd HH:mm:ss.fff");

            return await _context.Transferencias
                .AnyAsync(t => t.IdContaCorrenteOrigem == contaOrigemId &&
                              t.IdContaCorrenteDestino == contaDestinoId &&
                              t.Valor == valor &&
                              t.DataMovimento == dataStr);
        }

        public async Task<Transferencia> ProcessarTransferenciaAsync(
            string contaOrigemId,
            string contaDestinoId,
            decimal valor)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Criar registro de transferência
                var transferencia = new Transferencia
                {
                    IdTransferencia = Guid.NewGuid().ToString(),
                    IdContaCorrenteOrigem = contaOrigemId,
                    IdContaCorrenteDestino = contaDestinoId,
                    DataMovimento = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    Valor = valor
                };

                await AddAsync(transferencia);

                // Criar movimentos correspondentes
                var movimentoDebito = new Movimento
                {
                    IdMovimento = Guid.NewGuid().ToString(),
                    IdContaCorrente = contaOrigemId,
                    DataMovimento = transferencia.DataMovimento,
                    TipoMovimento = "D",
                    Valor = valor
                };

                var movimentoCredito = new Movimento
                {
                    IdMovimento = Guid.NewGuid().ToString(),
                    IdContaCorrente = contaDestinoId,
                    DataMovimento = transferencia.DataMovimento,
                    TipoMovimento = "C",
                    Valor = valor
                };

                await _context.Movimentos.AddAsync(movimentoDebito);
                await _context.Movimentos.AddAsync(movimentoCredito);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return transferencia;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    // Interface para o repositório
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
