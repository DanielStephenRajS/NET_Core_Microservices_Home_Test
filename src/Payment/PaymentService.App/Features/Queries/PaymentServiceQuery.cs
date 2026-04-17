using MediatR;
using PaymentService.Domain.Models;

namespace PaymentService.App.Features.Queries
{
    public class PaymentServiceQuery : IRequest<List<PaymentServiceResponse>>
    {
    }
}
