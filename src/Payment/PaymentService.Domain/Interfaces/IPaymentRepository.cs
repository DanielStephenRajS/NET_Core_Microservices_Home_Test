using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task<List<Payment>> GetPaymentAsync(CancellationToken cancellationToken);
        Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken);
    }
}
