using MediatR;
using NotificationService.Domain.Models;

namespace NotificationService.App.Features.Queries
{
    public class GetNotificationServiceQuery : IRequest<List<NotificationResponse>>
    {
    }
}
