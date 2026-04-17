

namespace PaymentService.Domain.Entities
{
    public record Payment
    {
        public required  string Id { get; init; }  
        public int OrderId { get; init; }

        public decimal Amount { get; init; }

        public required string CustomerEmail { get; init; }

        public required string Status { get; init; }

        public DateTime Timestamp { get; init; }
    }
}
