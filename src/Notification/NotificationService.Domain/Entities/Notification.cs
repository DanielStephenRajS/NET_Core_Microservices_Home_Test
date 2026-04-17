namespace NotificationService.Domain.Entities
{
    public record Notification
    {
        public int Id { get; init; }
        public int OrderId { get; init; }
        public required string PaymentId { get; init; }
        public decimal Amount { get; init; }
        public DateTime Timestamp { get; init; }
    }
}
