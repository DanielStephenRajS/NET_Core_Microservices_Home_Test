using Microsoft.Extensions.DependencyInjection;
using NotificationService.App.Features.Queries;

namespace NotificationService.App.ServiceRegistration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(GetNotificationServiceQueryHandler).Assembly);
            });

            // Auto mapper registration
            services.AddAutoMapper(typeof(GetNotificationServiceQueryHandler).Assembly);

            return services;
        }
    }
}
