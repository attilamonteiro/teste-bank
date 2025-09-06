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
            services.AddScoped<IContaCorrenteRepository, ContaCorrenteRepository>();
            services.AddScoped<IMovimentoRepository, MovimentoRepository>();
            services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();
            services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();
            services.AddScoped<ITarifaRepository, TarifaRepository>();

            // Manter repositórios legacy para compatibilidade
            services.AddScoped<IAccountRepository, AccountRepository>();

            services.AddAutoMapper(typeof(DependencyConfiguration).Assembly);
        }
    }
}
