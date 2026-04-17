using System.ComponentModel.DataAnnotations;

namespace OrderService.Domain.Entities
{
    public record Orders
    {
        [Key]
        public int Id { get; init; }

        public decimal Amount { get; init; }

        public required string CustomerEmail { get; init; }

        public required string Status { get; init; }

        public DateTime CreatedDate { get; init; }
    }
}
