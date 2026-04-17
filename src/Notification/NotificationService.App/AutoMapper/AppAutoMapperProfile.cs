using AutoMapper;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Models;

namespace NotificationService.App.AutoMapper
{
    public class AppAutoMapperProfile : Profile
    {
        public AppAutoMapperProfile()
        {

            CreateMap<Notification, NotificationResponse>()
                .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
