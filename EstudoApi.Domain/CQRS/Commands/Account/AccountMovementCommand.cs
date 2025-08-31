using MediatR;

namespace EstudoApi.Domain.CQRS.Commands.Account
{
    public class AccountMovementCommand : IRequest<(bool Success, string? Error, string? Tipo)>
    {
        public string RequisicaoId { get; set; } = string.Empty;
        public int? NumeroConta { get; set; } // Opcional, pode vir do token
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = string.Empty; // "C" ou "D"
    }
}
