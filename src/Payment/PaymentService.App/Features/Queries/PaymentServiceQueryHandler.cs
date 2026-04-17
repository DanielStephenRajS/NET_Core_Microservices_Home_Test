using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Models;

namespace PaymentService.App.Features.Queries
{
    public class PaymentServiceQueryHandler : IRequestHandler<PaymentServiceQuery, List<PaymentServiceResponse>>
    {
        private readonly ILogger<PaymentServiceQueryHandler> _logger;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        public PaymentServiceQueryHandler(ILogger<PaymentServiceQueryHandler> logger, IPaymentRepository paymentRepository, IMapper mapper)
        {
            _logger = logger;
            _paymentRepository = paymentRepository;
            _mapper = mapper;
        }

        public async Task<List<PaymentServiceResponse>> Handle(PaymentServiceQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling PaymentServiceQuery to retrieve all payments.");
            var payments = await _paymentRepository.GetPaymentAsync(cancellationToken);

            // Auto Mapper
            var mappedPayments = _mapper.Map<List<PaymentServiceResponse>>(payments);
            _logger.LogInformation("Successfully retrieved and mapped {Count} payments.", mappedPayments.Count);

            return mappedPayments;
        }
    }
}
