using MediatR;

namespace EstudoApi.Domain.CQRS.Commands.ContaBancaria
{
    public class LoginContaBancariaCommand : IRequest<(bool Success, EstudoApi.Domain.Models.ContaBancaria? Conta, string? Error, string? Tipo)>
    {
        public int? NumeroConta { get; set; }
        public string? Cpf { get; set; }
        public string Senha { get; set; } = string.Empty;
    }
}
