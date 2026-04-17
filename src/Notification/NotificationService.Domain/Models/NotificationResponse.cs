namespace NotificationService.Domain.Models
{
    public record NotificationResponse
    {
        public int NotificationId { get; init; }
        public int OrderId { get; init; }
        public required string PaymentId { get; init; }
        public decimal Amount { get; init; }
        public DateTime Timestamp { get; init; }
    }
}
