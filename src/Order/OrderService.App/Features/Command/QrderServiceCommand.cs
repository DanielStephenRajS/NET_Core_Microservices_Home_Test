using MediatR;
using OrderService.App.Features.Queries.Models;

namespace OrderService.App.Features.Command
{
    public class QrderServiceCommand : IRequest<int>
    {
        public required OrderServiceRequest OrderServiceRequest { get; init; }
    }
}
