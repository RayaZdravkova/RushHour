using System.ComponentModel.DataAnnotations;

namespace RushHour.Domain.DTOs.Accounts
{
    public class AccountRequestDtoForLogin
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
