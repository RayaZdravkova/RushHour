using AutoMapper;
using RushHour.Persistance.Entities;

namespace RushHour.Domain.DTOs.Employees
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<EmployeeRequestDto, Employee>()
                .ForPath(x => x.Provider.Id, opt => opt.MapFrom(src => src.ProviderId))
                .ForPath(x => x.Account.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForPath(x => x.Account.Email, opt => opt.MapFrom(src => src.Email))
                .ForPath(x => x.Account.Role, opt => opt.MapFrom(src => src.Role))
                .ForPath(x => x.Account.Username, opt => opt.MapFrom(src => src.Username))
                .ForPath(x => x.Account.Password, opt => opt.MapFrom(src => src.Password));
            CreateMap<EmployeeRequestDtoForUpdate, Employee>()
                .ForPath(x => x.Provider.Id, opt => opt.MapFrom(src => src.ProviderId))
                .ForPath(x => x.Account.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForPath(x => x.Account.Email, opt => opt.MapFrom(src => src.Email))
                .ForPath(x => x.Account.Role, opt => opt.MapFrom(src => src.Role))
                .ForPath(x => x.Account.Username, opt => opt.MapFrom(src => src.Username))
                .ForPath(x => x.Account.Salt, option => option.Ignore())
                .ForPath(x => x.Account.Id, option => option.Ignore())
                .ForPath(x => x.Account.Password, option => option.Ignore());
            CreateMap<Employee, EmployeeResponseDto>()
                .ForMember(x => x.ProviderId, opt => opt.MapFrom(src => src.Provider.Id))
                .ForMember(x => x.FullName, opt => opt.MapFrom(src => src.Account.FullName))
                .ForMember(x => x.Email, opt => opt.MapFrom(src => src.Account.Email))
                .ForMember(x => x.Role, opt => opt.MapFrom(src => src.Account.Role))
                .ForMember(x => x.Username, opt => opt.MapFrom(src => src.Account.Username));

        }
    }
}