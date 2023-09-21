using RushHour.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RushHour.Domain.DTOs.Accounts
{
    public class AccountRequestDtoForClient
    {
        public string Email { get; set; }

        public string FullName { get; set; }

        public string Password { get; set; }

        public string Username { get; set; }
    }
}

