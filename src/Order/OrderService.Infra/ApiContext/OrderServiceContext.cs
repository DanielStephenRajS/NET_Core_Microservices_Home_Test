using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infra.ApiContext
{
    public class OrderServiceContext : DbContext
    {
        public OrderServiceContext(DbContextOptions<OrderServiceContext> options) : base(options)
        {
        }

        public DbSet<Orders> Orders { get; set; }
    }
}
