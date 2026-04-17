using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentService.App.Features.Queries;
using PaymentService.Domain.Models;
using PaymentServiceApi.Controller;
using Shouldly;

namespace PaymentServiceTests.Presentation
{
    public class PaymentServiceControllerFixture
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<PaymentController>> _mockLogger;
        private readonly PaymentController _controller;
        private readonly MockDataProvider.MockDataProvider _mockDataProvider;
        private readonly IFixture _fixture;

        public PaymentServiceControllerFixture()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<PaymentController>>();
            _controller = new PaymentController(_mockLogger.Object, _mockMediator.Object);
            _mockDataProvider = new MockDataProvider.MockDataProvider();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task GetPaymentDetails_Should_Return_Ok_With_Payments()
        {
            var expectedPayments = _mockDataProvider.GetMockPaymentResponses();
            _mockMediator.Setup(x => x.Send(It.IsAny<PaymentServiceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPayments);

            var result = await _controller.GetPaymentDetails();

            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.Value.ShouldBe(expectedPayments);
        }

        [Fact]
        public async Task GetPaymentDetails_Should_Return_Ok_With_Empty_List_When_No_Payments()
        {
            _mockMediator.Setup(x => x.Send(It.IsAny<PaymentServiceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PaymentServiceResponse>());

            var result = await _controller.GetPaymentDetails();

            var okResult = result.ShouldBeOfType<OkObjectResult>();
            var payments = okResult.Value as List<PaymentServiceResponse>;
            payments.ShouldNotBeNull();
            payments.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetPaymentDetails_Should_Call_Mediator_Once()
        {
            _mockMediator.Setup(x => x.Send(It.IsAny<PaymentServiceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PaymentServiceResponse>());

            await _controller.GetPaymentDetails();

            _mockMediator.Verify(x => x.Send(It.IsAny<PaymentServiceQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
