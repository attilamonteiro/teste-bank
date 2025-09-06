using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;

namespace EstudoApi.Tests
{
    public class ContaCorrenteInativacaoIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ContaCorrenteInativacaoIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Inativar_Returns_401_If_Unauthorized()
        {
            // Arrange
            var request = new { senha = "senhaQualquer" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/inativar", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Inativar_Returns_401_If_Invalid_Token()
        {
            // Arrange
            var request = new { senha = "senhaQualquer" };
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/inativar", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Inativar_Returns_204_If_Valid_Password()
        {
            // Arrange: criar conta e obter token
            var cpf = "12345678901";
            var senha = "SenhaForte123";
            var createResp = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf, senha });
            createResp.EnsureSuccessStatusCode();

            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf, senha });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginJson);
            Assert.False(string.IsNullOrWhiteSpace(loginJson.token));

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);

            var request = new { senha = senha }; // Mesma senha usada na criação

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/inativar", request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verificar se a conta foi realmente inativada tentando consultar saldo
            var saldoResp = await _client.GetAsync("/api/conta/saldo");
            Assert.Equal(HttpStatusCode.BadRequest, saldoResp.StatusCode);
            var saldoErrorJson = await saldoResp.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(saldoErrorJson);
            Assert.Equal("INACTIVE_ACCOUNT", saldoErrorJson.tipo);
        }

        [Fact]
        public async Task Inativar_Returns_400_If_Invalid_Password()
        {
            // Arrange: criar conta e obter token
            var cpf = "98765432109";
            var senha = "SenhaForte456";
            var createResp = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf, senha });
            createResp.EnsureSuccessStatusCode();

            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf, senha });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginJson);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);

            var request = new { senha = "SenhaErrada123" }; // Senha incorreta

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/inativar", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorJson = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(errorJson);
            Assert.Equal("USER_UNAUTHORIZED", errorJson.tipo);
            Assert.Contains("Senha inválida", errorJson.mensagem);
        }

        [Fact]
        public async Task Inativar_Returns_400_If_Account_Already_Inactive()
        {
            // Arrange: criar conta, obter token e inativar
            var cpf = "11122233344";
            var senha = "SenhaForte789";
            var createResp = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf, senha });
            createResp.EnsureSuccessStatusCode();

            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf, senha });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginJson);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);

            var request = new { senha = senha };

            // Primeira inativação (deve funcionar)
            var firstInactivateResp = await _client.PostAsJsonAsync("/api/conta/inativar", request);
            firstInactivateResp.EnsureSuccessStatusCode();

            // Act: tentar inativar novamente
            var response = await _client.PostAsJsonAsync("/api/conta/inativar", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorJson = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(errorJson);
            Assert.Equal("INACTIVE_ACCOUNT", errorJson.tipo);
            Assert.Contains("já está inativa", errorJson.mensagem);
        }

        [Fact]
        public async Task Inativar_Prevents_Future_Operations()
        {
            // Arrange: criar conta, inativar e tentar fazer movimentação
            var cpf = "55566677788";
            var senha = "SenhaForte999";
            var createResp = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf, senha });
            createResp.EnsureSuccessStatusCode();

            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf, senha });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginJson);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);

            // Inativar conta
            var inactivateRequest = new { senha = senha };
            var inactivateResp = await _client.PostAsJsonAsync("/api/conta/inativar", inactivateRequest);
            inactivateResp.EnsureSuccessStatusCode();

            // Act: tentar fazer uma movimentação após inativação
            var movementRequest = new
            {
                requisicaoId = "req-test-inactive",
                valor = 100.00m,
                tipo = "C"
            };
            var movementResp = await _client.PostAsJsonAsync("/api/conta/movimentar", movementRequest);

            // Assert: movimentação deve ser rejeitada
            Assert.Equal(HttpStatusCode.BadRequest, movementResp.StatusCode);
            var errorJson = await movementResp.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(errorJson);
            Assert.Equal("INACTIVE_ACCOUNT", errorJson.tipo);
        }

        // DTOs para deserialização
        private class LoginResponse { public string token { get; set; } = string.Empty; }
        private class ErrorResponse
        {
            public string mensagem { get; set; } = string.Empty;
            public string tipo { get; set; } = string.Empty;
        }
    }
}
