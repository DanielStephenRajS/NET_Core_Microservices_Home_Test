using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentService.App.Constants;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using ServiceContracts;

namespace PaymentService.App.Features.Consumer
{
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedConsumer> _logger;
        private readonly IPaymentRepository _paymentRepository;

        public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, IPaymentRepository paymentRepository)
        {
            _logger = logger;
            _paymentRepository = paymentRepository;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            _logger.LogInformation("Received OrderCreatedEvent: {Message}", context.Message);
            var message = context.Message;

            _logger.LogInformation("Received OrderCreatedEvent: OrderId={OrderId}, Amount={Amount}, CustomerEmail={CustomerEmail}",
                message.OrderId, message.Amount, message.CustomerEmail);

            var payment = new Payment
            {
                OrderId = message.OrderId,
                Id = $"abc- {new Random().Next(1, 1000)}",
                Amount = message.Amount,
                CustomerEmail = message.CustomerEmail,
                Status = PaymentConstants.StatusCompleted,
                Timestamp = DateTime.UtcNow
            };

            await _paymentRepository.CreatePaymentAsync(payment, context.CancellationToken);
            _logger.LogInformation("Payment created for OrderId={OrderId} with PaymentId={PaymentId}", payment.OrderId, payment.Id);

            // Publish PaymentSucceededEvent
            var paymentSucceededEvent = new PaymentSucceededEvent
            {
                OrderId = payment.OrderId,
                PaymentId = payment.Id,
                Amount = payment.Amount,
                Timestamp = payment.Timestamp
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(15)); // Hang up after 10 seconds like a circuit breaker

            await context.Publish(paymentSucceededEvent, cts.Token);
            _logger.LogInformation("PaymentSucceededEvent published for OrderId={OrderId}, PaymentId={PaymentId}", payment.OrderId, payment.Id);
        }
    }
}
