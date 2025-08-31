using MediatR;
namespace EstudoApi.Domain.CQRS.Commands.Account
{
    public class CreateAccountCommand : IRequest<(bool Success, int? NumeroConta, string? Error, string? Tipo)>
    {
        public string Cpf { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}
