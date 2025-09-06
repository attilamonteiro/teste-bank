using EstudoApi.Domain.Models;
using EstudoApi.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EstudoApi.Infrastructure.Repositories
{
    public class ContaCorrenteRepository : IContaCorrenteRepository
    {
        private readonly AppDbContext _context;

        public ContaCorrenteRepository(AppDbContext context)
        {
            _context = context;
        }

        // Métodos para ContaCorrente
        public async Task<ContaCorrente?> GetByNumeroAsync(int numero)
        {
            return await _context.ContasCorrentes
                .Include(cc => cc.Movimentos)
                .FirstOrDefaultAsync(cc => cc.Numero == numero && cc.Ativo == 1);
        }

        public async Task<ContaCorrente?> GetByIdAsync(string id)
        {
            return await _context.ContasCorrentes
                .Include(cc => cc.Movimentos)
                .FirstOrDefaultAsync(cc => cc.IdContaCorrente == id && cc.Ativo == 1);
        }

        public async Task<decimal> GetSaldoAsync(string contaCorrenteId)
        {
            var movimentos = await _context.Movimentos
                .Where(m => m.IdContaCorrente == contaCorrenteId)
                .ToListAsync();

            var creditos = movimentos.Where(m => m.TipoMovimento == "C").Sum(m => m.Valor);
            var debitos = movimentos.Where(m => m.TipoMovimento == "D").Sum(m => m.Valor);

            return creditos - debitos;
        }

        public async Task<IEnumerable<ContaCorrente>> GetAllAsync()
        {
            return await _context.ContasCorrentes
                .Where(cc => cc.Ativo == 1)
                .Include(cc => cc.Movimentos)
                .ToListAsync();
        }

        public async Task<ContaCorrente> AddAsync(ContaCorrente conta)
        {
            await _context.ContasCorrentes.AddAsync(conta);
            await _context.SaveChangesAsync();
            return conta;
        }

        public async Task UpdateAsync(ContaCorrente conta)
        {
            _context.ContasCorrentes.Update(conta);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var conta = await GetByIdAsync(id);
            if (conta != null)
            {
                conta.Ativo = 0; // Soft delete
                await UpdateAsync(conta);
            }
        }

        public async Task<bool> ExisteContaAsync(int numero)
        {
            return await _context.ContasCorrentes
                .AnyAsync(cc => cc.Numero == numero && cc.Ativo == 1);
        }

        public async Task<bool> ValidarSenhaAsync(int numero, string senha)
        {
            var conta = await GetByNumeroAsync(numero);
            if (conta == null) return false;

            // Aqui você implementaria a validação de hash da senha
            // Por exemplo, usando BCrypt:
            // return BCrypt.Net.BCrypt.Verify(senha, conta.Senha);

            // Por enquanto, comparação simples (NÃO usar em produção)
            return conta.Senha == senha;
        }
    }

    // Interface para o repositório
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
    }
}
