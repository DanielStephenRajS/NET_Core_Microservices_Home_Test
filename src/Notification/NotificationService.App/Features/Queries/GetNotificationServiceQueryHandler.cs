using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Models;

namespace NotificationService.App.Features.Queries
{
    public class GetNotificationServiceQueryHandler : IRequestHandler<GetNotificationServiceQuery, List<NotificationResponse>>
    {
        private readonly ILogger<GetNotificationServiceQueryHandler> _logger;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public GetNotificationServiceQueryHandler(ILogger<GetNotificationServiceQueryHandler> logger, INotificationRepository notificationRepository, IMapper mapper)
        {
            _logger = logger;
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<List<NotificationResponse>> Handle(GetNotificationServiceQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetNotificationServiceQuery to retrieve all notifications.");

            var notificationResponse = await _notificationRepository.GetNotificationsAsync(cancellationToken);
            var mappedNotifications = _mapper.Map<List<NotificationResponse>>(notificationResponse);

            _logger.LogInformation("Successfully retrieved and mapped {Count} notifications.", mappedNotifications.Count);

            return mappedNotifications;
        }
    }
}
