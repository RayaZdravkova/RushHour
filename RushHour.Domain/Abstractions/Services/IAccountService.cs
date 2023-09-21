using RushHour.Domain.DTOs.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RushHour.Domain.Abstractions.Services
{
    public interface IAccountService
    {
        Task ChangePassword(AccountRequestDtoForPasswordUpdate dto);
    }
}
