using AutoMapper;
using RushHour.Domain.DTOs.Activities;
using RushHour.Persistance.Entities;

namespace RushHour.Persistance.AutomapperProfiles
{
    public class ActivityProfile : Profile
    {
        public ActivityProfile()
        {
            CreateMap<ActivityRequestDto, Activity>();
            CreateMap<Activity, ActivityResponseDto>()
                .ForMember(x => x.ProviderId, opt => opt.MapFrom(src => src.Provider.Id));
        }
    }
}
