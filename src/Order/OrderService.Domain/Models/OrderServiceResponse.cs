namespace OrderService.App.Features.Queries.Models
{
    public record OrderServiceResponse
    {
        public int OrderId { get; init; }
        public decimal Amount { get; init; }
        public required string CustomerEmail { get; init; }
        public required string Status { get; init; }
        public DateTime CreatedDate { get; init; }
    }
}
