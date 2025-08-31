using Microsoft.Extensions.DependencyInjection;
using EstudoApi.Infrastructure.Repositories;
using EstudoApi.Interfaces.Repositories;

namespace EstudoApi.Infrastructure.Configuration
{
    public static class DependencyConfiguration
    {
        public static void ConfigureInfrastructureDependencies(this IServiceCollection services)
        {
            // Removed ProductRepository registration
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddAutoMapper(typeof(DependencyConfiguration).Assembly);
        }
    }
}
