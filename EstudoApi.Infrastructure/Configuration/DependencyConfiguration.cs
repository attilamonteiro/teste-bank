using Microsoft.Extensions.DependencyInjection;
using EstudoApi.Infrastructure.Repositories;
using EstudoApi.Interfaces.Repositories;

namespace EstudoApi.Infrastructure.Configuration
{
    public static class DependencyConfiguration
    {
        public static void ConfigureInfrastructureDependencies(this IServiceCollection services)
        {
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddAutoMapper(typeof(DependencyConfiguration).Assembly);
        }
    }
}
