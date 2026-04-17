using AutoMapper;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Models;

namespace PaymentService.App.AutoMapper
{
    public class AppAutoMapperProfile : Profile
    {
        public AppAutoMapperProfile()
        {
            CreateMap<Payment, PaymentServiceResponse>()
                .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
