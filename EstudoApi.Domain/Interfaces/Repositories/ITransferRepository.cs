using EstudoApi.Domain.Models;

namespace EstudoApi.Interfaces.Repositories
{
    public interface ITransferRepository
    {
        Task<Transfer?> GetTransferByRequisicaoId(string requisicaoId);
        Task AddTransfer(Transfer transfer);
        Task UpdateTransfer(Transfer transfer);
        Task<IEnumerable<Transfer>> GetTransfersByAccount(int accountId);
    }
}
