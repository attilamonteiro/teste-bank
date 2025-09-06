using Microsoft.Extensions.DependencyInjection;
using EstudoApi.Banking.Transfer.Services;
using EstudoApi.Infrastructure.Repositories;
using MediatR;

namespace EstudoApi.Banking.Configuration
{
    /// <summary>
    /// Configuração de Dependency Injection para EstudoApi.Banking
    /// </summary>
    public static class BankingDependencyConfiguration
    {
        public static IServiceCollection ConfigureBankingServices(this IServiceCollection services)
        {
            // Registrar MediatR para versão 11.1.0
            services.AddMediatR(typeof(BankingDependencyConfiguration).Assembly);

            // Registrar repositórios SQLite da Ana
            services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();
            services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

            // Registrar HttpClient primeiro
            services.AddHttpClient<ContaCorrenteApiService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "EstudoApi.Banking/1.0");
            });

            // Registrar serviços de transferência
            services.AddScoped<IContaCorrenteApiService, ContaCorrenteApiService>();

            // Registrar HttpContextAccessor para extrair token
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
