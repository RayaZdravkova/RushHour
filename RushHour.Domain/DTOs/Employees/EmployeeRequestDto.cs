using RushHour.Domain.DTOs.Accounts;
using System.ComponentModel.DataAnnotations;
namespace RushHour.Domain.DTOs.Employees
{
    public class EmployeeRequestDto : AccountRequestDto
    {
        public string Title { get; set; }

        public string Phone { get; set; }

        public decimal RatePerHour { get; set; }

        public DateTime HireDate { get; set; }

        public int ProviderId { get; set; }
    }
}
