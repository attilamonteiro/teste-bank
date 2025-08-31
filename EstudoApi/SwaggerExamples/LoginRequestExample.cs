using EstudoApi.Contracts;
using Swashbuckle.AspNetCore.Filters;

namespace EstudoApi.SwaggerExamples
{
    public class LoginRequestExample : IExamplesProvider<LoginRequest>
    {
        public LoginRequest GetExamples()
        {
            return new LoginRequest(
                Email: "joao@email.com",
                Senha: "minhasenha123"
            );
        }
    }
}
