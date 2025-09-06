using EstudoApi.Domain.Models;
using EstudoApi.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EstudoApi.Infrastructure.Repositories
{
    public class MovimentoRepository : IMovimentoRepository
    {
        private readonly AppDbContext _context;

        public MovimentoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Movimento> AddAsync(Movimento movimento)
        {
            await _context.Movimentos.AddAsync(movimento);
            await _context.SaveChangesAsync();
            return movimento;
        }

        public async Task<IEnumerable<Movimento>> GetByContaAsync(string contaCorrenteId)
        {
            return await _context.Movimentos
                .Where(m => m.IdContaCorrente == contaCorrenteId)
                .Include(m => m.ContaCorrente)
                .OrderByDescending(m => m.DataMovimento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movimento>> GetByContaAndPeriodoAsync(
            string contaCorrenteId,
            DateTime dataInicio,
            DateTime dataFim)
        {
            // Como DataMovimento é string no SQLite, vamos fazer a conversão
            var dataInicioStr = dataInicio.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var dataFimStr = dataFim.ToString("yyyy-MM-dd HH:mm:ss.fff");

            return await _context.Movimentos
                .Where(m => m.IdContaCorrente == contaCorrenteId &&
                           string.Compare(m.DataMovimento, dataInicioStr) >= 0 &&
                           string.Compare(m.DataMovimento, dataFimStr) <= 0)
                .Include(m => m.ContaCorrente)
                .OrderByDescending(m => m.DataMovimento)
                .ToListAsync();
        }

        public async Task<decimal> GetSaldoAtualAsync(string contaCorrenteId)
        {
            var movimentos = await _context.Movimentos
                .Where(m => m.IdContaCorrente == contaCorrenteId)
                .ToListAsync();

            var creditos = movimentos.Where(m => m.TipoMovimento == "C").Sum(m => m.Valor);
            var debitos = movimentos.Where(m => m.TipoMovimento == "D").Sum(m => m.Valor);

            return creditos - debitos;
        }

        public async Task<decimal> GetSaldoEmDataAsync(string contaCorrenteId, DateTime data)
        {
            var dataStr = data.ToString("yyyy-MM-dd HH:mm:ss.fff");

            var movimentos = await _context.Movimentos
                .Where(m => m.IdContaCorrente == contaCorrenteId &&
                           string.Compare(m.DataMovimento, dataStr) <= 0)
                .ToListAsync();

            var creditos = movimentos.Where(m => m.TipoMovimento == "C").Sum(m => m.Valor);
            var debitos = movimentos.Where(m => m.TipoMovimento == "D").Sum(m => m.Valor);

            return creditos - debitos;
        }

        public async Task<IEnumerable<Movimento>> GetUltimosMovimentosAsync(string contaCorrenteId, int quantidade = 10)
        {
            return await _context.Movimentos
                .Where(m => m.IdContaCorrente == contaCorrenteId)
                .Include(m => m.ContaCorrente)
                .OrderByDescending(m => m.DataMovimento)
                .Take(quantidade)
                .ToListAsync();
        }

        public async Task<Movimento?> GetByIdAsync(string id)
        {
            return await _context.Movimentos
                .Include(m => m.ContaCorrente)
                .FirstOrDefaultAsync(m => m.IdMovimento == id);
        }

        public async Task AddCreditoAsync(string contaCorrenteId, decimal valor, string? descricao = null)
        {
            var movimento = new Movimento
            {
                IdMovimento = Guid.NewGuid().ToString(),
                IdContaCorrente = contaCorrenteId,
                DataMovimento = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                TipoMovimento = "C",
                Valor = valor
            };

            await AddAsync(movimento);
        }

        public async Task AddDebitoAsync(string contaCorrenteId, decimal valor, string? descricao = null)
        {
            var movimento = new Movimento
            {
                IdMovimento = Guid.NewGuid().ToString(),
                IdContaCorrente = contaCorrenteId,
                DataMovimento = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                TipoMovimento = "D",
                Valor = valor
            };

            await AddAsync(movimento);
        }
    }

    // Interface para o repositório
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
