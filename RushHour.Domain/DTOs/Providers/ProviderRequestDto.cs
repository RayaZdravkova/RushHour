using RushHour.Domain.Enums;

namespace RushHour.Domain.DTOs.Providers
{
    public class ProviderRequestDto
    {
        public string Name { get; set; }

        public string Website { get; set; }

        public string BusinessDomain { get; set; }

        public string Phone { get; set; }

        public DateTime StartTimeOfTheWorkingDay { get; set; }

        public DateTime EndTimeOfTheWorkingDay { get; set; }

        public DaysOfWeek WorkingDays { get; set; }
    }
}
