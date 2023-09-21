namespace RushHour.Domain.DTOs.Appointments
{
    public class AppointmentCreateRequestDto
    {
        public DateTime StartDate { get; set; }

        public int EmployeeId { get; set; }

        public int ClientId { get; set; }

        public List<int> ActivityIds { get; set; }
    }
}
