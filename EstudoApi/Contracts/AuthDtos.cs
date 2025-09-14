namespace EstudoApi.Contracts;

public record RegisterUserRequest(string Cpf, string Senha);
public record LoginRequest(string Cpf, string Senha);
public record AuthResponse(string AccessToken, DateTime ExpiresAt, string TokenType = "Bearer");
