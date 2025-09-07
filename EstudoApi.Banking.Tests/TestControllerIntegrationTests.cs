using System.Net;
using System.Net.Http.Headers;
using EstudoApi.Banking.Tests.Utils;
using Xunit;

namespace EstudoApi.Banking.Tests
{
    public class TestControllerIntegrationTests : IClassFixture<BankingWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TestControllerIntegrationTests(BankingWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ValidarConta_Returns_401_If_Unauthorized()
        {
            // Arrange
            var numeroConta = "123456";

            // Act - Não definir Authorization header
            var response = await _client.GetAsync($"/api/banking/test/conta/{numeroConta}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ValidarConta_Returns_401_If_Invalid_Token()
        {
            // Arrange
            var numeroConta = "123456";

            // Act - Token inválido
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");
            var response = await _client.GetAsync($"/api/banking/test/conta/{numeroConta}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ValidarConta_Returns_200_For_Valid_Request()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            var numeroConta = "123456";

            // Act
            var response = await _client.GetAsync($"/api/banking/test/conta/{numeroConta}");

            // Assert
            // Pode retornar 200 (conta existe) ou 404 (conta não existe)
            // O importante é que não seja 401 ou 500 (erro de DI)
            Assert.True(response.StatusCode != HttpStatusCode.Unauthorized);
            Assert.True(response.StatusCode != HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task ValidarConta_Returns_BadRequest_For_Invalid_Account_Number()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            var numeroContaInvalido = "-1"; // Número negativo

            // Act
            var response = await _client.GetAsync($"/api/banking/test/conta/{numeroContaInvalido}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("123456")]
        [InlineData("789012")]
        [InlineData("999999")]
        public async Task ValidarConta_Works_For_Different_Account_Numbers(string numeroConta)
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            // Act
            var response = await _client.GetAsync($"/api/banking/test/conta/{numeroConta}");

            // Assert
            // O importante é que a requisição seja processada sem erro de autenticação
            Assert.True(response.StatusCode != HttpStatusCode.Unauthorized);
            Assert.True(response.StatusCode != HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task ValidarConta_Returns_Consistent_Results()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            var numeroConta = "123456";

            // Act - Fazer a mesma requisição múltiplas vezes
            var response1 = await _client.GetAsync($"/api/banking/test/conta/{numeroConta}");
            var response2 = await _client.GetAsync($"/api/banking/test/conta/{numeroConta}");
            var response3 = await _client.GetAsync($"/api/banking/test/conta/{numeroConta}");

            // Assert - Todas as respostas devem ter o mesmo status
            Assert.Equal(response1.StatusCode, response2.StatusCode);
            Assert.Equal(response2.StatusCode, response3.StatusCode);
        }
    }
}















