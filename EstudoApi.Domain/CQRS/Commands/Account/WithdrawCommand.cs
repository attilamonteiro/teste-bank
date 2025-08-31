namespace EstudoApi.Domain.CQRS.Commands.Account
{
    public class WithdrawCommand
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
