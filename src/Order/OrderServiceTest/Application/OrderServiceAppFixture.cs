using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.App.Features.Command;
using OrderService.App.Features.Queries;
using OrderService.App.Features.Queries.Models;
using OrderService.Domain.Entities;
using OrderService.Domain.NewFolder;
using ServiceContracts;
using Shouldly;

namespace OrderServiceTest.Application
{
    public class OrderServiceAppFixture
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<OrderServiceQueryHandler>> _queryLoggerMock;
        private readonly Mock<ILogger<OrderServiceCommandHandler>> _commandLoggerMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly MockDataProvider.MockDataProvider _mockDataProvider;

        public OrderServiceAppFixture()
        {
            _mapperMock = new Mock<IMapper>();
            _queryLoggerMock = new Mock<ILogger<OrderServiceQueryHandler>>();
            _commandLoggerMock = new Mock<ILogger<OrderServiceCommandHandler>>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _mockDataProvider = new MockDataProvider.MockDataProvider();
        }

        [Fact]
        public async Task QueryHandler_Should_Return_All_Orders()
        {
            var mockOrders = _mockDataProvider.GetMockOrders();
            var mockResponses = _mockDataProvider.GetMockOrderResponses();

            _orderRepositoryMock.Setup(repo => repo.GetOrdersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockOrders);

            _mapperMock.Setup(m => m.Map<List<OrderServiceResponse>>(It.IsAny<List<Orders>>()))
                .Returns(mockResponses);

            var handler = new OrderServiceQueryHandler(_queryLoggerMock.Object, _orderRepositoryMock.Object, _mapperMock.Object);
            var query = new OrderServiceQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            result.ShouldNotBeNull();
            result.Count.ShouldBe(3);
            result[0].OrderId.ShouldBe(mockResponses[0].OrderId);
            result[0].CustomerEmail.ShouldBe(mockResponses[0].CustomerEmail);
        }

        [Fact]
        public async Task QueryHandler_Should_Return_Empty_List_When_No_Orders()
        {
            _orderRepositoryMock.Setup(repo => repo.GetOrdersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Orders>());

            _mapperMock.Setup(m => m.Map<List<OrderServiceResponse>>(It.IsAny<List<Orders>>()))
                .Returns(new List<OrderServiceResponse>());

            var handler = new OrderServiceQueryHandler(_queryLoggerMock.Object, _orderRepositoryMock.Object, _mapperMock.Object);
            var query = new OrderServiceQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task QueryHandler_Should_Return_Empty_List_When_Repository_Returns_Null()
        {
            _orderRepositoryMock.Setup(repo => repo.GetOrdersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<Orders>?)null);

            var handler = new OrderServiceQueryHandler(_queryLoggerMock.Object, _orderRepositoryMock.Object, _mapperMock.Object);
            var query = new OrderServiceQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task CommandHandler_Should_Create_Order_Successfully()
        {
            var orderId = 123;
            var request = new OrderServiceRequest
            {
                Amount = 150.00m,
                CustomerEmail = "test@example.com",
                Status = "Pending",
                CreatedDate = DateTime.UtcNow
            };

            _orderRepositoryMock.Setup(repo => repo.CreateOrderAsync(It.IsAny<Orders>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(orderId);

            _publishEndpointMock.Setup(p => p.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new OrderServiceCommandHandler(_commandLoggerMock.Object, _orderRepositoryMock.Object, _publishEndpointMock.Object);
            var command = new QrderServiceCommand { OrderServiceRequest = request };

            var result = await handler.Handle(command, CancellationToken.None);

            result.ShouldBe(orderId);
            _orderRepositoryMock.Verify(repo => repo.CreateOrderAsync(It.IsAny<Orders>(), It.IsAny<CancellationToken>()), Times.Once);
            _publishEndpointMock.Verify(p => p.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CommandHandler_Should_Throw_Exception_When_Request_Is_Null()
        {
            var handler = new OrderServiceCommandHandler(_commandLoggerMock.Object, _orderRepositoryMock.Object, _publishEndpointMock.Object);

            await Should.ThrowAsync<ArgumentNullException>(async () => 
                await handler.Handle(null!, CancellationToken.None));
        }

        [Fact]
        public async Task CommandHandler_Should_Publish_Event_After_Creating_Order()
        {
            var orderId = 456;
            var request = new OrderServiceRequest
            {
                Amount = 200.50m,
                CustomerEmail = "customer@example.com",
                Status = "Completed",
                CreatedDate = DateTime.UtcNow
            };

            _orderRepositoryMock.Setup(repo => repo.CreateOrderAsync(It.IsAny<Orders>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(orderId);

            _publishEndpointMock.Setup(p => p.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new OrderServiceCommandHandler(_commandLoggerMock.Object, _orderRepositoryMock.Object, _publishEndpointMock.Object);
            var command = new QrderServiceCommand { OrderServiceRequest = request };

            var result = await handler.Handle(command, CancellationToken.None);

            result.ShouldBe(orderId);
            _publishEndpointMock.Verify(
                p => p.Publish(
                    It.Is<OrderCreatedEvent>(e => 
                        e.OrderId == orderId && 
                        e.CustomerEmail == request.CustomerEmail && 
                        e.Amount == request.Amount), 
                    It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}
