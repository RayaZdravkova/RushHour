using RushHour.Domain.DTOs.Appointments;
using RushHour.Domain.Pagination;

namespace RushHour.Domain.Abstractions.Services
{
    public interface IAppointmentService
    {
        Task<AppointmentsAndPriceResponseDto> CreateAsync(AppointmentCreateRequestDto dto);
        Task<AppointmentResponseDto> UpdateAsync(int id, AppointmentRequestDto dto);
        Task DeleteAsync(int id);
        Task<AppointmentResponseDto> GetByIdAsync(int id);
        Task<List<AppointmentResponseDto>> GetAllAsync(PagingInfo pagingInfo);
    }
}
