namespace EstudoApi.Domain.CQRS.Commands
{
    public class LoginUserCommand
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
