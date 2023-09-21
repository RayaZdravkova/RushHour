using RushHour.Domain.DTOs.Accounts;

namespace RushHour.Domain.DTOs.Employees
{
    public class EmployeeRequestDtoForUpdate : AccountRequestDtoForEmployeeUpdate
    {
        public string Title { get; set; }

        public string Phone { get; set; }

        public decimal RatePerHour { get; set; }

        public DateTime HireDate { get; set; }

        public int ProviderId { get; set; }
    }
}
