using OrderService.App.Features.Queries;
using OrderService.App.Features.Command;
using OrderService.App.AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;

namespace OrderService.App.ServiceRegistration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(OrderServiceQueryHandler).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(OrderServiceCommandHandler).Assembly);
            });

            services.AddAutoMapper(typeof(AppAutoMapperProfile).Assembly);

            return services;
        }
    }
}
