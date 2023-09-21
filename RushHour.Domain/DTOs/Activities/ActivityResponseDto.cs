using RushHour.Domain.DTOs.BaseDtos;

namespace RushHour.Domain.DTOs.Activities
{
    public class ActivityResponseDto : BaseResponseDto
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Duration { get; set; }

        public int ProviderId { get; set; }

        public List<int> EmployeeIds { get; set; }
    }
}
