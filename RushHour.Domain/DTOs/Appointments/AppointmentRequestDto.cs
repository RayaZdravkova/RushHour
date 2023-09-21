namespace RushHour.Domain.DTOs.Appointments
{
    public class AppointmentRequestDto
    {
        public DateTime StartDate { get; set; }

        public int EmployeeId { get; set; }

        public int ClientId { get; set; }

        public int ActivityId { get; set; }
    }
}
