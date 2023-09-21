using AutoMapper;
using RushHour.Domain.DTOs.Clients;
using RushHour.Persistance.Entities;

namespace RushHour.Persistance.AutomapperProfiles
{
    public class ClientProfile : Profile
    {
        public ClientProfile() 
        {
            CreateMap<ClientRequestDto, Client>()
                .ForPath(x => x.Account.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForPath(x => x.Account.Email, opt => opt.MapFrom(src => src.Email))
                .ForPath(x => x.Account.Password, opt => opt.MapFrom(src => src.Password))
                .ForPath(x => x.Account.Username, opt => opt.MapFrom(src => src.Username));
            CreateMap<ClientRequestDtoForUpdate, Client>()
                .ForPath(x => x.Account.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForPath(x => x.Account.Email, opt => opt.MapFrom(src => src.Email))
                .ForPath(x => x.Account.Username, opt => opt.MapFrom(src => src.Username))
                .ForPath(x => x.Account.Salt, option => option.Ignore())
                .ForPath(x => x.Account.Id, option => option.Ignore())
                .ForPath(x => x.Account.Password, option => option.Ignore());
            CreateMap<Client, ClientResponseDto>()
                .ForMember(x => x.FullName, opt => opt.MapFrom(src => src.Account.FullName))
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(x => x.Role, opt => opt.MapFrom(src => src.Account.Role))
                .ForMember(x => x.Username, opt => opt.MapFrom(src => src.Account.Username));

        }
    }
}
