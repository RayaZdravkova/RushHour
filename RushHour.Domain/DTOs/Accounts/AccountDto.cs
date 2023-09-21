using RushHour.Domain.DTOs.BaseDtos;
using RushHour.Domain.Enums;

namespace RushHour.Domain.DTOs.Accounts
{
    public class AccountDto : BaseResponseDto
    {
        public string Email { get; set; }

        public string FullName { get; set; }

        public Roles Role { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public byte[] Salt { get; set; }
    }
}
