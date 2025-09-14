namespace EstudoApi.Domain.CQRS.Commands
{
    public class LoginUserCommand
    {
        public string? Cpf { get; set; }
        public string? Password { get; set; }
    }
}
