using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infra.ApiContext;
using OrderService.Infra.Repository;
using Shouldly;

namespace OrderServiceTest.Infrastructure
{
    public class OrderServiceInfraFixture : IDisposable
    {
        private readonly IFixture _fixture;
        private readonly OrderServiceContext _context;
        private readonly OrderRepository _repository;

        public OrderServiceInfraFixture()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            var options = new DbContextOptionsBuilder<OrderServiceContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new OrderServiceContext(options);
            _repository = new OrderRepository(_context);
        }

        [Fact]
        public async Task CreateOrderAsync_Should_Add_Order_To_Database()
        {
            var order = _fixture.Build<Orders>()
                .With(x => x.Id, 0)
                .With(x => x.Amount, 100.50m)
                .With(x => x.CustomerEmail, "test@example.com")
                .With(x => x.Status, "Pending")
                .With(x => x.CreatedDate, DateTime.UtcNow)
                .Create();

            var result = await _repository.CreateOrderAsync(order, CancellationToken.None);

            result.ShouldBeGreaterThan(0);
            var savedOrder = await _context.Orders.FindAsync(result);
            savedOrder.ShouldNotBeNull();
            savedOrder.CustomerEmail.ShouldBe(order.CustomerEmail);
            savedOrder.Amount.ShouldBe(order.Amount);
        }

        [Fact]
        public async Task CreateOrderAsync_Should_Throw_Exception_When_Order_Is_Null()
        {
            await Should.ThrowAsync<ArgumentNullException>(async () =>
                await _repository.CreateOrderAsync(null!, CancellationToken.None));
        }

        [Fact]
        public async Task GetOrdersAsync_Should_Return_All_Orders()
        {
            var orders = _fixture.Build<Orders>()
                .With(x => x.Id, 0)
                .With(x => x.Amount, _fixture.Create<decimal>())
                .With(x => x.CustomerEmail, _fixture.Create<string>())
                .With(x => x.Status, _fixture.Create<string>())
                .With(x => x.CreatedDate, _fixture.Create<DateTime>())
                .CreateMany(3)
                .ToList();

            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();

            var result = await _repository.GetOrdersAsync(CancellationToken.None);

            result.ShouldNotBeNull();
            result.Count.ShouldBe(3);
            result.ShouldAllBe(order => !string.IsNullOrEmpty(order.CustomerEmail));
        }

        [Fact]
        public async Task GetOrdersAsync_Should_Return_Empty_List_When_No_Orders()
        {
            var result = await _repository.GetOrdersAsync(CancellationToken.None);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        [Fact]
        public async Task CreateOrderAsync_Should_Save_All_Order_Properties()
        {
            var order = _fixture.Build<Orders>()
                .With(x => x.Id, 0)
                .With(x => x.Amount, 250.75m)
                .With(x => x.CustomerEmail, "customer@test.com")
                .With(x => x.Status, "Completed")
                .With(x => x.CreatedDate, new DateTime(2024, 1, 15))
                .Create();

            var orderId = await _repository.CreateOrderAsync(order, CancellationToken.None);

            var savedOrder = await _context.Orders.FindAsync(orderId);
            savedOrder.ShouldNotBeNull();
            savedOrder.Amount.ShouldBe(250.75m);
            savedOrder.CustomerEmail.ShouldBe("customer@test.com");
            savedOrder.Status.ShouldBe("Completed");
            savedOrder.CreatedDate.ShouldBe(new DateTime(2024, 1, 15));
        }

        [Fact]
        public async Task GetOrdersAsync_Should_Not_Track_Entities()
        {
            var order = _fixture.Build<Orders>()
                .With(x => x.Id, 0)
                .With(x => x.Amount, 100m)
                .With(x => x.CustomerEmail, "test@example.com")
                .With(x => x.Status, "Pending")
                .With(x => x.CreatedDate, DateTime.UtcNow)
                .Create();

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var result = await _repository.GetOrdersAsync(CancellationToken.None);

            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            _context.ChangeTracker.Entries().ShouldBeEmpty();
        }

        public void Dispose()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
        }
    }
}
