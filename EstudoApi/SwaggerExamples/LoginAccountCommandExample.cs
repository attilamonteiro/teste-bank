using EstudoApi.Domain.CQRS.Commands.Account;
using Swashbuckle.AspNetCore.Filters;

namespace EstudoApi.SwaggerExamples
{
    public class LoginAccountCommandExample : IExamplesProvider<LoginAccountCommand>
    {
        public LoginAccountCommand GetExamples()
        {
            return new LoginAccountCommand
            {
                NumeroConta = 1,
                Cpf = "12345678901",
                Senha = "minhasenha123"
            };
        }
    }
}
