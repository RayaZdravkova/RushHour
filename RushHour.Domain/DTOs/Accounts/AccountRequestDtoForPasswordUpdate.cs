using System.ComponentModel.DataAnnotations;

namespace RushHour.Domain.DTOs.Accounts
{
    public class AccountRequestDtoForPasswordUpdate
    {
        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
    }
}
