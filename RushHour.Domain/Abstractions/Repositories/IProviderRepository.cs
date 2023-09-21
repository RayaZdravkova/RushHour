using RushHour.Domain.DTOs.Providers;
using RushHour.Domain.Pagination;

namespace RushHour.Domain.Abstractions.Repositories
{
    public interface IProviderRepository
    {
        Task<ProviderResponseDto> CreateAsync(ProviderRequestDto dto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<ProviderResponseDto>> GetAllAsync(PagingInfo pagingInfo);
        Task<ProviderResponseDto> GetByIdAsync(int id);
        Task<ProviderResponseDto> UpdateAsync(int id, ProviderRequestDto dto);
    }
}
