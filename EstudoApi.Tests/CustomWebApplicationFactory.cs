using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EstudoApi.Infrastructure.Contexts;

namespace EstudoApi.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove autenticação real (JWT)
            var authDescriptor = services.SingleOrDefault(d => d.ServiceType.FullName == "Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider");
            if (authDescriptor != null)
                services.Remove(authDescriptor);

            // Adiciona autenticação fake
            services.AddAuthentication("Test")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, EstudoApi.Tests.Utils.TestAuthHandler>("Test", options => { });

            // Remove todos os contextos anteriores
            var dbContextTypes = services.Where(d => d.ServiceType.Name.Contains("DbContextOptions")).ToList();
            foreach (var d in dbContextTypes) services.Remove(d);

            // Usa SQLite in-memory compartilhado para AppDbContext (que já herda de IdentityDbContext<AppUser>)
            var dbName = $"TestDb_{Guid.NewGuid()}";
            var connectionString = $"DataSource={dbName};Mode=Memory;Cache=Shared";
            var keepAliveConnection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            keepAliveConnection.Open();
            services.AddSingleton(keepAliveConnection);
            services.AddDbContext<AppDbContext>((sp, options) =>
                options.UseSqlite(keepAliveConnection), ServiceLifetime.Singleton);

            // Garante que o banco está criado e faz migrations do Identity
            var spProvider = services.BuildServiceProvider();
            using var scope = spProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            try
            {
                db.Database.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TEST DB ERROR] Migration: {ex.Message}\n{ex.StackTrace}");
            }
            Console.WriteLine("[TEST DB] Banco de dados em memória inicializado e pronto para uso.");
        });
    }
}
