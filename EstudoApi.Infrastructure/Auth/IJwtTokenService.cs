namespace EstudoApi.Infrastructure.Auth
{
    public interface IJwtTokenService
    {
        (string token, DateTime expiresAt) CreateToken(object userObj, IList<string>? roles = null);
        (string token, DateTime expiresAt) CreateTokenForAccount(int accountId);
    }
}
