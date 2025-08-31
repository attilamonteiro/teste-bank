using EstudoApi.Domain.CQRS.Commands.Account;
using Swashbuckle.AspNetCore.Filters;

namespace EstudoApi.SwaggerExamples
{
    public class AccountMovementCommandExample : IExamplesProvider<AccountMovementCommand>
    {
        public AccountMovementCommand GetExamples()
        {
            return new AccountMovementCommand
            {
                RequisicaoId = "req-123",
                NumeroConta = 1,
                Valor = 10,
                Tipo = "C" // "C" para crédito, "D" para débito
            };
        }
    }
}
