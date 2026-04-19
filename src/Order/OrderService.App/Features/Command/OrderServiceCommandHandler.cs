using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;
using OrderService.Domain.NewFolder;
using ServiceContracts;

namespace OrderService.App.Features.Command
{
    public class OrderServiceCommandHandler : IRequestHandler<QrderServiceCommand, int>
    {
        private readonly ILogger<OrderServiceCommandHandler> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderServiceCommandHandler(ILogger<OrderServiceCommandHandler> logger, IOrderRepository orderRepository, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<int> Handle(QrderServiceCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling OrderServiceCommand to create a new order.");
            if (request == null)
            {
                _logger.LogError("Received null request in OrderServiceCommandHandler.");
                throw new ArgumentNullException(nameof(request));
            }

            var orderEntity = new Orders
            {
                Id = new Random().Next(1, 1000), // Randomly generate Id
                Amount = request.OrderServiceRequest.Amount,
                CustomerEmail = request.OrderServiceRequest.CustomerEmail,
                Status = request.OrderServiceRequest.Status,
                CreatedDate = request.OrderServiceRequest.CreatedDate
            };

            var orderId = await _orderRepository.CreateOrderAsync(orderEntity, cancellationToken);
            _logger.LogInformation("Order created with Id: {OrderId}", orderId);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(15)); // Hang up after 10 seconds like a circuit breaker

            // Publish an event to message broker
            await _publishEndpoint.Publish(new OrderCreatedEvent
            {
                OrderId = orderId,
                Amount = orderEntity.Amount,
                CustomerEmail = orderEntity.CustomerEmail,
            }, cts.Token);

            _logger.LogInformation(
           "[OrderService] Order {OrderId} created and event published.", orderId);

            return orderId;
        }
    }
}
