# API Gateway - Order Processing System

This API Gateway uses Ocelot to route requests to the Order, Payment, and Notification services.

## Service Ports

- **API Gateway**: http://localhost:5000 (HTTPS: 7000)
- **Order Service**: http://localhost:5020 (HTTPS: 7289)
- **Payment Service**: http://localhost:5232 (HTTPS: 7054)
- **Notification Service**: http://localhost:5196 (HTTPS: 7215)

## API Gateway Routes

All requests go through the API Gateway at `http://localhost:5000`

### Order Service Routes
- `GET/POST/PUT/DELETE http://localhost:5000/order-service/api/orders/{id}`
- `GET http://localhost:5000/order-service/health`
- `GET http://localhost:5000/order-service/swagger/v1/swagger.json`

### Payment Service Routes
- `GET/POST/PUT/DELETE http://localhost:5000/payment-service/api/payments/{id}`
- `GET http://localhost:5000/payment-service/health`
- `GET http://localhost:5000/payment-service/swagger/v1/swagger.json`

### Notification Service Routes
- `GET/POST/PUT/DELETE http://localhost:5000/notification-service/api/notifications/{id}`
- `GET http://localhost:5000/notification-service/health`
- `GET http://localhost:5000/notification-service/swagger/v1/swagger.json`

## Running the Application

1. **Start all services in order:**
   ```powershell
   # Start Order Service
   cd src\Order\OrderServiceApi
   dotnet run

   # In a new terminal - Start Payment Service
   cd src\Payment\PaymentServiceApi
   dotnet run

   # In a new terminal - Start Notification Service
   cd src\Notification\NotificationServiceApi
   dotnet run

   # In a new terminal - Start API Gateway
   cd src\APIGateway\ApiGateway
   dotnet run
   ```

2. **Access services through the gateway:**
   ```
   http://localhost:5000/order-service/api/orders
   http://localhost:5000/payment-service/api/payments
   http://localhost:5000/notification-service/api/notifications
   ```

## Configuration

The `ocelot.json` file contains all routing configuration. You can modify:
- **DownstreamHostAndPorts**: Change service host/port
- **UpstreamPathTemplate**: Change the gateway route pattern
- **RateLimitOptions**: Add rate limiting (see Ocelot documentation)
- **QoSOptions**: Add quality of service settings
- **AuthenticationOptions**: Add authentication

## Advanced Features (Optional)

### Rate Limiting
Add to any route in `ocelot.json`:
```json
"RateLimitOptions": {
  "ClientWhitelist": [],
  "EnableRateLimiting": true,
  "Period": "1s",
  "PeriodTimespan": 1,
  "Limit": 10
}
```

### Caching
Add to any route:
```json
"FileCacheOptions": {
  "TtlSeconds": 30,
  "Region": "region-name"
}
```

### Load Balancing
Add multiple downstream hosts:
```json
"DownstreamHostAndPorts": [
  { "Host": "localhost", "Port": 5020 },
  { "Host": "localhost", "Port": 5021 }
],
"LoadBalancerOptions": {
  "Type": "LeastConnection"
}
```

## Troubleshooting

1. **Services not responding**: Ensure all downstream services are running
2. **404 errors**: Check the route templates in `ocelot.json`
3. **CORS issues**: The gateway includes CORS policy "AllowAll" for development
4. **Port conflicts**: Update ports in `launchSettings.json` if needed

## Notes

- The gateway runs on HTTP for development
- CORS is enabled for all origins in development
- Swagger endpoints are proxied for each service
- Health check endpoints are available for monitoring
