using EstudoApi.Infrastructure.Identity;

namespace EstudoApi.Infrastructure.Auth;

public interface IJwtTokenService
{
    (string token, DateTime expiresAt) CreateToken(AppUser user, IList<string>? roles = null);
}
