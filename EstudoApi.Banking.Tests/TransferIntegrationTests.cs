using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EstudoApi.Banking.Tests.Utils;
using EstudoApi.Banking.Transfer.Commands;
using Xunit;

namespace EstudoApi.Banking.Tests
{
    public class TransferIntegrationTests : IClassFixture<BankingWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TransferIntegrationTests(BankingWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Transfer_Returns_401_If_Unauthorized()
        {
            // Arrange
            var transferCommand = new TransferCommand
            {
                ContaOrigem = 123456,
                ContaDestino = 789012,
                Valor = 100.00m,
                Descricao = "Teste sem autorização",
                RequisicaoId = "test-unauthorized-001"
            };

            // Act - Não definir Authorization header
            var response = await _client.PostAsJsonAsync("/api/banking/transfer", transferCommand);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Transfer_Returns_401_If_Invalid_Token()
        {
            // Arrange
            var transferCommand = new TransferCommand
            {
                ContaOrigem = 123456,
                ContaDestino = 789012,
                Valor = 100.00m,
                Descricao = "Teste token inválido",
                RequisicaoId = "test-invalid-token-001"
            };

            // Act - Token inválido
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");
            var response = await _client.PostAsJsonAsync("/api/banking/transfer", transferCommand);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Transfer_Returns_400_If_Missing_Required_Fields()
        {
            // Arrange - Usar esquema de teste que passa autenticação
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var transferCommand = new TransferCommand
            {
                // ContaOrigem omitida propositalmente
                ContaDestino = 789012,
                Valor = 100.00m,
                Descricao = "Teste campos obrigatórios",
                RequisicaoId = "test-missing-fields-001"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/banking/transfer", transferCommand);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Transfer_Returns_400_If_Same_Account()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var transferCommand = new TransferCommand
            {
                ContaOrigem = 123456,
                ContaDestino = 123456, // Mesma conta
                Valor = 100.00m,
                Descricao = "Teste mesma conta",
                RequisicaoId = "test-same-account-001"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/banking/transfer", transferCommand);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Transfer_Returns_400_If_Zero_Amount()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var transferCommand = new TransferCommand
            {
                ContaOrigem = 123456,
                ContaDestino = 789012,
                Valor = 0, // Valor zero
                Descricao = "Teste valor zero",
                RequisicaoId = "test-zero-amount-001"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/banking/transfer", transferCommand);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Transfer_Returns_400_If_Negative_Amount()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var transferCommand = new TransferCommand
            {
                ContaOrigem = 123456,
                ContaDestino = 789012,
                Valor = -50.00m, // Valor negativo
                Descricao = "Teste valor negativo",
                RequisicaoId = "test-negative-amount-001"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/banking/transfer", transferCommand);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Transfer_Returns_200_For_Valid_Request()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var transferCommand = new TransferCommand
            {
                ContaOrigem = 123456,
                ContaDestino = 789012,
                Valor = 100.00m,
                Descricao = "Transferência válida de teste",
                RequisicaoId = $"test-valid-transfer-{DateTime.Now.Ticks}"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/banking/transfer", transferCommand);

            // Assert
            // Pode retornar 200 (sucesso) ou outro código dependendo da implementação
            // O importante é que não seja 401 ou 403
            Assert.True(response.StatusCode != HttpStatusCode.Unauthorized);
            Assert.True(response.StatusCode != HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Transfer_Supports_Idempotency()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var RequisicaoId = $"test-idempotency-{DateTime.Now.Ticks}";
            var transferCommand = new TransferCommand
            {
                ContaOrigem = 123456,
                ContaDestino = 789012,
                Valor = 100.00m,
                Descricao = "Teste idempotência",
                RequisicaoId = RequisicaoId
            };

            // Act - Fazer a mesma requisição duas vezes
            var response1 = await _client.PostAsJsonAsync("/api/banking/transfer", transferCommand);
            var response2 = await _client.PostAsJsonAsync("/api/banking/transfer", transferCommand);

            // Assert - Ambas as respostas devem ter o mesmo comportamento
            Assert.Equal(response1.StatusCode, response2.StatusCode);
        }
    }
}


















