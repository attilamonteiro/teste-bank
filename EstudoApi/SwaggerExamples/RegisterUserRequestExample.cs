using EstudoApi.Contracts;
using Swashbuckle.AspNetCore.Filters;

namespace EstudoApi.SwaggerExamples
{
    public class RegisterUserRequestExample : IExamplesProvider<RegisterUserRequest>
    {
        public RegisterUserRequest GetExamples()
        {
            return new RegisterUserRequest(
                Nome: "João da Silva",
                Email: "joao@email.com",
                Senha: "minhasenha123",
                Cpf: "12345678901"
            );
        }
    }
}
