using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EstudoApi.Infrastructure.Identity;
using Microsoft.IdentityModel.Tokens;


using EstudoApi.Domain.Contracts;
namespace EstudoApi.Infrastructure.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _opts;

    public JwtTokenService(JwtOptions opts)
    {
        _opts = opts;
    }

    public (string token, DateTime expiresAt) CreateToken(object userObj, IList<string>? roles = null)
    {
        if (userObj is not AppUser user)
            throw new ArgumentException("user must be AppUser", nameof(userObj));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Nome),
            new(ClaimTypes.Email, user.Email!),
            new("cpf", user.Cpf)
        };

        if (roles != null)
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_opts.ExpMinutes);

        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: cred
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }
    public (string token, DateTime expiresAt) CreateTokenForAccount(int accountId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, accountId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("accountId", accountId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_opts.ExpMinutes);

        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: cred
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }
}
