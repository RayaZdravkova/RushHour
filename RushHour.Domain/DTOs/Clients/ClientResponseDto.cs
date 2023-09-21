using RushHour.Domain.DTOs.Accounts;

namespace RushHour.Domain.DTOs.Clients
{
    public class ClientResponseDto : AccountResponseDto
    {
        public string Phone { get; set; }

        public string Address { get; set; }
    }
}
