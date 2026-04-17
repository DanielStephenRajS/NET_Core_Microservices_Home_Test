using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentService.App.Features.Consumer;
using PaymentService.App.Features.Queries;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Models;
using ServiceContracts;
using Shouldly;

namespace PaymentServiceTests.Application
{
    public class PaymentServiceAppFixture
    {
        private readonly IFixture _fixture;
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<PaymentServiceQueryHandler>> _queryLoggerMock;
        private readonly Mock<ILogger<OrderCreatedConsumer>> _consumerLoggerMock;
        private readonly MockDataProvider.MockDataProvider _mockDataProvider;

        public PaymentServiceAppFixture()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _mapperMock = new Mock<IMapper>();
            _queryLoggerMock = new Mock<ILogger<PaymentServiceQueryHandler>>();
            _consumerLoggerMock = new Mock<ILogger<OrderCreatedConsumer>>();
            _mockDataProvider = new MockDataProvider.MockDataProvider();
        }

        [Fact]
        public async Task QueryHandler_Should_Return_All_Payments()
        {
            var mockPayments = _mockDataProvider.GetMockPayments();
            var mockResponses = _mockDataProvider.GetMockPaymentResponses();

            _paymentRepositoryMock.Setup(repo => repo.GetPaymentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockPayments);

            _mapperMock.Setup(m => m.Map<List<PaymentServiceResponse>>(It.IsAny<List<Payment>>()))
                .Returns(mockResponses);

            var handler = new PaymentServiceQueryHandler(_queryLoggerMock.Object, _paymentRepositoryMock.Object, _mapperMock.Object);
            var query = new PaymentServiceQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            result.ShouldNotBeNull();
            result.Count.ShouldBe(3);
            result[0].PaymentId.ShouldBe(mockResponses[0].PaymentId);
            result[0].CustomerEmail.ShouldBe(mockResponses[0].CustomerEmail);
            _paymentRepositoryMock.Verify(repo => repo.GetPaymentAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task QueryHandler_Should_Return_Empty_List_When_No_Payments()
        {
            _paymentRepositoryMock.Setup(repo => repo.GetPaymentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Payment>());

            _mapperMock.Setup(m => m.Map<List<PaymentServiceResponse>>(It.IsAny<List<Payment>>()))
                .Returns(new List<PaymentServiceResponse>());

            var handler = new PaymentServiceQueryHandler(_queryLoggerMock.Object, _paymentRepositoryMock.Object, _mapperMock.Object);
            var query = new PaymentServiceQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task QueryHandler_Should_Map_Payments_Correctly()
        {
            var mockPayments = _mockDataProvider.GetMockPayments();
            var mockResponses = _mockDataProvider.GetMockPaymentResponses();

            _paymentRepositoryMock.Setup(repo => repo.GetPaymentAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockPayments);

            _mapperMock.Setup(m => m.Map<List<PaymentServiceResponse>>(It.IsAny<List<Payment>>()))
                .Returns(mockResponses);

            var handler = new PaymentServiceQueryHandler(_queryLoggerMock.Object, _paymentRepositoryMock.Object, _mapperMock.Object);
            var query = new PaymentServiceQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            result.ShouldNotBeNull();
            _mapperMock.Verify(m => m.Map<List<PaymentServiceResponse>>(mockPayments), Times.Once);
        }

        [Fact]
        public async Task Consumer_Should_Create_Payment_From_OrderCreatedEvent()
        {
            var orderCreatedEvent = _fixture.Build<OrderCreatedEvent>()
                .With(x => x.OrderId, _fixture.Create<int>())
                .With(x => x.Amount, _fixture.Create<decimal>())
                .With(x => x.CustomerEmail, _fixture.Create<string>())
                .Create();

            var consumeContextMock = new Mock<ConsumeContext<OrderCreatedEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(orderCreatedEvent);
            consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
            consumeContextMock.Setup(x => x.Publish(It.IsAny<PaymentSucceededEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _paymentRepositoryMock.Setup(repo => repo.CreatePaymentAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var consumer = new OrderCreatedConsumer(_consumerLoggerMock.Object, _paymentRepositoryMock.Object);

            await consumer.Consume(consumeContextMock.Object);

            _paymentRepositoryMock.Verify(
                repo => repo.CreatePaymentAsync(
                    It.Is<Payment>(p => 
                        p.OrderId == orderCreatedEvent.OrderId &&
                        p.Amount == orderCreatedEvent.Amount &&
                        p.CustomerEmail == orderCreatedEvent.CustomerEmail &&
                        p.Status == "Completed"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Consumer_Should_Publish_PaymentSucceededEvent()
        {
            var orderCreatedEvent = _fixture.Build<OrderCreatedEvent>()
                .With(x => x.OrderId, 123)
                .With(x => x.Amount, 100.50m)
                .With(x => x.CustomerEmail, "test@example.com")
                .Create();

            var consumeContextMock = new Mock<ConsumeContext<OrderCreatedEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(orderCreatedEvent);
            consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
            consumeContextMock.Setup(x => x.Publish(It.IsAny<PaymentSucceededEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _paymentRepositoryMock.Setup(repo => repo.CreatePaymentAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var consumer = new OrderCreatedConsumer(_consumerLoggerMock.Object, _paymentRepositoryMock.Object);

            await consumer.Consume(consumeContextMock.Object);

            consumeContextMock.Verify(
                ctx => ctx.Publish(
                    It.Is<PaymentSucceededEvent>(e =>
                        e.OrderId == orderCreatedEvent.OrderId &&
                        e.Amount == orderCreatedEvent.Amount),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Consumer_Should_Process_Event_With_All_Properties()
        {
            var orderCreatedEvent = _fixture.Build<OrderCreatedEvent>()
                .With(x => x.OrderId, 456)
                .With(x => x.Amount, 250.99m)
                .With(x => x.CustomerEmail, "customer@test.com")
                .Create();

            var consumeContextMock = new Mock<ConsumeContext<OrderCreatedEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(orderCreatedEvent);
            consumeContextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
            consumeContextMock.Setup(x => x.Publish(It.IsAny<PaymentSucceededEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _paymentRepositoryMock.Setup(repo => repo.CreatePaymentAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var consumer = new OrderCreatedConsumer(_consumerLoggerMock.Object, _paymentRepositoryMock.Object);

            await consumer.Consume(consumeContextMock.Object);

            _paymentRepositoryMock.Verify(repo => repo.CreatePaymentAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
            consumeContextMock.Verify(ctx => ctx.Publish(It.IsAny<PaymentSucceededEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
