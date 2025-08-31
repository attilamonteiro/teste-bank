namespace EstudoApi.Domain.CQRS.Commands.Account
{
    public class DepositCommand
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
