
using System.Threading.Tasks;
using EstudoApi.Domain.CQRS.Commands.Account;
using EstudoApi.Interfaces.Repositories;
using EstudoApi.Domain.Models;
using EstudoApi.Domain.Contracts;

namespace EstudoApi.Domain.CQRS.Handlers.Account
{
    using MediatR;
    public class LoginAccountCommandHandler : IRequestHandler<LoginAccountCommand, (bool Success, string? Token, string? Error, string? Tipo)>
    {
        private readonly IAccountRepository _repository;
        private readonly IJwtTokenService _jwtService;
        public LoginAccountCommandHandler(IAccountRepository repository, IJwtTokenService jwtService)
        {
            _repository = repository;
            _jwtService = jwtService;
        }
        public async Task<(bool Success, string? Token, string? Error, string? Tipo)> Handle(LoginAccountCommand command, CancellationToken cancellationToken)
        {
            EstudoApi.Domain.Models.Account? account = null;
            if (command.NumeroConta.HasValue)
            {
                account = await _repository.GetAccount(command.NumeroConta.Value);
            }
            else if (!string.IsNullOrWhiteSpace(command.Cpf))
            {
                account = await _repository.GetAccountByCpf(command.Cpf);
            }
            if (account == null || !account.Ativo)
                return (false, null, "Conta não encontrada ou inativa.", "USER_UNAUTHORIZED");
            if (!account.ValidarSenha(command.Senha))
                return (false, null, "Senha inválida.", "USER_UNAUTHORIZED");
            (string token, DateTime _) = _jwtService.CreateTokenForAccount(account.Id);
            return (true, token, null, null);
        }
    }
}
