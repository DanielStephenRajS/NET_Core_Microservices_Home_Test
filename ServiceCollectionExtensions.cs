namespace OrderService.App.ServiceRegistration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Payment).Assembly);
            });

            return services;
        }
    }
}
