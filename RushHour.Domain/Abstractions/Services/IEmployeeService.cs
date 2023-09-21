using RushHour.Domain.DTOs.Employees;
using RushHour.Domain.Pagination;

namespace RushHour.Domain.Abstractions.Services
{
    public interface IEmployeeService
    {
        Task<EmployeeResponseDto> CreateAsync(EmployeeRequestDto dto);

        Task DeleteAsync(int id);

        Task<List<EmployeeResponseDto>> GetAllAsync(PagingInfo pagingInfo);

        Task<EmployeeResponseDto> GetByIdAsync(int id);

        Task<EmployeeResponseDto> UpdateAsync(int id, EmployeeRequestDtoForUpdate dto);
    }
}
