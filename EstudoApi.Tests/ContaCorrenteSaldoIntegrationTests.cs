using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace EstudoApi.Tests
{
    public class ContaCorrenteSaldoIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ContaCorrenteSaldoIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }


        [Fact]
        public async Task GetSaldo_Returns_401_If_Unauthorized()
        {
            // Não seta Authorization header
            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync("/api/conta/saldo");
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }


        [Fact]
        public async Task GetSaldo_Returns_200_If_Authorized()
        {
            // Arrange: cria conta e faz login para obter token
            var cpf = "12345678901";
            var senha = "SenhaForte123";
            var createResp = await _client.PostAsJsonAsync("/api/auth/conta/cadastrar", new { cpf, senha });
            createResp.EnsureSuccessStatusCode();
            var loginResp = await _client.PostAsJsonAsync("/api/auth/conta/login", new { cpf, senha });
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(loginJson);
            Assert.False(string.IsNullOrWhiteSpace(loginJson.token));

            // Act: consulta saldo autenticado
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.token);
            var saldoResp = await _client.GetAsync("/api/conta/saldo");
            saldoResp.EnsureSuccessStatusCode();
            var saldoJson = await saldoResp.Content.ReadFromJsonAsync<SaldoResponse>();
            Assert.NotNull(saldoJson);
            Assert.Equal("0.00", saldoJson.saldo); // saldo inicial
        }

        private class LoginResponse { public string token { get; set; } }
        private class SaldoResponse { public int numeroConta { get; set; } public string nomeTitular { get; set; } public string dataConsulta { get; set; } public string saldo { get; set; } }
    }
}
