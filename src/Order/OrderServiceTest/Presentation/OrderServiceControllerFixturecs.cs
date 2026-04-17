using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.App.Features.Command;
using OrderService.App.Features.Queries;
using OrderService.App.Features.Queries.Models;
using OrderServiceApi.Controller;
using Shouldly;

namespace OrderServiceTest.Presentation
{
    public class OrderServiceControllerFixturecs
    {
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<OrderController>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly OrderController _controller;

        public OrderServiceControllerFixturecs()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _loggerMock = new Mock<ILogger<OrderController>>();
            _mediatorMock = new Mock<IMediator>();
            _controller = new OrderController(_loggerMock.Object, _mediatorMock.Object);
        }

        [Fact]
        public async Task GetOrderDetails_Should_Return_Ok_With_Orders()
        {
            var expectedOrders = _fixture.Build<OrderServiceResponse>()
                .With(x => x.OrderId, _fixture.Create<int>())
                .With(x => x.Amount, _fixture.Create<decimal>())
                .With(x => x.CustomerEmail, _fixture.Create<string>())
                .With(x => x.Status, _fixture.Create<string>())
                .With(x => x.CreatedDate, _fixture.Create<DateTime>())
                .CreateMany(3)
                .ToList();

            _mediatorMock.Setup(m => m.Send(It.IsAny<OrderServiceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrders);

            var result = await _controller.GetOrderDetails();

            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);
            var orders = okResult.Value.ShouldBeOfType<List<OrderServiceResponse>>();
            orders.Count.ShouldBe(3);
            _mediatorMock.Verify(m => m.Send(It.IsAny<OrderServiceQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetOrderDetails_Should_Return_Ok_With_Empty_List_When_No_Orders()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<OrderServiceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OrderServiceResponse>());

            var result = await _controller.GetOrderDetails();

            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);
            var orders = okResult.Value.ShouldBeOfType<List<OrderServiceResponse>>();
            orders.ShouldBeEmpty();
        }

        [Fact]
        public async Task CreateOrder_Should_Return_Ok_With_OrderId()
        {
            var expectedOrderId = _fixture.Create<int>();
            var orderRequest = _fixture.Build<OrderServiceRequest>()
                .With(x => x.Amount, _fixture.Create<decimal>())
                .With(x => x.CustomerEmail, _fixture.Create<string>())
                .With(x => x.Status, _fixture.Create<string>())
                .With(x => x.CreatedDate, _fixture.Create<DateTime>())
                .Create();

            _mediatorMock.Setup(m => m.Send(It.IsAny<QrderServiceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrderId);

            var result = await _controller.CreateOrder(orderRequest);

            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.StatusCode.ShouldBe(200);
            var orderId = okResult.Value.ShouldBeOfType<int>();
            orderId.ShouldBe(expectedOrderId);
            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<QrderServiceCommand>(cmd => cmd.OrderServiceRequest == orderRequest), 
                    It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task CreateOrder_Should_Send_Command_With_Correct_Request()
        {
            var expectedOrderId = _fixture.Create<int>();
            var orderRequest = _fixture.Build<OrderServiceRequest>()
                .With(x => x.Amount, 150.50m)
                .With(x => x.CustomerEmail, "test@example.com")
                .With(x => x.Status, "Pending")
                .With(x => x.CreatedDate, DateTime.UtcNow)
                .Create();

            _mediatorMock.Setup(m => m.Send(It.IsAny<QrderServiceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrderId);

            var result = await _controller.CreateOrder(orderRequest);

            result.ShouldNotBeNull();
            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<QrderServiceCommand>(cmd => 
                        cmd.OrderServiceRequest.Amount == orderRequest.Amount &&
                        cmd.OrderServiceRequest.CustomerEmail == orderRequest.CustomerEmail &&
                        cmd.OrderServiceRequest.Status == orderRequest.Status), 
                    It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task GetOrderDetails_Should_Call_Mediator_Once()
        {
            var expectedOrders = _fixture.CreateMany<OrderServiceResponse>(2).ToList();

            _mediatorMock.Setup(m => m.Send(It.IsAny<OrderServiceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrders);

            await _controller.GetOrderDetails();

            _mediatorMock.Verify(m => m.Send(It.IsAny<OrderServiceQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_Should_Call_Mediator_Once()
        {
            var expectedOrderId = _fixture.Create<int>();
            var orderRequest = _fixture.Create<OrderServiceRequest>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<QrderServiceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrderId);

            await _controller.CreateOrder(orderRequest);

            _mediatorMock.Verify(m => m.Send(It.IsAny<QrderServiceCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
