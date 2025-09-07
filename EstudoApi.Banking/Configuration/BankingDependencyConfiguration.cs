using Microsoft.Extensions.DependencyInjection;
using EstudoApi.Banking.Transfer.Services;
using EstudoApi.Infrastructure.Repositories;
using MediatR;
using EstudoApi.Banking.Transfer.Commands;

namespace EstudoApi.Banking.Configuration
{
    /// <summary>
    /// Configuração de Dependency Injection para EstudoApi.Banking
    /// </summary>
    public static class BankingDependencyConfiguration
    {
        public static IServiceCollection ConfigureBankingServices(this IServiceCollection services)
        {
            Console.WriteLine("[Banking DI] Iniciando configuração...");

            // Registro simples do MediatR (assembly atual)
            services.AddMediatR(typeof(BankingDependencyConfiguration).Assembly);

            // Registro explícito do handler principal (garantia) via reflexão para evitar problemas de namespace
            var assembly = typeof(BankingDependencyConfiguration).Assembly;
            var all = assembly.GetTypes();
            Console.WriteLine($"[Banking DI] Total tipos assembly: {all.Length}");
            foreach (var t in all.Where(t=> t.FullName!=null && t.FullName.Contains("Transfer")).OrderBy(t=>t.FullName))
            {
                Console.WriteLine($"  [TYPE] {t.FullName}");
            }
            var cmdType = assembly.GetType("EstudoApi.Banking.Transfer.Commands.TransferCommand");
            var resType = assembly.GetType("EstudoApi.Banking.Transfer.Commands.TransferResult");
            var primaryHandlerType = assembly.GetType("EstudoApi.Banking.Transfer.Handlers.TransferCommandHandler");
            var fallbackHandlerType = assembly.GetType("EstudoApi.Banking.Transfer.Handlers.BankingTransferCommandHandler");
            Console.WriteLine($"[Banking DI] Tipos: cmd:{cmdType!=null} res:{resType!=null} primary:{primaryHandlerType!=null} fallback:{fallbackHandlerType!=null}");

            if (cmdType != null && resType != null)
            {
                var handlerInterface = typeof(IRequestHandler<,>).MakeGenericType(cmdType, resType);
                if (primaryHandlerType != null)
                {
                    services.AddScoped(handlerInterface, primaryHandlerType);
                    Console.WriteLine("[Banking DI] Primary TransferCommandHandler registrado.");
                }
                else if (fallbackHandlerType != null)
                {
                    services.AddScoped(handlerInterface, fallbackHandlerType);
                    Console.WriteLine("[Banking DI][WARN] Fallback BankingTransferCommandHandler registrado (primary ausente).");
                }
                else
                {
                    Console.WriteLine("[Banking DI][ERROR] Nenhum handler disponível para TransferCommand.");
                }
            }
            else
            {
                Console.WriteLine("[Banking DI][ERROR] Command ou Result type não localizado.");
            }

            Console.WriteLine("[Banking DI] Configuração concluída.");

            // Registrar repositórios SQLite da Ana
            services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();
            services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

            // Registrar HttpClient e serviços de transferência
            services.AddHttpClient<EstudoApi.Banking.Transfer.Services.ITransferContaCorrenteApiService, EstudoApi.Banking.Transfer.Services.TransferContaCorrenteApiService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5041");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "EstudoApi.Banking/1.0");
            });

            // Registrar HttpContextAccessor para extrair token
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
