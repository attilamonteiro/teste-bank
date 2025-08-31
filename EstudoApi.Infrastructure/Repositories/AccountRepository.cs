using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EstudoApi.Domain.Models;
using EstudoApi.Interfaces.Repositories;
using EstudoApi.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EstudoApi.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;
        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetAccount(int id)
        {
            return await _context.Set<Account>().Include(a => a.Transactions).FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Account?> GetAccountByCpf(string cpf)
        {
            return await _context.Set<Account>().Include(a => a.Transactions).FirstOrDefaultAsync(a => a.Cpf == cpf);
        }

        public async Task<IEnumerable<Account>> GetAccountsByUser(int userId)
        {
            // Supondo que Account tenha um campo UserId
            return await _context.Set<Account>().Where(a => EF.Property<int>(a, "UserId") == userId).ToListAsync();
        }

        public async Task AddAccount(Account account)
        {
            _context.Set<Account>().Add(account);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAccount(Account account)
        {
            _context.Set<Account>().Update(account);
            await _context.SaveChangesAsync();
        }
    }
}
