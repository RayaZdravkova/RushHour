using RushHour.Domain.DTOs.BaseDtos;
using RushHour.Domain.Enums;

namespace RushHour.Domain.DTOs.Accounts
{
    public class AccountResponseDto : BaseResponseDto
    {
        public string Email { get; set; }

        public string FullName { get; set; }

        public Roles Role { get; set; }

        public string Username { get; set; }
    }
}
