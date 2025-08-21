namespace EstudoApi.Contracts;

public record RegisterUserRequest(string Nome, string Email, string Senha, string Cpf);
public record LoginRequest(string Email, string Senha);
public record AuthResponse(string AccessToken, DateTime ExpiresAt, string TokenType = "Bearer");
