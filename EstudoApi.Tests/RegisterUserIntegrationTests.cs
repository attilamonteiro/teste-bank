using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using EstudoApi.Contracts;

namespace EstudoApi.Tests
{
    public class RegisterUserIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public RegisterUserIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_User_Works()
        {
            var register = new RegisterUserRequest("Usuário Teste", "register-teste@email.com", "Senha123", "12345678901");
            var regResp = await _client.PostAsJsonAsync("/api/v1/users", register);
            regResp.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Register_Duplicate_User_Returns_Conflict()
        {
            var register = new RegisterUserRequest("Usuário Teste", "duplicado@email.com", "Senha123", "12345678901");
            var regResp1 = await _client.PostAsJsonAsync("/api/v1/users", register);
            regResp1.EnsureSuccessStatusCode();
            var regResp2 = await _client.PostAsJsonAsync("/api/v1/users", register);
            Assert.Equal(System.Net.HttpStatusCode.Conflict, regResp2.StatusCode);
        }
    }
}
