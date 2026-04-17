using Microsoft.AspNetCore.Mvc;
using MediatR;
using PaymentService.App.Features.Queries;

namespace PaymentServiceApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IMediator _mediator;

        public PaymentController(ILogger<PaymentController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("GetPayments")]
        public async Task<IActionResult> GetPaymentDetails() 
        {
          _logger.LogInformation("Fetching payment details...");

            var response = await _mediator.Send(new PaymentServiceQuery());
            return Ok(response);
        }
    }
}
