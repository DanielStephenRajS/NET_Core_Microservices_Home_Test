using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.App.Features.Queries.Models;
using OrderService.Domain.NewFolder;

namespace OrderService.App.Features.Queries
{
    public class OrderServiceQueryHandler : IRequestHandler<OrderServiceQuery, List<OrderServiceResponse>>
    {
        private readonly ILogger<OrderServiceQueryHandler> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public OrderServiceQueryHandler(ILogger<OrderServiceQueryHandler> logger, IOrderRepository orderRepository, IMapper mapper)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<List<OrderServiceResponse>> Handle(OrderServiceQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling OrderServiceQuery to retrieve all orders.");

            var orders = await _orderRepository.GetOrdersAsync(cancellationToken).ConfigureAwait(false);
            if (orders == null) return new List<OrderServiceResponse>();

            var orderResponses = _mapper.Map<List<OrderServiceResponse>>(orders);
            _logger.LogInformation("Successfully retrieved and mapped {Count} orders.", orderResponses.Count);

            return orderResponses;
        }
    }
}
