using RushHour.Domain.DTOs.BaseDtos;

namespace RushHour.Domain.DTOs.Appointments
{
    public class AppointmentResponseDto : BaseResponseDto
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int EmployeeId { get; set; }

        public int ClientId { get; set; }

        public int ActivityId { get; set; }

    }
}
