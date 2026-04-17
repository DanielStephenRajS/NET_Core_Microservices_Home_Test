using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task CreateNotificationAsync(Notification notification, CancellationToken cancellationToken);
        Task<List<Notification>> GetNotificationsAsync(CancellationToken cancellationToken);
    }
}
