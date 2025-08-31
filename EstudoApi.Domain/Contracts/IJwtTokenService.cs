using System;

namespace EstudoApi.Domain.Contracts
{
    public interface IJwtTokenService
    {
        (string token, DateTime expiresAt) CreateTokenForAccount(int accountId);
#if NET8_0_OR_GREATER
        (string token, DateTime expiresAt) CreateToken(object user, System.Collections.Generic.IList<string>? roles = null);
#else
    (string token, DateTime expiresAt) CreateToken(object user, System.Collections.Generic.IList<string> roles = null);
#endif
    }
}
