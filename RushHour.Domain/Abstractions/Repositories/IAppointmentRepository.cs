using RushHour.Domain.DTOs.Appointments;
using RushHour.Domain.Pagination;

namespace RushHour.Domain.Abstractions.Repositories
{
    public interface IAppointmentRepository
    {
        Task<AppointmentsAndPriceResponseDto> CreateAsync(AppointmentCreateRequestDto dto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<AppointmentResponseDto>> GetAllAsync(PagingInfo pagingInfo);
        Task<AppointmentResponseDto?> GetByIdAsync(int id);
        Task<AppointmentResponseDto> UpdateAsync(int id, AppointmentRequestDto dto);
        Task<bool> DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(int appointmentId, int accountId);
        Task<bool> DoesAppointmentClientAccountMatchLoggedUserAccountAsync(int appointmentId, int accountId);
        Task<bool> AreAppointmentAndAccountPartOfTheSameProviderAsync(int appointmentId, int accountId);
    }
}
