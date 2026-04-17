using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.App.Features.Consumer;
using NotificationService.App.Features.Queries;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Models;
using ServiceContracts;
using Shouldly;

namespace NotificationServiceTests.Application
{
    public class NotificationServiceAppFixture
    {
        private readonly Mock<INotificationRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<GetNotificationServiceQueryHandler>> _mockQueryLogger;
        private readonly Mock<ILogger<PaymentSucceededConsumer>> _mockConsumerLogger;
        private readonly GetNotificationServiceQueryHandler _queryHandler;
        private readonly PaymentSucceededConsumer _consumer;
        private readonly MockDataProvider.MockDataProvider _mockDataProvider;
        private readonly IFixture _fixture;

        public NotificationServiceAppFixture()
        {
            _mockRepository = new Mock<INotificationRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockQueryLogger = new Mock<ILogger<GetNotificationServiceQueryHandler>>();
            _mockConsumerLogger = new Mock<ILogger<PaymentSucceededConsumer>>();
            _queryHandler = new GetNotificationServiceQueryHandler(_mockQueryLogger.Object, _mockRepository.Object, _mockMapper.Object);
            _consumer = new PaymentSucceededConsumer(_mockConsumerLogger.Object, _mockRepository.Object);
            _mockDataProvider = new MockDataProvider.MockDataProvider();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task QueryHandler_Should_Return_All_Notifications()
        {
            var mockNotifications = _mockDataProvider.GetMockNotifications();
            var mockResponses = _mockDataProvider.GetMockNotificationResponses();

            _mockRepository.Setup(x => x.GetNotificationsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockNotifications);
            _mockMapper.Setup(x => x.Map<List<NotificationResponse>>(mockNotifications))
                .Returns(mockResponses);

            var result = await _queryHandler.Handle(new GetNotificationServiceQuery(), CancellationToken.None);

            result.ShouldNotBeNull();
            result.Count.ShouldBe(3);
            result.ShouldBe(mockResponses);
        }

        [Fact]
        public async Task QueryHandler_Should_Return_Empty_List_When_No_Notifications()
        {
            _mockRepository.Setup(x => x.GetNotificationsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Notification>());
            _mockMapper.Setup(x => x.Map<List<NotificationResponse>>(It.IsAny<List<Notification>>()))
                .Returns(new List<NotificationResponse>());

            var result = await _queryHandler.Handle(new GetNotificationServiceQuery(), CancellationToken.None);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task QueryHandler_Should_Map_Notifications_Correctly()
        {
            var mockNotifications = _mockDataProvider.GetMockNotifications();
            var mockResponses = _mockDataProvider.GetMockNotificationResponses();

            _mockRepository.Setup(x => x.GetNotificationsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockNotifications);
            _mockMapper.Setup(x => x.Map<List<NotificationResponse>>(mockNotifications))
                .Returns(mockResponses);

            await _queryHandler.Handle(new GetNotificationServiceQuery(), CancellationToken.None);

            _mockMapper.Verify(x => x.Map<List<NotificationResponse>>(mockNotifications), Times.Once);
        }

        [Fact]
        public async Task Consumer_Should_Create_Notification_From_PaymentSucceededEvent()
        {
            var paymentEvent = _fixture.Build<PaymentSucceededEvent>()
                .With(e => e.OrderId, 100)
                .With(e => e.PaymentId, "PAY-TEST-001")
                .With(e => e.Amount, 500.00m)
                .With(e => e.Timestamp, DateTime.UtcNow)
                .Create();

            var mockContext = new Mock<ConsumeContext<PaymentSucceededEvent>>();
            mockContext.Setup(x => x.Message).Returns(paymentEvent);
            mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

            _mockRepository.Setup(x => x.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _consumer.Consume(mockContext.Object);

            _mockRepository.Verify(x => x.CreateNotificationAsync(
                It.Is<Notification>(n => n.OrderId == 100 && n.PaymentId == "PAY-TEST-001" && n.Amount == 500.00m),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Consumer_Should_Throw_Exception_When_Context_Is_Null()
        {
            await Should.ThrowAsync<ArgumentNullException>(async () =>
            {
                await _consumer.Consume(null);
            });
        }

        [Fact]
        public async Task Consumer_Should_Process_Event_With_All_Properties()
        {
            var timestamp = new DateTime(2024, 6, 15, 10, 30, 0);
            var paymentEvent = new PaymentSucceededEvent
            {
                OrderId = 200,
                PaymentId = "PAY-TEST-002",
                Amount = 750.50m,
                Timestamp = timestamp
            };

            var mockContext = new Mock<ConsumeContext<PaymentSucceededEvent>>();
            mockContext.Setup(x => x.Message).Returns(paymentEvent);
            mockContext.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

            Notification capturedNotification = null;
            _mockRepository.Setup(x => x.CreateNotificationAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
                .Callback<Notification, CancellationToken>((n, ct) => capturedNotification = n)
                .Returns(Task.CompletedTask);

            await _consumer.Consume(mockContext.Object);

            capturedNotification.ShouldNotBeNull();
            capturedNotification.OrderId.ShouldBe(200);
            capturedNotification.PaymentId.ShouldBe("PAY-TEST-002");
            capturedNotification.Amount.ShouldBe(750.50m);
            capturedNotification.Timestamp.ShouldBe(timestamp);
        }
    }
}
