namespace EstudoApi.Domain.CQRS.Handlers.Account
{
    using System.Threading.Tasks;
    using EstudoApi.Domain.CQRS.Commands.Account;
    using EstudoApi.Interfaces.Repositories;
    using EstudoApi.Domain.Models;

    using MediatR;
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, (bool Success, int? NumeroConta, string? Error, string? Tipo)>
    {
        private readonly IAccountRepository _repository;
        public CreateAccountCommandHandler(IAccountRepository repository)
        {
            _repository = repository;
        }
        public async Task<(bool Success, int? NumeroConta, string? Error, string? Tipo)> Handle(CreateAccountCommand command, CancellationToken cancellationToken)
        {
            if (!CpfUtils.IsValid(command.Cpf))
                return (false, null, "CPF inválido.", "INVALID_DOCUMENT");
            var exists = await _repository.GetAccountByCpf(command.Cpf);
            if (exists != null)
                return (false, null, "Conta já existe para este CPF.", "INVALID_DOCUMENT");
            var account = new Account(command.Cpf, command.Senha);
            await _repository.AddAccount(account);
            return (true, account.Id, null, null);
        }
    }

    public static class CpfUtils
    {
        public static bool IsValid(string cpf)
        {
            // Validação simplificada de CPF
            if (string.IsNullOrWhiteSpace(cpf) || cpf.Length != 11) return false;
            return long.TryParse(cpf, out _);
        }
    }
}
