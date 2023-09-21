namespace RushHour.Domain.DTOs.Appointments
{
    public class AppointmentsAndPriceResponseDto
    {
        public List<AppointmentResponseDto> Appointments { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
