
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;

namespace PaymentService.Infra.ApiContext
{
    public class PaymentServiceDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public PaymentServiceDbContext(DbContextOptions<PaymentServiceDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }
    }
}
