using System.Threading.Tasks;
using EstudoApi.Domain.CQRS.Commands;
using EstudoApi.Infrastructure.Auth;
using EstudoApi.Domain.Contracts;
using Microsoft.AspNetCore.Identity;
using EstudoApi.Infrastructure.Identity;

namespace EstudoApi.Infrastructure.CQRS.Handlers
{
    public class LoginUserCommandHandler
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;

        public LoginUserCommandHandler(UserManager<AppUser> userManager, IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<(string token, DateTime expiresAt)?> Handle(LoginUserCommand command)
        {

            if (string.IsNullOrEmpty(command.Email) || string.IsNullOrEmpty(command.Password))
                return null;

            var user = await _userManager.FindByEmailAsync(command.Email);
            if (user == null)
                return null;

            var valid = await _userManager.CheckPasswordAsync(user, command.Password);
            if (!valid)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            var (token, expiresAt) = _jwtTokenService.CreateToken(user, roles);
            return (token, expiresAt);
        }
    }
}
