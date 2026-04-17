

namespace PaymentService.Domain.Models
{
    public record PaymentServiceResponse
    {
        public required string PaymentId { get; init; }
        public int OrderId { get; init; }

        public decimal Amount { get; init; }

        public required string CustomerEmail { get; init; }

        public required string Status { get; init; }

        public DateTime Timestamp { get; init; }
    }
}
