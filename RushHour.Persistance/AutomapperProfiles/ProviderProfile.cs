using AutoMapper;
using RushHour.Domain.DTOs.Providers;
using RushHour.Persistance.Entities;

namespace RushHour.Persistance.AutomapperProfiles
{
    public class ProviderProfile : Profile
    {
        public ProviderProfile()
        {
            CreateMap<ProviderRequestDto, Provider>();
            CreateMap<Provider, ProviderResponseDto>();
        }
    }
}
