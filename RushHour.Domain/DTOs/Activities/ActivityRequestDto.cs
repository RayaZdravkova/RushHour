namespace RushHour.Domain.DTOs.Activities
{
    public class ActivityRequestDto
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Duration { get; set; }

        public List<int> EmployeeIds { get; set; }
    }
}
