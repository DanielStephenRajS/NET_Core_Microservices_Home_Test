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
- Docker Desktop (for containerized deployment)
- Visual Studio 2026 or VS Code (optional, for development)

### Running Locally (Without Docker)

#### Option 1: Using Visual Studio (Recommended)

This is the simplest way to run all services locally.

1. **Start RabbitMQ**
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

2. **Configure Multiple Startup Projects in Visual Studio**
   - Right-click on the solution in Solution Explorer
   - Select **"Configure Startup Projects..."**
   - Choose **"Multiple startup projects"**
   - Set the following projects to **"Start"**:
     - `OrderServiceApi`
     - `PaymentServiceApi`
     - `NotificationServiceApi`
     - `ApiGateway`
   - Click **OK**

3. **Run the Solution**
   - Press **F5** or click the **Start** button
   - Visual Studio will launch all four services simultaneously
   - Each service will open in its own console window

4. **Verify services are running**:
   - Order Service: http://localhost:5020/health
   - Payment Service: http://localhost:5232/health
   - Notification Service: http://localhost:5196/health
   - API Gateway: http://localhost:5000
   - RabbitMQ Management: http://localhost:15672

#### Option 2: Using Command Line

If you prefer using the terminal or don't have Visual Studio:

1. **Start RabbitMQ**
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

2. **Start each service** (open separate terminals):

   ```bash
   # Terminal 1 - Order Service
   cd src\Order\OrderServiceApi
   dotnet run
   ```

   ```bash
   # Terminal 2 - Payment Service
   cd src\Payment\PaymentServiceApi
   dotnet run
   ```

   ```bash
   # Terminal 3 - Notification Service
   cd src\Notification\NotificationServiceApi
   dotnet run
   ```

   ```bash
   # Terminal 4 - API Gateway
   cd src\APIGateway\ApiGateway
   dotnet run
   ```

3. **Verify services are running**: Same as Option 1 above

### Running with Docker Compose

This is the easiest way to run the entire system:

```bash
# Build and start all services
docker-compose up --build

# Or run in detached mode
docker-compose up -d --build

# Stop all services
docker-compose down
```

Docker Compose will start:
- Order Service on port 5000
- Payment Service on port 5001
- Notification Service on port 5002
- RabbitMQ on ports 5672 and 15672

## API Endpoints

All requests go through the API Gateway at `http://localhost:5000`

### Create Order
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

### Get All Orders
```http
GET http://localhost:5000/order-service/api/order/GetOrders
```

### Get All Payments
```http
GET http://localhost:5000/payment-service/api/payment/GetPayments
```

### Get All Notifications
```http
GET http://localhost:5000/notification-service/api/notification/GetNotifications
```

### Health Checks
```http
GET http://localhost:5000/order-service/health
GET http://localhost:5000/payment-service/health
GET http://localhost:5000/notification-service/health
```

### Swagger Documentation
- Order Service: http://localhost:5000/order-service/swagger/index.html
- Payment Service: http://localhost:5000/payment-service/swagger/index.html
- Notification Service: http://localhost:5000/notification-service/swagger/index.html

## Design Decisions

### Architecture Patterns

- **CQRS with MediatR**: Order Service uses CQRS to separate read and write operations, making the code easier to maintain and scale.

- **Event-Driven Architecture**: Services communicate asynchronously through RabbitMQ, ensuring loose coupling and resilience.

- **Clean Architecture**: Each service follows a layered architecture (API → Application → Domain → Infrastructure) for better separation of concerns.

- **Repository Pattern**: Data access is abstracted through repository interfaces, making it easy to swap out data stores.

### Technology Choices

- **MassTransit**: Provides a powerful abstraction over RabbitMQ with retry policies, error handling, and message routing.

- **In-Memory Database**: Using EF Core In-Memory database for simplicity. In production, this would be replaced with SQL Server, PostgreSQL, or another persistent database.

- **Ocelot API Gateway**: Lightweight and configurable gateway suitable for .NET microservices.

- **AutoMapper**: Used for object-to-object mapping. *(Note: The current version has known security vulnerabilities. In production, always ensure NuGet packages are up-to-date and vulnerability-free.)*

### Design Assumptions

- Order IDs are randomly generated (1-1000 range). In production, use database auto-increment or a distributed ID generator.
- Email notifications are simulated (logged only). Real implementation would integrate with SendGrid, AWS SES, or similar.
- All services use in-memory databases that reset on restart. Production would require persistent storage.
- RabbitMQ runs with default credentials. Production should use proper authentication and TLS.
- No authentication/authorization is implemented. Production APIs should use OAuth2/JWT tokens.

## Known Limitations & Future Improvements

### Current Limitations

1. **Code Duplication**: Exception handling middleware is duplicated across all three services. This could be moved to a shared NuGet package or common library.

2. **In-Memory Storage**: Data is lost when services restart. Should be replaced with persistent databases (SQL Server, PostgreSQL, MongoDB, etc.).

3. **Limited Error Handling**: Basic error handling exists, but needs more sophisticated retry policies, circuit breakers (Polly), and dead-letter queues.

4. **Package Vulnerabilities**: AutoMapper package has known vulnerabilities. Should be updated or replaced in production environments.

5. **Hard-coded Configuration**: Some settings are hard-coded. Should use Azure Key Vault or similar for secrets management.

### Future Enhancements

- **Database Per Service**: Implement SQL Server for Order Service, PostgreSQL for Payment Service, and MongoDB for Notification Service to demonstrate polyglot persistence.

- **API Authentication**: Add JWT-based authentication and role-based authorization.

- **Saga Pattern**: Implement distributed transactions using MassTransit Sagas for more complex workflows (e.g., order cancellation, refunds).

- **Observability**: 
  - Add structured logging with Serilog
  - Implement distributed tracing (Jaeger/OpenTelemetry)
  - Add metrics and monitoring (Prometheus/Grafana)

- **Resilience Patterns**:
  - Circuit breakers using Polly
  - Retry policies with exponential backoff
  - Bulkhead isolation
  - Timeout policies

- **Testing**:
  - Integration tests using TestContainers
  - Contract testing for event schemas
  - Load testing with k6 or JMeter

- **Deployment**:
  - Kubernetes manifests for orchestration
  - Helm charts for easier deployment
  - CI/CD pipelines (GitHub Actions/Azure DevOps)

- **Common Library**: Extract shared code (middleware, DTOs, extensions, configurations) into a shared NuGet package to reduce duplication.

## Testing

Run tests for individual services:

```bash
# Order Service tests
cd src\Order\OrderServiceTest
dotnet test

# Payment Service tests
cd src\Payment\PaymentServiceTests
dotnet test

# Notification Service tests
cd src\Notification\NotificationServiceTests
dotnet test
```

## Monitoring

- **RabbitMQ Management UI**: http://localhost:15672
  - Username: guest
  - Password: guest
  - Monitor queues, exchanges, and message rates

- **Health Endpoints**: Each service exposes a `/health` endpoint for monitoring.

## Troubleshooting

**Services can't connect to RabbitMQ:**
- Ensure RabbitMQ is running: `docker ps | grep rabbitmq`
- Check connection settings in `appsettings.json`
- Verify network connectivity

**API Gateway returns 503:**
- Verify downstream services are running
- Check port configurations in `ocelot.json` match actual service ports

**Messages not flowing between services:**
- Check RabbitMQ management UI for messages in queues
- Verify consumers are registered and running
- Check service logs for errors

## Contributing

1. Follow the existing code structure and patterns
2. Add tests for new features
3. Update this README if adding new services or changing architecture
4. Ensure all packages are vulnerability-free before committing

---

**Note**: This system is built for educational purposes and demonstrates microservices patterns. It is NOT production-ready and requires significant enhancements for production use, particularly around security, data persistence, observability, and resilience.
