namespace BarnManagementAPI.Contracts
{
    public record BalanceDto : IEquatable<BalanceDto>
    {
        public decimal Amount { get; init; }
    }
}
