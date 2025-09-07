using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EstudoApi.Banking.Transfer.Commands;
using Xunit;

namespace EstudoApi.Banking.Tests.E2E
{
    /// <summary>
    /// Testes End-to-End que usam as APIs reais com JWT
    /// Requer que ambas as APIs estejam rodando (EstudoApi na 5041 e EstudoApi.Banking na 60016)
    /// </summary>
    public class BankingE2ETests
    {
        private readonly HttpClient _mainApiClient;
        private readonly HttpClient _bankingApiClient;

        public BankingE2ETests()
        {
            _mainApiClient = new HttpClient { BaseAddress = new Uri("http://localhost:5041") };
            _bankingApiClient = new HttpClient { BaseAddress = new Uri("http://localhost:60016") };
        }

        [Fact(Skip = "Requires both APIs running")]
        public async Task FullTransferFlow_WithRealJWT_Should_Work()
        {
            try
            {
                // 1. Registrar usuário na API principal
                var registerRequest = new
                {
                    cpf = "12345678901",
                    senha = "SenhaForte123"
                };

                var registerResponse = await _mainApiClient.PostAsJsonAsync("/api/auth/conta/cadastrar", registerRequest);

                if (!registerResponse.IsSuccessStatusCode)
                {
                    var registerError = await registerResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Erro no registro: {registerError}");
                }

                // 2. Fazer login para obter JWT
                var loginRequest = new
                {
                    cpf = "12345678901",
                    senha = "SenhaForte123"
                };

                var loginResponse = await _mainApiClient.PostAsJsonAsync("/api/auth/login", loginRequest);
                loginResponse.EnsureSuccessStatusCode();

                var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
                var token = loginJson.GetProperty("token").GetString();

                Assert.NotNull(token);
                Assert.NotEmpty(token);

                // 3. Configurar header de autorização na API Banking
                _bankingApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 4. Testar endpoint de validação de conta
                var validationResponse = await _bankingApiClient.GetAsync("/api/banking/test/conta/123456");

                // Deve retornar 200 ou 404, mas não 401/500
                Assert.True(validationResponse.StatusCode == HttpStatusCode.OK ||
                           validationResponse.StatusCode == HttpStatusCode.NotFound);

                // 5. Tentar transferência (pode falhar por regras de negócio, mas não por autenticação)
                var transferCommand = new TransferCommand
                {
                    ContaOrigem = 123456,
                    ContaDestino = 789012,
                    Valor = 50.00m,
                    Descricao = "Teste E2E",
                    RequisicaoId = $"e2e-test-{DateTime.Now.Ticks}"
                };

                var transferResponse = await _bankingApiClient.PostAsJsonAsync("/api/banking/transfer", transferCommand);

                // Não deve retornar 401 (Unauthorized)
                Assert.NotEqual(HttpStatusCode.Unauthorized, transferResponse.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erro de conectividade: {ex.Message}. Verifique se as APIs estão rodando.");
            }
        }

        [Fact(Skip = "Requires both APIs running")]
        public async Task BankingAPI_Should_Reject_Invalid_JWT()
        {
            try
            {
                // Configurar token inválido
                _bankingApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token-invalido");

                // Tentar acessar endpoint protegido
                var response = await _bankingApiClient.GetAsync("/api/banking/test/conta/123456");

                // Deve retornar 401
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erro de conectividade: {ex.Message}. Verifique se a API Banking está rodando.");
            }
        }

        [Fact(Skip = "Requires both APIs running")]
        public async Task BankingAPI_Should_Reject_Missing_JWT()
        {
            try
            {
                // Não configurar header de autorização
                _bankingApiClient.DefaultRequestHeaders.Authorization = null;

                // Tentar acessar endpoint protegido
                var response = await _bankingApiClient.GetAsync("/api/banking/test/conta/123456");

                // Deve retornar 401
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erro de conectividade: {ex.Message}. Verifique se a API Banking está rodando.");
            }
        }

        [Fact(Skip = "Requires Banking API running")]
        public async Task BankingAPI_Swagger_Should_Be_Accessible()
        {
            try
            {
                var response = await _bankingApiClient.GetAsync("/swagger");

                // Swagger não requer autenticação
                Assert.True(response.StatusCode == HttpStatusCode.OK ||
                           response.StatusCode == HttpStatusCode.Redirect);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erro de conectividade: {ex.Message}. Verifique se a API Banking está rodando.");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mainApiClient?.Dispose();
                _bankingApiClient?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}


















