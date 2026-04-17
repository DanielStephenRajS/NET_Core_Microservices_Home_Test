using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.App.Features.Command;
using OrderService.App.Features.Queries;
using OrderService.App.Features.Queries.Models;

namespace OrderServiceApi.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IMediator _mediator;
        public OrderController(ILogger<OrderController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("GetOrders")]
        public async Task<IActionResult> GetOrderDetails()
        {
            var response = await _mediator.Send(new OrderServiceQuery());
            return Ok(response);
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderServiceRequest request)
        {
            var response = await _mediator.Send(new QrderServiceCommand { OrderServiceRequest = request });
            return Ok(response);
        }
    }
}