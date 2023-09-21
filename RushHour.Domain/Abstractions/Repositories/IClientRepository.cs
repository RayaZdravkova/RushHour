using RushHour.Domain.DTOs.Clients;
using RushHour.Domain.Pagination;

namespace RushHour.Domain.Abstractions.Repositories
{
    public interface IClientRepository
    {
        Task<ClientResponseDto> CreateAsync(ClientRequestDto dto, byte[] salt);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<ClientResponseDto>> GetAllAsync(PagingInfo pagingInfo);
        Task<ClientResponseDto> GetByIdAsync(int id);
        Task<ClientResponseDto> UpdateAsync(int id, ClientRequestDtoForUpdate dto);
        Task<bool> DoesClientMatchAccountAsync(int clientId, int accountId);
    }
}
