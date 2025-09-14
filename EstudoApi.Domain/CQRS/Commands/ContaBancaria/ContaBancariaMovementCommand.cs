using MediatR;

namespace EstudoApi.Domain.CQRS.Commands.ContaBancaria
{
    public class ContaBancariaMovementCommand : IRequest<(bool Success, string? Error, string? Tipo)>
    {
        public string RequisicaoId { get; set; } = string.Empty;
        public int? NumeroConta { get; set; }
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = string.Empty; // "C" = Crédito, "D" = Débito
        public string? Descricao { get; set; }
    }
}
