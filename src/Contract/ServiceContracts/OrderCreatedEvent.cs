namespace ServiceContracts
{
    public record OrderCreatedEvent
    {
        public int OrderId { get; init; }
        public decimal Amount { get; init; }
        public required string CustomerEmail { get; init; } 
    }
}
