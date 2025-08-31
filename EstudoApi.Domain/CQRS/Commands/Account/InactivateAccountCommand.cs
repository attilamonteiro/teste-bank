using MediatR;

namespace EstudoApi.Domain.CQRS.Commands.Account
{
    public class InactivateAccountCommand : IRequest<(bool Success, string? Error, string? Tipo)>
    {
        public int AccountId { get; set; }
        public string Senha { get; set; } = string.Empty;
    }
}
