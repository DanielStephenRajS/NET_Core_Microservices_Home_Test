using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.NewFolder;
using OrderService.Infra.ApiContext;

namespace OrderService.Infra.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderServiceContext _context;
        public OrderRepository(OrderServiceContext context)
        {
            _context = context;
        }

        public async Task<int> CreateOrderAsync(Orders order, CancellationToken cancellationToken)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            await _context.Orders.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            var orderId = order.Id;
            return orderId;
        }

        public async Task<List<Orders>> GetOrdersAsync(CancellationToken cancellationToken)
        {
            var response = await _context.Orders.AsNoTracking().ToListAsync(cancellationToken);
            return response;
        }
    }
}
