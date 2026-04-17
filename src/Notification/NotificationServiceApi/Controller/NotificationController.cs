using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationService.App.Features.Queries;

namespace NotificationServiceApi.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly IMediator _mediator;
        public NotificationController(ILogger<NotificationController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }


        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotifications()
        {

            var response = await _mediator.Send(new GetNotificationServiceQuery());
            return Ok(response);
        }
    }
}
