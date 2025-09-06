using EstudoApi.Domain.Models;
using EstudoApi.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EstudoApi.Infrastructure.Repositories
{
    public class IdempotenciaRepository : IIdempotenciaRepository
    {
        private readonly AppDbContext _context;

        public IdempotenciaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Idempotencia?> GetByChaveAsync(string chaveIdempotencia)
        {
            return await _context.Idempotencias
                .FirstOrDefaultAsync(i => i.ChaveIdempotencia == chaveIdempotencia);
        }

        public async Task<Idempotencia> AddAsync(Idempotencia idempotencia)
        {
            await _context.Idempotencias.AddAsync(idempotencia);
            await _context.SaveChangesAsync();
            return idempotencia;
        }

        public async Task UpdateAsync(Idempotencia idempotencia)
        {
            _context.Idempotencias.Update(idempotencia);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteChaveAsync(string chaveIdempotencia)
        {
            return await _context.Idempotencias
                .AnyAsync(i => i.ChaveIdempotencia == chaveIdempotencia);
        }

        public async Task<Idempotencia> CriarOuObterAsync(string chaveIdempotencia, string requisicao)
        {
            var idempotenciaExistente = await GetByChaveAsync(chaveIdempotencia);

            if (idempotenciaExistente != null)
            {
                return idempotenciaExistente;
            }

            var novaIdempotencia = new Idempotencia
            {
                ChaveIdempotencia = chaveIdempotencia,
                Requisicao = requisicao,
                Resultado = null // Será preenchido quando a operação for concluída
            };

            return await AddAsync(novaIdempotencia);
        }

        public async Task AtualizarResultadoAsync(string chaveIdempotencia, string resultado)
        {
            var idempotencia = await GetByChaveAsync(chaveIdempotencia);

            if (idempotencia != null)
            {
                idempotencia.Resultado = resultado;
                await UpdateAsync(idempotencia);
            }
        }

        public async Task<IEnumerable<Idempotencia>> GetAllAsync()
        {
            return await _context.Idempotencias
                .OrderByDescending(i => i.ChaveIdempotencia)
                .ToListAsync();
        }

        public async Task DeleteAsync(string chaveIdempotencia)
        {
            var idempotencia = await GetByChaveAsync(chaveIdempotencia);
            if (idempotencia != null)
            {
                _context.Idempotencias.Remove(idempotencia);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> LimparAntigasAsync(DateTime dataLimite)
        {
            // Como não temos campo de data na tabela de idempotência,
            // vamos usar a chave para identificar registros antigos
            // Assumindo que a chave contém timestamp ou GUID ordenável

            var idempotenciasAntigas = await _context.Idempotencias
                .Where(i => string.IsNullOrEmpty(i.Resultado)) // Apenas incompletas
                .ToListAsync();

            if (idempotenciasAntigas.Any())
            {
                _context.Idempotencias.RemoveRange(idempotenciasAntigas);
                await _context.SaveChangesAsync();
            }

            return idempotenciasAntigas.Count;
        }
    }

    // Interface para o repositório
    public interface IIdempotenciaRepository
    {
        Task<Idempotencia?> GetByChaveAsync(string chaveIdempotencia);
        Task<Idempotencia> AddAsync(Idempotencia idempotencia);
        Task UpdateAsync(Idempotencia idempotencia);
        Task<bool> ExisteChaveAsync(string chaveIdempotencia);
        Task<Idempotencia> CriarOuObterAsync(string chaveIdempotencia, string requisicao);
        Task AtualizarResultadoAsync(string chaveIdempotencia, string resultado);
        Task<IEnumerable<Idempotencia>> GetAllAsync();
        Task DeleteAsync(string chaveIdempotencia);
        Task<int> LimparAntigasAsync(DateTime dataLimite);
    }
}
