using MediatR;

namespace EstudoApi.Domain.CQRS.Commands.ContaBancaria
{
    public class CreateContaBancariaCommand : IRequest<(bool Success, int? NumeroConta, string? Error, string? Tipo)>
    {
        public string Cpf { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}
