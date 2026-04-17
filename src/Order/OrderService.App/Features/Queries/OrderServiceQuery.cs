using MediatR;
using OrderService.App.Features.Queries.Models;

namespace OrderService.App.Features.Queries
{
    public class OrderServiceQuery : IRequest<List<OrderServiceResponse>>
    {
    }
}
