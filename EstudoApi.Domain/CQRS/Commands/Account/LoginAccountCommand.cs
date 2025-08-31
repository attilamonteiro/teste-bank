using MediatR;
namespace EstudoApi.Domain.CQRS.Commands.Account
{
    using MediatR;
    public class LoginAccountCommand : IRequest<(bool Success, string? Token, string? Error, string? Tipo)>
    {
        public int? NumeroConta { get; set; }
        public string? Cpf { get; set; }
        public string Senha { get; set; } = string.Empty;
    }
}
