using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EstudoApi.Banking.Tests.Utils
{
    public class BankingWebApplicationFactory : WebApplicationFactory<EstudoApi.Banking.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove autenticação real (JWT) para testes
                services.RemoveAll<IAuthenticationSchemeProvider>();
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                // Configurar banco em memória para testes
                // (se necessário para testes específicos)
            });

            builder.UseEnvironment("Testing");
        }
    }
}


















