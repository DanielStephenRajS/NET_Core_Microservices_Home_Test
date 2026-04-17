using PaymentService.Domain.Entities;
using PaymentService.Domain.Models;

namespace PaymentServiceTests.MockDataProvider
{
    public class MockDataProvider
    {
        public List<Payment> GetMockPayments()
        {
            return new List<Payment>
            {
                new Payment
                {
                    Id = "PAY-001",
                    OrderId = 1,
                    Amount = 100.50m,
                    CustomerEmail = "customer1@example.com",
                    Status = "Completed",
                    Timestamp = new DateTime(2024, 1, 1, 10, 0, 0)
                },
                new Payment
                {
                    Id = "PAY-002",
                    OrderId = 2,
                    Amount = 250.75m,
                    CustomerEmail = "customer2@example.com",
                    Status = "Completed",
                    Timestamp = new DateTime(2024, 1, 2, 15, 30, 0)
                },
                new Payment
                {
                    Id = "PAY-003",
                    OrderId = 3,
                    Amount = 75.00m,
                    CustomerEmail = "customer3@example.com",
                    Status = "Pending",
                    Timestamp = new DateTime(2024, 1, 3, 9, 15, 0)
                }
            };
        }

        public List<PaymentServiceResponse> GetMockPaymentResponses()
        {
            return new List<PaymentServiceResponse>
            {
                new PaymentServiceResponse
                {
                    PaymentId = "PAY-001",
                    OrderId = 1,
                    Amount = 100.50m,
                    CustomerEmail = "customer1@example.com",
                    Status = "Completed",
                    Timestamp = new DateTime(2024, 1, 1, 10, 0, 0)
                },
                new PaymentServiceResponse
                {
                    PaymentId = "PAY-002",
                    OrderId = 2,
                    Amount = 250.75m,
                    CustomerEmail = "customer2@example.com",
                    Status = "Completed",
                    Timestamp = new DateTime(2024, 1, 2, 15, 30, 0)
                },
                new PaymentServiceResponse
                {
                    PaymentId = "PAY-003",
                    OrderId = 3,
                    Amount = 75.00m,
                    CustomerEmail = "customer3@example.com",
                    Status = "Pending",
                    Timestamp = new DateTime(2024, 1, 3, 9, 15, 0)
                }
            };
        }
    }
}
