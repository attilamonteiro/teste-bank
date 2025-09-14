using EstudoApi.Contracts;
using Swashbuckle.AspNetCore.Filters;

namespace EstudoApi.SwaggerExamples
{
    public class LoginRequestExample : IExamplesProvider<LoginRequest>
    {
        public LoginRequest GetExamples()
        {
            return new LoginRequest(
                Cpf: "12345678901",
                Senha: "minhasenha123"
            );
        }
    }
}
