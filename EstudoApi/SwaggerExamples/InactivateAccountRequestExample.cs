using EstudoApi.Controllers;
using Swashbuckle.AspNetCore.Filters;

namespace EstudoApi.SwaggerExamples
{
    public class InactivateAccountRequestExample : IExamplesProvider<ContaCorrenteController.InactivateAccountRequest>
    {
        public ContaCorrenteController.InactivateAccountRequest GetExamples()
        {
            return new ContaCorrenteController.InactivateAccountRequest
            {
                Senha = "minhaSenhaForte123"
            };
        }
    }
}
