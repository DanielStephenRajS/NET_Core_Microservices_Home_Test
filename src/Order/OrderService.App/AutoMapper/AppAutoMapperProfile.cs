using AutoMapper;
using OrderService.App.Features.Queries.Models;
using OrderService.Domain.Entities;

namespace OrderService.App.AutoMapper
{
    public class AppAutoMapperProfile : Profile
    {
        public AppAutoMapperProfile() { 

            CreateMap<Orders,OrderServiceResponse>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id));
        }
    }
}