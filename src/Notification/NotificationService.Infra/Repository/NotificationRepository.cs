using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;
using NotificationService.Infra.ApiContext;

namespace NotificationService.Infra.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationServiceDbContext _dbContext;
        private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(NotificationServiceDbContext dbContext, ILogger<NotificationRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task CreateNotificationAsync(Notification notification, CancellationToken cancellationToken)
        {
            if (notification == null)
            {
                _logger.LogError("Attempted to create a notification with null value.");
                throw new ArgumentNullException(nameof(notification));
            }
            await _dbContext.Notifications.AddAsync(notification, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Notification>> GetNotificationsAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Notifications.AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}