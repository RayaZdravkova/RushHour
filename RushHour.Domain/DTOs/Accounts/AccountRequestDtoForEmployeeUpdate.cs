using RushHour.Domain.Enums;

namespace RushHour.Domain.DTOs.Accounts
{
    public class AccountRequestDtoForEmployeeUpdate
    {
        public string Email { get; set; }

        public string FullName { get; set; }

        public Roles Role { get; set; }

        public string Username { get; set; }
    }
}
