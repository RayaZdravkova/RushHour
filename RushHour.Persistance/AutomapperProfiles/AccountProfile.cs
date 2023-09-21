using AutoMapper;
using RushHour.Domain.DTOs.Accounts;
using RushHour.Persistance.Entities;

namespace RushHour.Persistance.AutomapperProfiles
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<Account, AccountDto>();
        }
    }
}
