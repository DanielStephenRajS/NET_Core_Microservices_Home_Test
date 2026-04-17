
using Microsoft.Extensions.DependencyInjection;
using PaymentService.App.Features.Queries;

namespace PaymentService.App.ServiceRegistration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(PaymentServiceQueryHandler).Assembly);
            });

            services.AddAutoMapper(typeof(PaymentServiceQueryHandler).Assembly);
            return services;
        }
    }
}
