using OrderService.Domain.Entities;
using OrderService.App.Features.Queries.Models;

namespace OrderServiceTest.MockDataProvider
{
    public class MockDataProvider
    {
        public List<Orders> GetMockOrders()
        {
            return new List<Orders>
            {
                new Orders 
                { 
                    Id = 1, 
                    Amount = 100.50m, 
                    CustomerEmail = "customer1@example.com", 
                    Status = "Pending", 
                    CreatedDate = new DateTime(2024, 1, 1, 10, 0, 0) 
                },
                new Orders 
                { 
                    Id = 2, 
                    Amount = 250.75m, 
                    CustomerEmail = "customer2@example.com", 
                    Status = "Completed", 
                    CreatedDate = new DateTime(2024, 1, 2, 15, 30, 0) 
                },
                new Orders 
                { 
                    Id = 3, 
                    Amount = 75.00m, 
                    CustomerEmail = "customer3@example.com", 
                    Status = "Cancelled", 
                    CreatedDate = new DateTime(2024, 1, 3, 9, 15, 0) 
                }
            };
        }

        public List<OrderServiceResponse> GetMockOrderResponses()
        {
            return new List<OrderServiceResponse>
            {
                new OrderServiceResponse 
                { 
                    OrderId = 1, 
                    Amount = 100.50m, 
                    CustomerEmail = "customer1@example.com", 
                    Status = "Pending", 
                    CreatedDate = new DateTime(2024, 1, 1, 10, 0, 0) 
                },
                new OrderServiceResponse 
                { 
                    OrderId = 2, 
                    Amount = 250.75m, 
                    CustomerEmail = "customer2@example.com", 
                    Status = "Completed", 
                    CreatedDate = new DateTime(2024, 1, 2, 15, 30, 0) 
                },
                new OrderServiceResponse 
                { 
                    OrderId = 3, 
                    Amount = 75.00m, 
                    CustomerEmail = "customer3@example.com", 
                    Status = "Cancelled", 
                    CreatedDate = new DateTime(2024, 1, 3, 9, 15, 0) 
                }
            };
        }
    }
}
