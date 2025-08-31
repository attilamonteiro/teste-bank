using EstudoApi.Domain.CQRS.Commands.Account;
using Swashbuckle.AspNetCore.Filters;

namespace EstudoApi.SwaggerExamples
{
    public class CreateAccountCommandExample : IExamplesProvider<CreateAccountCommand>
    {
        public CreateAccountCommand GetExamples()
        {
            return new CreateAccountCommand
            {
                Cpf = "12345678901",
                Senha = "minhasenha123"
            };
        }
    }
}
