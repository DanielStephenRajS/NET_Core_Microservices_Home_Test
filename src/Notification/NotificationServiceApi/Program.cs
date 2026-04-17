using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Interfaces;
using NotificationService.Infra.ApiContext;
using NotificationService.Infra.Repository;
using NotificationService.App.ServiceRegistration;
using NotificationService.App.Features.Consumer;
using NotificationServiceApi.Configuration;
using NotificationServiceApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Bind configuration settings
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection(RabbitMQSettings.SectionName));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

// service registration
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Register services from the application layer
builder.Services.AddApplicationServices();

builder.Services.AddControllers();

// Health checks
builder.Services.AddHealthChecks();

// Use EF Core In-Memory database
var databaseName = builder.Configuration.GetConnectionString("NotificationServiceDb") ?? "NotificationServiceDb";
builder.Services.AddDbContext<NotificationServiceDbContext>(options =>
    options.UseInMemoryDatabase(databaseName));

// Register MassTransit and RabbitMQ with configuration
var rabbitMQSettings = builder.Configuration.GetSection(RabbitMQSettings.SectionName).Get<RabbitMQSettings>();

if (rabbitMQSettings == null)
{
    throw new InvalidOperationException("RabbitMQ configuration is missing. Please configure RabbitMQ settings in appsettings.json");
}

builder.Services.AddMassTransit(mt =>
{
    // Added the consumer
    mt.AddConsumer<PaymentSucceededConsumer>();
    mt.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMQSettings.Host, rabbitMQSettings.VirtualHost, h =>
        {
            h.Username(rabbitMQSettings.Username);
            h.Password(rabbitMQSettings.Password);
        });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "NotificationService API V1");
        options.RoutePrefix = string.Empty;
    });
}

//Middleware
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
