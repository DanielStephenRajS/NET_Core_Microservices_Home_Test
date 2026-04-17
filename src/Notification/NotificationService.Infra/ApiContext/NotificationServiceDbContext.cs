using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infra.ApiContext
{
    public class NotificationServiceDbContext : DbContext
    {
        public NotificationServiceDbContext(DbContextOptions<NotificationServiceDbContext> options) : base(options)
        {
        }
        public DbSet<Domain.Entities.Notification> Notifications { get; set; }
    }
}
