namespace EstudoApi.Infrastructure.Auth;

public class JwtOptions
{
    public string Issuer  { get; set; } = default!;
    public string Audience{ get; set; } = default!;
    public string Key     { get; set; } = default!;
    public int    ExpMinutes { get; set; } = 60;
}
