using MediatR;

namespace EstudoApi.Domain.CQRS.Commands.Account
{
    public class TransferCommand : IRequest<(bool Success, string? Error, string? Tipo)>
    {
        /// <summary>
        /// Identificação única da requisição para controle de idempotência
        /// </summary>
        public string RequisicaoId { get; set; } = string.Empty;

        /// <summary>
        /// Número da conta de origem (obrigatório)
        /// </summary>
        public int ContaOrigem { get; set; }

        /// <summary>
        /// Número da conta de destino (obrigatório)
        /// </summary>
        public int ContaDestino { get; set; }

        /// <summary>
        /// Valor da transferência (deve ser positivo)
        /// </summary>
        public decimal Valor { get; set; }

        /// <summary>
        /// Descrição opcional da transferência
        /// </summary>
        public string? Descricao { get; set; }
    }
}
