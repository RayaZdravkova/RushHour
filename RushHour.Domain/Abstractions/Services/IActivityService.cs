using RushHour.Domain.DTOs.Activities;
using RushHour.Domain.Pagination;

namespace RushHour.Domain.Abstractions.Services
{
    public interface IActivityService
    {
        Task<ActivityResponseDto> CreateAsync(ActivityRequestDto dto);
        Task<ActivityResponseDto> UpdateAsync(int id, ActivityRequestDto dto);
        Task DeleteAsync(int id);
        Task<ActivityResponseDto> GetByIdAsync(int id);
        Task<List<ActivityResponseDto>> GetAllAsync(PagingInfo pagingInfo);

    }
}
