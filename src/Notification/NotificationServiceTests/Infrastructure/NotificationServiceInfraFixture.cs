using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Domain.Entities;
using NotificationService.Infra.ApiContext;
using NotificationService.Infra.Repository;
using Shouldly;

namespace NotificationServiceTests.Infrastructure
{
    public class NotificationServiceInfraFixture : IDisposable
    {
        private readonly NotificationServiceDbContext _dbContext;
        private readonly NotificationRepository _repository;
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<NotificationRepository>> _mockLogger;

        public NotificationServiceInfraFixture()
        {
            var options = new DbContextOptionsBuilder<NotificationServiceDbContext>()
                .UseInMemoryDatabase(databaseName: $"NotificationTestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new NotificationServiceDbContext(options);
            _mockLogger = new Mock<ILogger<NotificationRepository>>();
            _repository = new NotificationRepository(_dbContext, _mockLogger.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task CreateNotificationAsync_Should_Add_Notification_To_Database()
        {
            var notification = _fixture.Build<Notification>()
                .With(n => n.Id, 1)
                .With(n => n.OrderId, 100)
                .With(n => n.PaymentId, "PAY-TEST-001")
                .With(n => n.Amount, 500.50m)
                .Create();

            await _repository.CreateNotificationAsync(notification, CancellationToken.None);

            var savedNotification = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id);
            savedNotification.ShouldNotBeNull();
            savedNotification.Id.ShouldBe(notification.Id);
        }

        [Fact]
        public async Task CreateNotificationAsync_Should_Throw_Exception_When_Notification_Is_Null()
        {
            await Should.ThrowAsync<ArgumentNullException>(async () =>
            {
                await _repository.CreateNotificationAsync(null, CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetNotificationsAsync_Should_Return_All_Notifications()
        {
            var notification1 = _fixture.Build<Notification>()
                .With(n => n.Id, 2)
                .With(n => n.OrderId, 101)
                .With(n => n.PaymentId, "PAY-TEST-002")
                .With(n => n.Amount, 200.00m)
                .Create();

            var notification2 = _fixture.Build<Notification>()
                .With(n => n.Id, 3)
                .With(n => n.OrderId, 102)
                .With(n => n.PaymentId, "PAY-TEST-003")
                .With(n => n.Amount, 300.00m)
                .Create();

            await _dbContext.Notifications.AddRangeAsync(notification1, notification2);
            await _dbContext.SaveChangesAsync();

            var notifications = await _repository.GetNotificationsAsync(CancellationToken.None);

            notifications.ShouldNotBeNull();
            notifications.Count.ShouldBe(2);
            notifications.ShouldContain(n => n.Id == 2);
            notifications.ShouldContain(n => n.Id == 3);
        }

        [Fact]
        public async Task GetNotificationsAsync_Should_Return_Empty_List_When_No_Notifications()
        {
            var notifications = await _repository.GetNotificationsAsync(CancellationToken.None);

            notifications.ShouldNotBeNull();
            notifications.ShouldBeEmpty();
        }

        [Fact]
        public async Task CreateNotificationAsync_Should_Save_All_Notification_Properties()
        {
            var timestamp = new DateTime(2024, 6, 15, 14, 30, 0);
            var notification = new Notification
            {
                Id = 4,
                OrderId = 103,
                PaymentId = "PAY-TEST-004",
                Amount = 150.75m,
                Timestamp = timestamp
            };

            await _repository.CreateNotificationAsync(notification, CancellationToken.None);

            var savedNotification = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == 4);
            savedNotification.ShouldNotBeNull();
            savedNotification.OrderId.ShouldBe(103);
            savedNotification.PaymentId.ShouldBe("PAY-TEST-004");
            savedNotification.Amount.ShouldBe(150.75m);
            savedNotification.Timestamp.ShouldBe(timestamp);
        }

        [Fact]
        public async Task GetNotificationsAsync_Should_Not_Track_Entities()
        {
            var notification = _fixture.Build<Notification>()
                .With(n => n.Id, 5)
                .With(n => n.OrderId, 104)
                .With(n => n.PaymentId, "PAY-TEST-005")
                .With(n => n.Amount, 400.00m)
                .Create();

            await _dbContext.Notifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();

            var notifications = await _repository.GetNotificationsAsync(CancellationToken.None);

            var trackedEntities = _dbContext.ChangeTracker.Entries<Notification>().Count();
            trackedEntities.ShouldBe(1);

            var retrievedNotification = notifications.First();
            var isTracked = _dbContext.Entry(retrievedNotification).State != EntityState.Detached;
            isTracked.ShouldBeFalse();
        }

        public void Dispose()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}
