using Microsoft.AspNetCore.Identity;

namespace EstudoApi.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public string Nome { get; set; } = default!;
    public string Cpf  { get; set; } = default!;
}
