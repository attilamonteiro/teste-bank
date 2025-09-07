using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EstudoApi.Banking.Tests.Utils
{
    /// <summary>
    /// Authentication handler para testes que simula autenticação JWT
    /// </summary>
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Verificar se o teste está enviando um token Authorization
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                // Sem header Authorization = não autenticado
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            
            // Se é o esquema de teste simples "Test" = válido
            if (authHeader == "Test")
            {
                // Token válido - criar claims de usuário de teste
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, "test-user"),
                    new Claim("accountId", "123456"), // Account ID do usuário de teste
                    new Claim("userId", "user-test-123")
                };

                var identity = new ClaimsIdentity(claims, "Test");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "Test");

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            
            // Se não tem Bearer token ou é "invalid"
            if (string.IsNullOrEmpty(authHeader) || 
                !authHeader.StartsWith("Bearer ") ||
                authHeader.Contains("invalid"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
            }

            // Token Bearer válido - criar claims de usuário de teste
            var claimsBearer = new[]
            {
                new Claim(ClaimTypes.Name, "test-user"),
                new Claim("accountId", "123456"), // Account ID do usuário de teste
                new Claim("userId", "user-test-123")
            };

            var identityBearer = new ClaimsIdentity(claimsBearer, "Test");
            var principalBearer = new ClaimsPrincipal(identityBearer);
            var ticketBearer = new AuthenticationTicket(principalBearer, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticketBearer));
        }
    }
}
