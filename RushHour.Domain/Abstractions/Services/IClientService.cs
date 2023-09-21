using RushHour.Domain.DTOs.Clients;
using RushHour.Domain.Pagination;

namespace RushHour.Domain.Abstractions.Services
{
    public interface IClientService
    {
        Task<ClientResponseDto> CreateAsync(ClientRequestDto dto);

        Task DeleteAsync(int id);

        Task<List<ClientResponseDto>> GetAllAsync(PagingInfo pagingInfo);

        Task<ClientResponseDto> GetByIdAsync(int id);

        Task<ClientResponseDto> UpdateAsync(int id, ClientRequestDtoForUpdate dto);
    }
}
