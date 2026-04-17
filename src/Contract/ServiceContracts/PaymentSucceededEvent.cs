namespace ServiceContracts
{
    public record PaymentSucceededEvent
    {
        public int OrderId { get; init; }
        public required string PaymentId { get; init; }
        public decimal Amount { get; init; }
        public DateTime Timestamp { get; init; }
    }
}
