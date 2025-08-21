using Microsoft.Extensions.DependencyInjection;

namespace EstudoApi.Domain.Configuration
{
    public static class AutoMapperConfiguration
    {
        public static void ConfigureAutoMapperependencies(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DependencyConfiguration).Assembly);
        }
    }
}
