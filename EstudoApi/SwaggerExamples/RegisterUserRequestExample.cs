using EstudoApi.Contracts;
using Swashbuckle.AspNetCore.Filters;

namespace EstudoApi.SwaggerExamples
{
    public class RegisterUserRequestExample : IExamplesProvider<RegisterUserRequest>
    {
        public RegisterUserRequest GetExamples()
        {
            return new RegisterUserRequest(
                Cpf: "12345678901",
                Senha: "minhasenha123"
            );
        }
    }
}
