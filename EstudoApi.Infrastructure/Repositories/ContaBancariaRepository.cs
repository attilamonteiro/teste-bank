using EstudoApi.Domain.Models;
using EstudoApi.Domain.Interfaces.Repositories;
using EstudoApi.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EstudoApi.Infrastructure.Repositories
{
    public class ContaBancariaRepository : IContaBancariaRepository
    {
        private readonly AppDbContext _context;

        public ContaBancariaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContaBancaria?> GetByIdAsync(string id)
        {
            return await _context.ContasBancarias
                .Include(cb => cb.Movimentos)
                .FirstOrDefaultAsync(cb => cb.Id == id && cb.Ativo);
        }

        public async Task<ContaBancaria?> GetByNumeroAsync(int numero)
        {
            return await _context.ContasBancarias
                .Include(cb => cb.Movimentos)
                .FirstOrDefaultAsync(cb => cb.Numero == numero && cb.Ativo);
        }

        public async Task<ContaBancaria?> GetByCpfAsync(string cpf)
        {
            return await _context.ContasBancarias
                .Include(cb => cb.Movimentos)
                .FirstOrDefaultAsync(cb => cb.Cpf == cpf && cb.Ativo);
        }

        public async Task<IEnumerable<ContaBancaria>> GetAllAsync()
        {
            return await _context.ContasBancarias
                .Where(cb => cb.Ativo)
                .Include(cb => cb.Movimentos)
                .ToListAsync();
        }

        public async Task<ContaBancaria> AddAsync(ContaBancaria conta)
        {
            await _context.ContasBancarias.AddAsync(conta);
            await _context.SaveChangesAsync();
            return conta;
        }

        public async Task UpdateAsync(ContaBancaria conta)
        {
            _context.ContasBancarias.Update(conta);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var conta = await GetByIdAsync(id);
            if (conta != null)
            {
                conta.Inativar(); // Soft delete
                await UpdateAsync(conta);
            }
        }

        public async Task<bool> ExisteContaAsync(int numero)
        {
            return await _context.ContasBancarias
                .AnyAsync(cb => cb.Numero == numero && cb.Ativo);
        }

        public async Task<bool> ExisteCpfAsync(string cpf)
        {
            return await _context.ContasBancarias
                .AnyAsync(cb => cb.Cpf == cpf && cb.Ativo);
        }

        public async Task<bool> ValidarSenhaAsync(int numero, string senha)
        {
            var conta = await GetByNumeroAsync(numero);
            return conta?.ValidarSenha(senha) == true;
        }

        public async Task<decimal> GetSaldoAsync(string contaId)
        {
            var conta = await GetByIdAsync(contaId);
            return conta?.CalcularSaldo() ?? 0;
        }

        public async Task<Movimento> AddMovimentoAsync(Movimento movimento)
        {
            await _context.Movimentos.AddAsync(movimento);
            await _context.SaveChangesAsync();
            return movimento;
        }

        public async Task<IEnumerable<Movimento>> GetMovimentosAsync(string contaId)
        {
            return await _context.Movimentos
                .Where(m => m.IdContaCorrente == contaId)
                .OrderByDescending(m => m.DataMovimento)
                .ToListAsync();
        }
    }
}
