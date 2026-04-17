using AutoFixture;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Infra.ApiContext;
using PaymentService.Infra.Repository;
using Shouldly;

namespace PaymentServiceTests.Infrastructure
{
    public class PaymentServiceInfraFixture : IDisposable
    {
        private readonly PaymentServiceDbContext _dbContext;
        private readonly PaymentRepository _repository;
        private readonly IFixture _fixture;

        public PaymentServiceInfraFixture()
        {
            var options = new DbContextOptionsBuilder<PaymentServiceDbContext>()
                .UseInMemoryDatabase(databaseName: $"PaymentTestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new PaymentServiceDbContext(options);
            _repository = new PaymentRepository(_dbContext);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task CreatePaymentAsync_Should_Add_Payment_To_Database()
        {
            var payment = _fixture.Build<Payment>()
                .With(p => p.Id, "PAY-TEST-001")
                .With(p => p.OrderId, 100)
                .With(p => p.Amount, 500.50m)
                .With(p => p.Status, "Completed")
                .Create();

            await _repository.CreatePaymentAsync(payment, CancellationToken.None);

            var savedPayment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == payment.Id);
            savedPayment.ShouldNotBeNull();
            savedPayment.Id.ShouldBe(payment.Id);
        }

        [Fact]
        public async Task CreatePaymentAsync_Should_Throw_Exception_When_Payment_Is_Null()
        {
            await Should.ThrowAsync<ArgumentNullException>(async () =>
            {
                await _repository.CreatePaymentAsync(null, CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetPaymentAsync_Should_Return_All_Payments()
        {
            var payment1 = _fixture.Build<Payment>()
                .With(p => p.Id, "PAY-TEST-002")
                .With(p => p.OrderId, 101)
                .With(p => p.Amount, 200.00m)
                .Create();

            var payment2 = _fixture.Build<Payment>()
                .With(p => p.Id, "PAY-TEST-003")
                .With(p => p.OrderId, 102)
                .With(p => p.Amount, 300.00m)
                .Create();

            await _dbContext.Payments.AddRangeAsync(payment1, payment2);
            await _dbContext.SaveChangesAsync();

            var payments = await _repository.GetPaymentAsync(CancellationToken.None);

            payments.ShouldNotBeNull();
            payments.Count.ShouldBe(2);
            payments.ShouldContain(p => p.Id == "PAY-TEST-002");
            payments.ShouldContain(p => p.Id == "PAY-TEST-003");
        }

        [Fact]
        public async Task GetPaymentAsync_Should_Return_Empty_List_When_No_Payments()
        {
            var payments = await _repository.GetPaymentAsync(CancellationToken.None);

            payments.ShouldNotBeNull();
            payments.ShouldBeEmpty();
        }

        [Fact]
        public async Task CreatePaymentAsync_Should_Save_All_Payment_Properties()
        {
            var payment = new Payment
            {
                Id = "PAY-TEST-004",
                OrderId = 103,
                Amount = 150.75m,
                CustomerEmail = "test@example.com",
                Status = "Pending",
                Timestamp = new DateTime(2024, 6, 15, 14, 30, 0)
            };

            await _repository.CreatePaymentAsync(payment, CancellationToken.None);

            var savedPayment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == "PAY-TEST-004");
            savedPayment.ShouldNotBeNull();
            savedPayment.OrderId.ShouldBe(103);
            savedPayment.Amount.ShouldBe(150.75m);
            savedPayment.CustomerEmail.ShouldBe("test@example.com");
            savedPayment.Status.ShouldBe("Pending");
            savedPayment.Timestamp.ShouldBe(new DateTime(2024, 6, 15, 14, 30, 0));
        }

        [Fact]
        public async Task GetPaymentAsync_Should_Not_Track_Entities()
        {
            var payment = _fixture.Build<Payment>()
                .With(p => p.Id, "PAY-TEST-005")
                .With(p => p.OrderId, 104)
                .With(p => p.Amount, 400.00m)
                .Create();

            await _dbContext.Payments.AddAsync(payment);
            await _dbContext.SaveChangesAsync();

            var payments = await _repository.GetPaymentAsync(CancellationToken.None);

            var trackedEntities = _dbContext.ChangeTracker.Entries<Payment>().Count();
            trackedEntities.ShouldBe(1);

            var retrievedPayment = payments.First();
            var isTracked = _dbContext.Entry(retrievedPayment).State != EntityState.Detached;
            isTracked.ShouldBeFalse();
        }

        public void Dispose()
        {
            _dbContext?.Database.EnsureDeleted();
            _dbContext?.Dispose();
        }
    }
}
