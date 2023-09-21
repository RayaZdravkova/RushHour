using AutoMapper;
using RushHour.Domain.DTOs.Appointments;
using RushHour.Domain.DTOs.Employees;
using RushHour.Persistance.Entities;

namespace RushHour.Persistance.AutomapperProfiles
{
    public class AppointmentProfile : Profile
    {
        public AppointmentProfile()
        {
            CreateMap<AppointmentRequestDto, Appointment>();
            CreateMap<Appointment, AppointmentResponseDto>()
                .ForMember(x => x.ActivityId, opt => opt.MapFrom(src => src.Activity.Id))
                .ForMember(x => x.EmployeeId, opt => opt.MapFrom(src => src.Employee.Id))
                .ForMember(x => x.ClientId, opt => opt.MapFrom(src => src.Client.Id));
        }
    }
}
