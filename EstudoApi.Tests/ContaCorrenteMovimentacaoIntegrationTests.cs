using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EstudoApi.Domain.CQRS.Commands.Account;
using System.Net;

namespace EstudoApi.Tests
{
    public class ContaCorrenteMovimentacaoIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ContaCorrenteMovimentacaoIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Movimentar_Returns_401_If_Unauthorized()
        {
            // Arrange
            var command = new AccountMovementCommand
            {
                RequisicaoId = "req-test-001",
                NumeroConta = 1,
                Valor = 100.00m,
                Tipo = "C"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/movimentar", command);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Movimentar_Returns_401_If_Invalid_Token()
        {
            // Arrange
            var command = new AccountMovementCommand
            {
                RequisicaoId = "req-test-002",
                NumeroConta = 1,
                Valor = 100.00m,
                Tipo = "C"
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/movimentar", command);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Movimentar_Credito_Returns_204_If_Authorized()
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

            var command = new AccountMovementCommand
            {
                RequisicaoId = "req-test-003",
                Valor = 150.00m,
                Tipo = "C" // Crédito
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/movimentar", command);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verificar se o saldo foi atualizado
            var saldoResp = await _client.GetAsync("/api/conta/saldo");
            saldoResp.EnsureSuccessStatusCode();
            var saldoJson = await saldoResp.Content.ReadFromJsonAsync<SaldoResponse>();
            Assert.NotNull(saldoJson);
            Assert.Equal("150.00", saldoJson.saldo);
        }

        [Fact]
        public async Task Movimentar_Debito_Returns_204_If_Has_Balance()
        {
            // Arrange: criar conta, obter token e fazer depósito inicial
            var cpf = "98765432109";
            var senha = "SenhaForte456";
            var createResp = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf, senha });
            createResp.EnsureSuccessStatusCode();

            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf, senha });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginJson);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);

            // Primeiro um crédito para ter saldo
            var creditCommand = new AccountMovementCommand
            {
                RequisicaoId = "req-test-004-credit",
                Valor = 200.00m,
                Tipo = "C"
            };
            var creditResp = await _client.PostAsJsonAsync("/api/conta/movimentar", creditCommand);
            creditResp.EnsureSuccessStatusCode();

            // Agora um débito
            var debitCommand = new AccountMovementCommand
            {
                RequisicaoId = "req-test-004-debit",
                Valor = 75.00m,
                Tipo = "D"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/movimentar", debitCommand);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verificar saldo final (200 - 75 = 125)
            var saldoResp = await _client.GetAsync("/api/conta/saldo");
            saldoResp.EnsureSuccessStatusCode();
            var saldoJson = await saldoResp.Content.ReadFromJsonAsync<SaldoResponse>();
            Assert.NotNull(saldoJson);
            Assert.Equal("125.00", saldoJson.saldo);
        }

        [Fact]
        public async Task Movimentar_Returns_400_If_Invalid_Value()
        {
            // Arrange
            var cpf = "11122233344";
            var senha = "SenhaForte789";
            var createResp = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf, senha });
            createResp.EnsureSuccessStatusCode();

            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf, senha });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginJson);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);

            var command = new AccountMovementCommand
            {
                RequisicaoId = "req-test-005",
                Valor = -50.00m, // Valor negativo
                Tipo = "C"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/movimentar", command);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorJson = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(errorJson);
            Assert.Equal("INVALID_VALUE", errorJson.tipo);
        }

        [Fact]
        public async Task Movimentar_Returns_400_If_Invalid_Type()
        {
            // Arrange
            var cpf = "55566677788";
            var senha = "SenhaForte999";
            var createResp = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf, senha });
            createResp.EnsureSuccessStatusCode();

            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf, senha });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginJson);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);

            var command = new AccountMovementCommand
            {
                RequisicaoId = "req-test-006",
                Valor = 100.00m,
                Tipo = "X" // Tipo inválido
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/movimentar", command);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorJson = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(errorJson);
            Assert.Equal("INVALID_TYPE", errorJson.tipo);
        }

        [Fact]
        public async Task Movimentar_Returns_400_If_Insufficient_Balance()
        {
            // Arrange
            var cpf = "99988877766";
            var senha = "SenhaForte111";
            var createResp = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf, senha });
            createResp.EnsureSuccessStatusCode();

            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf, senha });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginJson);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);

            var command = new AccountMovementCommand
            {
                RequisicaoId = "req-test-007",
                Valor = 500.00m, // Valor maior que o saldo (que é 0)
                Tipo = "D"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/movimentar", command);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorJson = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(errorJson);
            Assert.Equal("INVALID_VALUE", errorJson.tipo);
        }

        // DTOs para deserialização
        private class LoginResponse { public string token { get; set; } = string.Empty; }
        private class SaldoResponse
        {
            public int numeroConta { get; set; }
            public string nomeTitular { get; set; } = string.Empty;
            public string dataConsulta { get; set; } = string.Empty;
            public string saldo { get; set; } = string.Empty;
        }
        private class ErrorResponse
        {
            public string mensagem { get; set; } = string.Empty;
            public string tipo { get; set; } = string.Empty;
        }
    }
}
