using System.Security.Claims;

namespace EstudoApi.Helpers
{
    public static class JwtClaimHelper
    {
        public static int? ExtrairNumeroConta(ClaimsPrincipal user)
        {
            string[] possiveisNomes = new[] { "idcontacorrente", "accountId", ClaimTypes.NameIdentifier, "sub" };
            foreach (var nome in possiveisNomes)
            {
                var valor = user.FindFirst(nome)?.Value;
                if (!string.IsNullOrEmpty(valor) && int.TryParse(valor, out int numeroConta) && numeroConta > 0)
                {
                    return numeroConta;
                }
            }
            return null;
        }
    }
}