using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.App.Features.Queries;
using NotificationService.Domain.Models;
using NotificationServiceApi.Controller;
using Shouldly;

namespace NotificationServiceTests.Presentation
{
    public class NotificationServiceControllerFixture
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<NotificationController>> _mockLogger;
        private readonly NotificationController _controller;
        private readonly MockDataProvider.MockDataProvider _mockDataProvider;
        private readonly IFixture _fixture;

        public NotificationServiceControllerFixture()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<NotificationController>>();
            _controller = new NotificationController(_mockLogger.Object, _mockMediator.Object);
            _mockDataProvider = new MockDataProvider.MockDataProvider();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task GetNotifications_Should_Return_Ok_With_Notifications()
        {
            var expectedNotifications = _mockDataProvider.GetMockNotificationResponses();
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNotificationServiceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedNotifications);

            var result = await _controller.GetNotifications();

            var okResult = result.ShouldBeOfType<OkObjectResult>();
            okResult.Value.ShouldBe(expectedNotifications);
        }

        [Fact]
        public async Task GetNotifications_Should_Return_Ok_With_Empty_List_When_No_Notifications()
        {
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNotificationServiceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NotificationResponse>());

            var result = await _controller.GetNotifications();

            var okResult = result.ShouldBeOfType<OkObjectResult>();
            var notifications = okResult.Value as List<NotificationResponse>;
            notifications.ShouldNotBeNull();
            notifications.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetNotifications_Should_Call_Mediator_Once()
        {
            _mockMediator.Setup(x => x.Send(It.IsAny<GetNotificationServiceQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NotificationResponse>());

            await _controller.GetNotifications();

            _mockMediator.Verify(x => x.Send(It.IsAny<GetNotificationServiceQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
