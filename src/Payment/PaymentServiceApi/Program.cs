using MassTransit;
using PaymentService.App.ServiceRegistration;
using Microsoft.EntityFrameworkCore;
using PaymentService.Infra.ApiContext;
using PaymentService.Domain.Interfaces;
using PaymentService.Infra.Repository;
using PaymentService.App.Features.Consumer;
using PaymentServiceApi.Configuration;
using PaymentServiceApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Bind configuration settings
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection(RabbitMQSettings.SectionName));

// Swagger/OpenAPI configuration
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

// Health checks
builder.Services.AddHealthChecks();

// service registration
builder.Services.AddApplicationServices();
builder.Services.AddScoped<IPaymentRepository,PaymentRepository>();

// Use EF Core In-Memory database
var databaseName = builder.Configuration.GetConnectionString("PaymentServiceDb") ?? "PaymentServiceDb";
builder.Services.AddDbContext<PaymentServiceDbContext>(options =>
     options.UseInMemoryDatabase(databaseName)
);

// Mass Transit with RabbitMQ configuration
var rabbitMQSettings = builder.Configuration.GetSection(RabbitMQSettings.SectionName).Get<RabbitMQSettings>();
if (rabbitMQSettings == null)
{
    throw new InvalidOperationException("RabbitMQ configuration is missing. Please configure RabbitMQ settings in appsettings.json");
}

builder.Services.AddMassTransit(x =>
{
    // Added consumer
    x.AddConsumer<OrderCreatedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentService API V1");
        options.RoutePrefix = string.Empty;
    });
}

//Middleware
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

