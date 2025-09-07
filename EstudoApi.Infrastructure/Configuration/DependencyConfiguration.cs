using Microsoft.Extensions.DependencyInjection;
using EstudoApi.Infrastructure.Repositories;
using EstudoApi.Interfaces.Repositories;

namespace EstudoApi.Infrastructure.Configuration
{
    public static class DependencyConfiguration
    {
        public static void ConfigureInfrastructureDependencies(this IServiceCollection services)
        {
            // Repositórios do esquema SQLite da Ana
            services.AddScoped<EstudoApi.Infrastructure.Repositories.IContaCorrenteRepository, ContaCorrenteRepository>();
            services.AddScoped<EstudoApi.Domain.Interfaces.Repositories.IContaCorrenteRepository, ContaCorrenteRepository>();
            services.AddScoped<EstudoApi.Infrastructure.Repositories.IMovimentoRepository, MovimentoRepository>();
            services.AddScoped<EstudoApi.Infrastructure.Repositories.ITransferenciaRepository, TransferenciaRepository>();
            services.AddScoped<EstudoApi.Infrastructure.Repositories.IIdempotenciaRepository, IdempotenciaRepository>();
            services.AddScoped<EstudoApi.Infrastructure.Repositories.ITarifaRepository, TarifaRepository>();

            // Manter repositórios legacy para compatibilidade
            services.AddScoped<IAccountRepository, AccountRepository>();

            services.AddAutoMapper(typeof(DependencyConfiguration).Assembly);
        }
    }
}
