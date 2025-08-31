using MediatR;

namespace EstudoApi.Domain.CQRS.Commands.Account
{
    public class GetAccountBalanceQuery : IRequest<GetAccountBalanceResult>
    {
        public int NumeroConta { get; set; }
    }

    public class GetAccountBalanceResult
    {
        public bool Found { get; set; }
        public bool Ativo { get; set; }
        public decimal Balance { get; set; }
        public int NumeroConta { get; set; }
        public string? NomeTitular { get; set; }
        public DateTime DataConsulta { get; set; }
    }
}
