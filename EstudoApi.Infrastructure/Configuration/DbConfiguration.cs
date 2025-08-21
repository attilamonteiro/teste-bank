using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EstudoApi.Infrastructure.Contexts;

namespace EstudoApi.Infrastructure.Configuration
{
    public static class DbConfiguration
    {
        public static void AddConnections(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"),
                                   b => b.MigrationsAssembly("EstudoApi.Infrastructure"))
                       .EnableSensitiveDataLogging()
                       .LogTo(Console.WriteLine, LogLevel.Information));
        }
    }

}
