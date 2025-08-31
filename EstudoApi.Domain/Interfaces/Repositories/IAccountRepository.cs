using System.Collections.Generic;
using System.Threading.Tasks;
using EstudoApi.Domain.Models;

namespace EstudoApi.Interfaces.Repositories
{
    public interface IAccountRepository
    {
        Task<Account?> GetAccount(int id);
        Task<Account?> GetAccountByCpf(string cpf);
        Task<IEnumerable<Account>> GetAccountsByUser(int userId);
        Task AddAccount(Account account);
        Task UpdateAccount(Account account);
    }
}
