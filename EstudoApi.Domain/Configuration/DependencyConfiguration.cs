using Microsoft.Extensions.DependencyInjection;

namespace EstudoApi.Domain.Configuration
{
    public static class DependencyConfiguration
    {
        public static void ConfigureDomainDependencies(this IServiceCollection services)
        {
            services.AddScoped<EstudoApi.Domain.Services.IAccountService, EstudoApi.Domain.Services.AccountService>();
        }
    }
}
