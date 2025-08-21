using Microsoft.Extensions.DependencyInjection;
using EstudoApi.Services;

namespace EstudoApi.Domain.Configuration
{
    public static class DependencyConfiguration
    {
        public static void ConfigureDomainDependencies(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductsService>();
        }
    }
}
