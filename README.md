# Order Processing System

A microservices-based order processing system built with .NET 10 that demonstrates event-driven architecture using RabbitMQ for asynchronous communication between services.

## Overview

This system handles the complete order lifecycle - from order creation to payment processing and customer notifications. It's designed with a microservices architecture where each service is independently deployable and communicates through message queues.

## Architecture

The system consists of four main components:

### Services

1. **Order Service** (Port 5020)
   - Handles order creation and management
   - Publishes `OrderCreatedEvent` when a new order is created
   - Uses CQRS pattern with MediatR for command/query separation
   - In-memory database for order storage

2. **Payment Service** (Port 5232)
   - Processes payments for orders
   - Consumes `OrderCreatedEvent` from Order Service
   - Publishes `PaymentProcessedEvent` after processing
   - In-memory database for payment records

3. **Notification Service** (Port 5196)
   - Sends notifications to customers
   - Consumes `PaymentProcessedEvent` from Payment Service
   - Handles email notifications (simulated)
   - In-memory database for notification logs

4. **API Gateway** (Port 5000)
   - Built with Ocelot
   - Single entry point for all client requests
   - Routes requests to appropriate microservices
   - Simplifies client interaction with the system

### Infrastructure

- **RabbitMQ** (Ports 5672, 15672)
  - Message broker for event-driven communication
  - Management UI available at http://localhost:15672
  - Default credentials: guest/guest

- **Service Contracts**
  - Shared library containing event definitions
  - Used by all services for consistent messaging

## Event Flow

Here's how an order flows through the system:

```
1. Client → API Gateway → Order Service
   POST /order-service/api/order/CreateOrder
   
2. Order Service creates order
   → Publishes OrderCreatedEvent to RabbitMQ
   
3. Payment Service consumes OrderCreatedEvent
   → Processes payment
   → Publishes PaymentProcessedEvent to RabbitMQ
   
4. Notification Service consumes PaymentProcessedEvent
   → Sends notification to customer
```

Each service operates independently. If Payment Service is down, Order Service continues working and the payment will be processed once Payment Service recovers.

## Project Structure

```
OrderProcessingSystem/
├── src/
│   ├── APIGateway/
│   │   └── ApiGateway/              # Ocelot API Gateway
│   ├── Contract/
│   │   └── ServiceContracts/        # Shared event contracts
│   ├── Order/
│   │   ├── OrderServiceApi/         # Order API & Controllers
│   │   ├── OrderService.App/        # Business logic & MediatR handlers
│   │   ├── OrderService.Domain/     # Domain entities & interfaces
│   │   ├── OrderService.Infra/      # Data access & repository
│   │   └── OrderServiceTest/        # Unit tests
│   ├── Payment/
│   │   ├── PaymentServiceApi/       # Payment API & Controllers
│   │   ├── PaymentService.App/      # Business logic & consumers
│   │   ├── PaymentService.Domain/   # Domain entities & interfaces
│   │   ├── PaymentService.Infra/    # Data access & repository
│   │   └── PaymentServiceTests/     # Unit tests
│   └── Notification/
│       ├── NotificationServiceApi/  # Notification API & Controllers
│       ├── NotificationService.App/ # Business logic & consumers
│       ├── NotificationService.Domain/ # Domain entities & interfaces
│       ├── NotificationService.Infra/ # Data access & repository
│       └── NotificationServiceTests/ # Unit tests
├── docker-compose.yml
└── README.md
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Visual Studio 2026 or VS Code

### Quick Start (Recommended - Works on All Laptops)

**Why CloudAMQP?** Some laptops have restrictions (no Docker, proxy issues, installation restrictions). CloudAMQP solves this by providing cloud-based RabbitMQ with a free tier.

#### 1. Setup CloudAMQP (One-time, Free)

1. Sign up at https://www.cloudamqp.com/
2. Create new instance → Choose **Little Lemur (Free)** → Region: **Tokyo** → Create
3. Configuration files are already updated with credentials

**AMQP (Advanced Message Queuing Protocol)** routes messages between services:
- Order Service → publishes OrderCreatedEvent → Payment Service consumes it
- Payment Service → publishes PaymentProcessedEvent → Notification Service consumes it
- Messages wait in queues if a service is down, ensuring reliability

#### 2. Run the Services

**Using Visual Studio:**
1. Right-click solution → **Configure Startup Projects**
2. Select **Multiple startup projects**
3. Set these to **Start**:
   - `OrderServiceApi`
   - `PaymentServiceApi`
   - `NotificationServiceApi`
   - `ApiGateway`
4. Press **F5**

**Access URLs:**
- API Gateway: http://localhost:5000
- Order Service: http://localhost:5020 (Swagger: http://localhost:5020/swagger)
- Payment Service: http://localhost:5232 (Swagger: http://localhost:5232/swagger)
- Notification Service: http://localhost:5196 (Swagger: http://localhost:5196/swagger)
- CloudAMQP Management: Login at cloudamqp.com

### Alternative: Using Local Docker

If you have Docker Desktop installed:

```bash
docker-compose up --build
```

Services will start on ports 5000-5002.

## API Endpoints

All requests go through API Gateway: `http://localhost:5000`

**Create Order:**
```http
POST http://localhost:5000/order-service/api/order/CreateOrder
Content-Type: application/json

{
  "amount": 110,
  "customerEmail": "danny@gmail.com",
  "status": "Pending",
  "createdDate": "2026-04-15T03:11:07.188Z"
}
```

**Get Data:**
- Orders: `GET http://localhost:5000/order-service/api/order/GetOrders`
- Payments: `GET http://localhost:5000/payment-service/api/payment/GetPayments`
- Notifications: `GET http://localhost:5000/notification-service/api/notification/GetNotifications`

**Health Checks:**
- `GET http://localhost:5000/order-service/health`
- `GET http://localhost:5000/payment-service/health`
- `GET http://localhost:5000/notification-service/health`

## Design Decisions

**Architecture:**
- CQRS with MediatR (Order Service)
- Event-Driven Architecture (async messaging)
- Clean Architecture (layered structure)
- Repository Pattern (data abstraction)

**Tech Stack:**
- MassTransit (RabbitMQ abstraction with retry & error handling)
- EF Core In-Memory DB (for simplicity, use SQL/PostgreSQL in production)
- Ocelot API Gateway
- CloudAMQP (managed RabbitMQ service)

**Note:** In production, replace in-memory DB with persistent storage, add authentication, and keep packages updated.

## Known Limitations & Future Improvements

**Current Limitations:**
- Code duplication (middleware across services)
- In-memory storage (data lost on restart)
- Basic error handling
- Package vulnerabilities (AutoMapper)

**Future Enhancements:**
- Persistent databases (SQL Server, PostgreSQL, MongoDB)
- JWT authentication & authorization
- Saga pattern for complex workflows
- Observability (Serilog, OpenTelemetry, Prometheus)
- Resilience patterns (Polly circuit breakers, retry policies)
- Integration & load tests
- Kubernetes deployment with CI/CD

## Testing

```bash
# Run all tests
dotnet test

# Or run individually
cd src\Order\OrderServiceTest && dotnet test
cd src\Payment\PaymentServiceTests && dotnet test
cd src\Notification\NotificationServiceTests && dotnet test
```

## Monitoring

- **CloudAMQP UI**: Login at cloudamqp.com → View queues, exchanges, messages
- **Health Endpoints**: `/health` on each service

## Troubleshooting

**Services can't connect:**
- Check `appsettings.json` RabbitMQ settings
- Verify CloudAMQP instance is running
- Check internet connectivity

**API Gateway returns 503:**
- Ensure all services are running
- Check port configurations

**Messages not flowing:**
- Check CloudAMQP UI for messages in queues
- Check service logs for errors

## Contributing

1. Follow the existing code structure and patterns
2. Add tests for new features
3. Update this README if adding new services or changing architecture
4. Ensure all packages are vulnerability-free before committing

---

**Note**: This system is built for educational purposes and demonstrates microservices patterns. It is NOT production-ready and requires significant enhancements for production use, particularly around security, data persistence, observability, and resilience.
