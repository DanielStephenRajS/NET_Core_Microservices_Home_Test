using Microsoft.EntityFrameworkCore;
using OrderService.Infra.ApiContext;
using OrderService.Infra.Repository;
using OrderService.App.ServiceRegistration;
using MassTransit;
using OrderService.Domain.NewFolder;
using OrderServiceApi.Configuration;
using OrderServiceApi.Middlewares;
using MassTransit.UsageTelemetry;

var builder = WebApplication.CreateBuilder(args);

// Get Configuration from appsettings.json
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection(RabbitMQSettings.SectionName));

// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

// Register services from the application layer
builder.Services.AddApplicationServices();

// Health checks
builder.Services.AddHealthChecks();

// Use EF Core In-Memory database for testing.
var databaseName = builder.Configuration.GetConnectionString("OrderServiceDb") ?? "OrderServiceDb";
builder.Services.AddDbContext<OrderServiceContext>(options =>
    options.UseInMemoryDatabase(databaseName));

// Register infra repository
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Mass Transit with RabbitMQ configuration
var rabbitMQSettings = builder.Configuration.GetSection(RabbitMQSettings.SectionName).Get<RabbitMQSettings>();
if (rabbitMQSettings == null)
{
    throw new InvalidOperationException("RabbitMQ configuration is missing. Please configure RabbitMQ settings in appsettings.json");
}

builder.Services.AddMassTransit(x =>
{
   x.UsingRabbitMq((context, cfg) =>
   {
       cfg.Host(rabbitMQSettings.Host, (ushort)rabbitMQSettings.Port, rabbitMQSettings.VirtualHost, h =>
       {
           h.Username(rabbitMQSettings.Username);
           h.Password(rabbitMQSettings.Password);

           if (rabbitMQSettings.UseSsl)
           {
               h.UseSsl(s =>
               {
                   s.Protocol = System.Security.Authentication.SslProtocols.Tls12;
                   s.ServerName = rabbitMQSettings.Host;
               });
           }
       });
       cfg.ConfigureEndpoints(context);
   });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
        app.UseSwagger();
        // Serve the Swagger UI at the app root (https://localhost:<port>/)
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService API V1");
            options.RoutePrefix = string.Empty;
        });
}

//Middle ware
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();