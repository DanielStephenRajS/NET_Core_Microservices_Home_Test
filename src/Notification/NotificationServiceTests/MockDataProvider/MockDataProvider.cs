using NotificationService.Domain.Entities;
using NotificationService.Domain.Models;

namespace NotificationServiceTests.MockDataProvider
{
    internal class MockDataProvider
    {
        public List<Notification> GetMockNotifications()
        {
            return new List<Notification>
            {
                new Notification
                {
                    Id = 1,
                    OrderId = 1,
                    PaymentId = "PAY-001",
                    Amount = 100.50m,
                    Timestamp = new DateTime(2024, 1, 1, 10, 0, 0)
                },
                new Notification
                {
                    Id = 2,
                    OrderId = 2,
                    PaymentId = "PAY-002",
                    Amount = 250.75m,
                    Timestamp = new DateTime(2024, 1, 2, 15, 30, 0)
                },
                new Notification
                {
                    Id = 3,
                    OrderId = 3,
                    PaymentId = "PAY-003",
                    Amount = 75.00m,
                    Timestamp = new DateTime(2024, 1, 3, 9, 15, 0)
                }
            };
        }

        public List<NotificationResponse> GetMockNotificationResponses()
        {
            return new List<NotificationResponse>
            {
                new NotificationResponse
                {
                    NotificationId = 1,
                    OrderId = 1,
                    PaymentId = "PAY-001",
                    Amount = 100.50m,
                    Timestamp = new DateTime(2024, 1, 1, 10, 0, 0)
                },
                new NotificationResponse
                {
                    NotificationId = 2,
                    OrderId = 2,
                    PaymentId = "PAY-002",
                    Amount = 250.75m,
                    Timestamp = new DateTime(2024, 1, 2, 15, 30, 0)
                },
                new NotificationResponse
                {
                    NotificationId = 3,
                    OrderId = 3,
                    PaymentId = "PAY-003",
                    Amount = 75.00m,
                    Timestamp = new DateTime(2024, 1, 3, 9, 15, 0)
                }
            };
        }
    }
}
