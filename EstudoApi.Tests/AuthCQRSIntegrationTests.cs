using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using EstudoApi.Contracts;

namespace EstudoApi.Tests
{
    public class AuthCQRSIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthCQRSIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_CQRS_Flow_Works()
        {
            // Arrange: criar usuário
            var register = new RegisterUserRequest("Usuário CQRS", "login-cqrs@email.com", "Senha123", "12345678901");
            var regResp = await _client.PostAsJsonAsync("/api/v1/users", register);
            regResp.EnsureSuccessStatusCode();

            // Act: login
            var login = new LoginRequest(register.Email, register.Senha);
            var loginResp = await _client.PostAsJsonAsync("/api/v1/auth/login", login);
            loginResp.EnsureSuccessStatusCode();
            var auth = await loginResp.Content.ReadFromJsonAsync<AuthResponse>();

            // Assert
            Assert.NotNull(auth);
            Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
            Assert.Equal("Bearer", auth.TokenType);
            Assert.True(auth.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task Login_Invalid_Credentials_Returns_Unauthorized()
        {
            var login = new LoginRequest("naoexiste@email.com", "SenhaErrada");
            var loginResp = await _client.PostAsJsonAsync("/api/v1/auth/login", login);
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, loginResp.StatusCode);
        }
    }
}
