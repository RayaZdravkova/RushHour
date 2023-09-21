using RushHour.Domain.DTOs.Activities;
using RushHour.Domain.Pagination;

namespace RushHour.Domain.Abstractions.Repositories
{
    public interface IActivityRepository
    {
        Task<ActivityResponseDto> CreateAsync(ActivityRequestDto dto, int loggedUserProviderId);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<ActivityResponseDto>> GetAllAsync(PagingInfo pagingInfo);
        Task<ActivityResponseDto> GetByIdAsync(int id);
        Task<ActivityResponseDto> UpdateAsync(int id, ActivityRequestDto dto);
        Task<int> GetActivityProviderAsync(int activityId);
        bool CheckIfEmployeeIsPartOfActivities(int employeeId, List<int> activityIds);
        bool CheckIfEmployeeIsPartOfActivity(int employeeId, int activityId);
    }
}
