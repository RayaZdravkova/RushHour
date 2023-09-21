using RushHour.Domain.DTOs.Employees;
using RushHour.Domain.Pagination;

namespace RushHour.Domain.Abstractions.Repositories
{
    public interface IEmployeeRepository
    {
        Task<EmployeeResponseDto> CreateAsync(EmployeeRequestDto dto, byte[] salt);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<EmployeeResponseDto>> GetAllAsync(PagingInfo pagingInfo);
        Task<EmployeeResponseDto> GetByIdAsync(int id);
        Task<EmployeeResponseDto> UpdateAsync(int id, EmployeeRequestDtoForUpdate dto);
        Task<EmployeeResponseDto> GetEmployeeByAccountIdAsync(int id);
        Task<bool> DoesEmployeeMatchAccountAsync(int employeeId, int accountId);
        Task<bool> IsAccountPartOfProviderAsync(int accountId, int providerId);
        Task<bool> AreEmployeeAndAccountPartOfTheSameProviderAsync(int accountId, int employeeId);
        Task<int> GetEmployeeProviderIdByAccountIdAsync(int id);
        bool CheckEmployeeProvidersPartOfTheSameProvider(List<int> employeeIds, int loggedUserProviderId);
        Task<bool> CheckIfEmployeeIsFreeInSpecificTimeAsync(DateTime startTime, int employeeId, List<int> activityIds);
        Task<bool> CheckIfEmployeeIsFreeInSpecificTimeForUpdateAsync(int appointmentId, DateTime startTime, int employeeId, int activityId);
    }
}
