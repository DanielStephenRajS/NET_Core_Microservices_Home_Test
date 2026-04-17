using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Infra.ApiContext;

namespace PaymentService.Infra.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentServiceDbContext _dbContext;

        public PaymentRepository(PaymentServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task CreatePaymentAsync(Payment payment, CancellationToken cancellationToken)
        {
            await _dbContext.Payments.AddAsync(payment, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Payment>> GetPaymentAsync(CancellationToken cancellationToken)
        {
            var payment = await _dbContext.Payments.
               AsNoTracking().ToListAsync(cancellationToken);
            return payment;
        }
    }
}
