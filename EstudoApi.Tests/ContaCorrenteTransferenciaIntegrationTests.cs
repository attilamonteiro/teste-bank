using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EstudoApi.Domain.CQRS.Commands.Account;
using Xunit;

namespace EstudoApi.Tests
{
    public class ContaCorrenteTransferenciaIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ContaCorrenteTransferenciaIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Transferir_Returns_401_If_Unauthorized()
        {
            // Arrange
            var command = new TransferCommand
            {
                RequisicaoId = "req-transfer-001",
                ContaOrigem = 1,
                ContaDestino = 2,
                Valor = 100.00m,
                Descricao = "Teste transferência"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/transferir", command);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Transferir_Returns_403_If_Not_Own_Account()
        {
            // Arrange - Criar duas contas
            var cpf1 = "11122233344";
            var senha1 = "SenhaForte111";
            var createResp1 = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf = cpf1, senha = senha1 });
            createResp1.EnsureSuccessStatusCode();
            var createJson1 = await createResp1.Content.ReadFromJsonAsync<CreateAccountResponse>();

            var cpf2 = "55566677788";
            var senha2 = "SenhaForte222";
            var createResp2 = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf = cpf2, senha = senha2 });
            createResp2.EnsureSuccessStatusCode();
            var createJson2 = await createResp2.Content.ReadFromJsonAsync<CreateAccountResponse>();

            // Login com a primeira conta
            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf = cpf1, senha = senha1 });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginJson);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);

            // Tentar transferir da conta 2 (não é do usuário logado)
            var command = new TransferCommand
            {
                RequisicaoId = "req-transfer-002",
                ContaOrigem = createJson2!.numeroConta,
                ContaDestino = createJson1!.numeroConta,
                Valor = 50.00m,
                Descricao = "Tentativa transferência conta alheia"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/transferir", command);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var errorJson = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(errorJson);
            Assert.Equal("UNAUTHORIZED_ORIGIN_ACCOUNT", errorJson.tipo);
        }

        [Fact]
        public async Task Transferir_Returns_400_If_Same_Account()
        {
            // Arrange
            var cpf = "99988877766";
            var senha = "SenhaForte999";
            var createResp = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf, senha });
            createResp.EnsureSuccessStatusCode();
            var createJson = await createResp.Content.ReadFromJsonAsync<CreateAccountResponse>();

            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf, senha });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginJson);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);

            // Fazer um depósito inicial
            var depositCommand = new AccountMovementCommand
            {
                RequisicaoId = "req-deposit-003",
                NumeroConta = createJson!.numeroConta,
                Valor = 1000.00m,
                Tipo = "C"
            };
            var depositResp = await _client.PostAsJsonAsync("/api/conta/movimentar", depositCommand);
            depositResp.EnsureSuccessStatusCode();

            var command = new TransferCommand
            {
                RequisicaoId = "req-transfer-003",
                ContaOrigem = createJson.numeroConta,
                ContaDestino = createJson.numeroConta, // Mesma conta
                Valor = 100.00m,
                Descricao = "Transferência mesma conta"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/transferir", command);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorJson = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(errorJson);
            Assert.Equal("SAME_ACCOUNT", errorJson.tipo);
        }

        [Fact]
        public async Task Transferir_Returns_204_If_Successful()
        {
            // Arrange - Criar duas contas
            var cpf1 = "12345678901";
            var senha1 = "SenhaForte123";
            var createResp1 = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf = cpf1, senha = senha1 });
            createResp1.EnsureSuccessStatusCode();
            var createJson1 = await createResp1.Content.ReadFromJsonAsync<CreateAccountResponse>();

            var cpf2 = "10987654321";
            var senha2 = "SenhaForte456";
            var createResp2 = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf = cpf2, senha = senha2 });
            createResp2.EnsureSuccessStatusCode();
            var createJson2 = await createResp2.Content.ReadFromJsonAsync<CreateAccountResponse>();

            // Login com a primeira conta
            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf = cpf1, senha = senha1 });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginJson);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);

            // Fazer um depósito inicial na conta 1
            var depositCommand = new AccountMovementCommand
            {
                RequisicaoId = "req-deposit-004",
                NumeroConta = createJson1!.numeroConta,
                Valor = 500.00m,
                Tipo = "C"
            };
            var depositResp = await _client.PostAsJsonAsync("/api/conta/movimentar", depositCommand);
            depositResp.EnsureSuccessStatusCode();

            // Transferir da conta 1 para conta 2
            var command = new TransferCommand
            {
                RequisicaoId = "req-transfer-004",
                ContaOrigem = createJson1.numeroConta,
                ContaDestino = createJson2!.numeroConta,
                Valor = 150.00m,
                Descricao = "Transferência de teste"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/transferir", command);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verificar saldos
            var saldoResp1 = await _client.GetAsync("/api/conta/saldo");
            saldoResp1.EnsureSuccessStatusCode();
            var saldoJson1 = await saldoResp1.Content.ReadFromJsonAsync<SaldoResponse>();
            Assert.NotNull(saldoJson1);
            Assert.Equal("350.00", saldoJson1.saldo); // 500 - 150

            // Login com a segunda conta para verificar saldo
            _client.DefaultRequestHeaders.Authorization = null;
            var loginResp2 = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf = cpf2, senha = senha2 });
            loginResp2.EnsureSuccessStatusCode();
            var loginJson2 = await loginResp2.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(loginJson2);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson2.token);

            var saldoResp2 = await _client.GetAsync("/api/conta/saldo");
            saldoResp2.EnsureSuccessStatusCode();
            var saldoJson2 = await saldoResp2.Content.ReadFromJsonAsync<SaldoResponse>();
            Assert.NotNull(saldoJson2);
            Assert.Equal("150.00", saldoJson2.saldo); // 0 + 150
        }

        [Fact]
        public async Task Transferir_Returns_400_If_Insufficient_Balance()
        {
            // Arrange - Criar duas contas
            var cpf1 = "33344455566";
            var senha1 = "SenhaForte333";
            var createResp1 = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf = cpf1, senha = senha1 });
            createResp1.EnsureSuccessStatusCode();
            var createJson1 = await createResp1.Content.ReadFromJsonAsync<CreateAccountResponse>();

            var cpf2 = "66677788899";
            var senha2 = "SenhaForte666";
            var createResp2 = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf = cpf2, senha = senha2 });
            createResp2.EnsureSuccessStatusCode();
            var createJson2 = await createResp2.Content.ReadFromJsonAsync<CreateAccountResponse>();

            // Login com a primeira conta
            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf = cpf1, senha = senha1 });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginJson);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);

            // Tentar transferir sem saldo suficiente
            var command = new TransferCommand
            {
                RequisicaoId = "req-transfer-005",
                ContaOrigem = createJson1!.numeroConta,
                ContaDestino = createJson2!.numeroConta,
                Valor = 100.00m,
                Descricao = "Transferência sem saldo"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/conta/transferir", command);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorJson = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(errorJson);
            Assert.Equal("INSUFFICIENT_BALANCE", errorJson.tipo);
        }

        // DTOs para deserialização
        private class CreateAccountResponse
        {
            public int numeroConta { get; set; }
        }

        private class LoginResponse
        {
            public string token { get; set; } = string.Empty;
        }

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
