using RushHour.Domain.DTOs.Accounts;

namespace RushHour.Domain.DTOs.Clients
{
    public class ClientRequestDtoForUpdate : AccountRequestDtoForClientUpdate
    {
        public string Phone { get; set; }

        public string Address { get; set; }
    }
}
