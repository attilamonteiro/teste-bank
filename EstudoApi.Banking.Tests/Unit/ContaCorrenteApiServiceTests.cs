using System.Net;
using System.Text;
using System.Text.Json;
using EstudoApi.Banking.Transfer.Services;
using EstudoApi.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace EstudoApi.Banking.Tests.Unit
{
    public class ContaCorrenteApiServiceTests
    {
        private readonly Mock<ILogger<TransferContaCorrenteApiService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly TransferContaCorrenteApiService _service;

        public ContaCorrenteApiServiceTests()
        {
            _mockLogger = new Mock<ILogger<TransferContaCorrenteApiService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // Configurar mock do HttpClient
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost:5041")
            };

            _service = new TransferContaCorrenteApiService(_httpClient, _mockLogger.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task ValidateContaAsync_Returns_True_For_Existing_Account()
        {
            // Arrange
            var numeroConta = "123456";
            var contaResponse = new ContaCorrente 
            { 
                Numero = 123456, 
                Nome = "Test Account"
            };

            // Mock da busca de conta (retorna a conta)
            SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(contaResponse), $"/api/contas/{numeroConta}");

            // Act
            var result = await _service.GetContaAsync(numeroConta);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(123456, result.Numero);
            VerifyHttpCall($"/api/contas/{numeroConta}", HttpMethod.Get);
        }

        [Fact]
        public async Task ValidateContaAsync_Returns_False_For_Nonexistent_Account()
        {
            // Arrange
            var numeroConta = "999999";

            // Mock do ping (retorna 200)
            SetupHttpResponse(HttpStatusCode.OK, "pong", "/api/ping");

            // Mock da busca de conta (retorna 404)
            SetupHttpResponse(HttpStatusCode.NotFound, "", $"/api/contas/{numeroConta}");

            // Act
            var result = await _service.ValidateContaAsync(numeroConta);

            // Assert
            Assert.False(result);
            VerifyHttpCall("/api/ping", HttpMethod.Get);
            VerifyHttpCall($"/api/contas/{numeroConta}", HttpMethod.Get);
        }

        [Fact]
        public async Task ValidateContaAsync_Returns_False_When_API_Ping_Fails()
        {
            // Arrange
            var numeroConta = "123456";

            // Mock do ping (retorna 500)
            SetupHttpResponse(HttpStatusCode.InternalServerError, "Error", "/api/ping");

            // Act
            var result = await _service.ValidateContaAsync(numeroConta);

            // Assert
            Assert.False(result);
            VerifyHttpCall("/api/ping", HttpMethod.Get);
            // Não deve chamar a API de conta se o ping falhar
            VerifyHttpCallNeverMade($"/api/contas/{numeroConta}");
        }

        [Fact]
        public async Task GetContaAsync_Returns_Account_For_Valid_Request()
        {
            // Arrange
            var numeroConta = "123456";
            var expectedConta = new ContaCorrente
            {
                Numero = 123456,
                Nome = "Test Account"
            };

            SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(expectedConta), $"/api/contas/{numeroConta}");

            // Act
            var result = await _service.GetContaAsync(numeroConta);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(123456, result.Numero);
            Assert.True(result.IsAtiva);
            VerifyHttpCall($"/api/contas/{numeroConta}", HttpMethod.Get);
        }

        [Fact]
        public async Task GetContaAsync_Returns_Null_For_Nonexistent_Account()
        {
            // Arrange
            var numeroConta = "999999";

            SetupHttpResponse(HttpStatusCode.NotFound, "", $"/api/contas/{numeroConta}");

            // Act
            var result = await _service.GetContaAsync(numeroConta);

            // Assert
            Assert.Null(result);
            VerifyHttpCall($"/api/contas/{numeroConta}", HttpMethod.Get);
        }

        [Fact]
        public async Task UpdateSaldoAsync_Returns_True_For_Successful_Update()
        {
            // Arrange
            var numeroConta = "123456";
            var novoSaldo = 2000.00m;

            SetupHttpResponse(HttpStatusCode.OK, "", $"/api/contas/{numeroConta}/saldo");

            // Act
            var result = await _service.UpdateSaldoAsync(numeroConta, novoSaldo);

            // Assert
            Assert.True(result);
            VerifyHttpCall($"/api/contas/{numeroConta}/saldo", HttpMethod.Put);
        }

        [Fact]
        public async Task UpdateSaldoAsync_Returns_False_For_Failed_Update()
        {
            // Arrange
            var numeroConta = "123456";
            var novoSaldo = 2000.00m;

            SetupHttpResponse(HttpStatusCode.BadRequest, "Invalid request", $"/api/contas/{numeroConta}/saldo");

            // Act
            var result = await _service.UpdateSaldoAsync(numeroConta, novoSaldo);

            // Assert
            Assert.False(result);
            VerifyHttpCall($"/api/contas/{numeroConta}/saldo", HttpMethod.Put);
        }

        [Fact]
        public async Task ValidateContaAsync_Handles_HttpRequestException()
        {
            // Arrange
            var numeroConta = "123456";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _service.ValidateContaAsync(numeroConta);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateContaAsync_Handles_TaskCanceledException()
        {
            // Arrange
            var numeroConta = "123456";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("Timeout"));

            // Act
            var result = await _service.ValidateContaAsync(numeroConta);

            // Assert
            Assert.False(result);
        }

        private void SetupHttpResponse(HttpStatusCode statusCode, string content, string requestUri)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.PathAndQuery == requestUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }

        private void VerifyHttpCall(string requestUri, HttpMethod method)
        {
            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri!.PathAndQuery == requestUri &&
                        req.Method == method),
                    ItExpr.IsAny<CancellationToken>());
        }

        private void VerifyHttpCallNeverMade(string requestUri)
        {
            _mockHttpMessageHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Never(),
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.PathAndQuery == requestUri),
                    ItExpr.IsAny<CancellationToken>());
        }
    }
}


















