using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using EstudoApi.Contracts;

namespace EstudoApi.Tests;


public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_And_Login_Returns_JwtToken()
    {
        // Arrange
    var register = new RegisterUserRequest("Usuario Teste", "testuser@email.com", "SenhaForte123", "12345678901");
        var login = new LoginRequest("testuser@email.com", "SenhaForte123");

        // Act
        var regResponse = await _client.PostAsJsonAsync("/api/v1/users", register);
        regResponse.EnsureSuccessStatusCode();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", login);
        loginResponse.EnsureSuccessStatusCode();
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
        Assert.True(auth.ExpiresAt > DateTime.UtcNow);
    }
}
