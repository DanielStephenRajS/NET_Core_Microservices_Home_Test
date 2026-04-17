using OrderService.Domain.Entities;

namespace OrderService.Domain.NewFolder
{
    public interface IOrderRepository
    {
        Task<List<Orders>> GetOrdersAsync(CancellationToken cancellationToken);

        Task<int> CreateOrderAsync(Orders order, CancellationToken cancellationToken);
    }
}
