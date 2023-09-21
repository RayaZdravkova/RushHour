using FluentValidation;
using RushHour.Domain.DTOs.Employees;
using System.Text.RegularExpressions;

namespace RushHour.Domain.FluentValidations.Employees
{
    public class EmployeeRequestDtoValidator : AbstractValidator<EmployeeRequestDto>
    {
        Regex titleRegex = new Regex("^[A-Za-z][A-Za-z0-9]*$", RegexOptions.None, TimeSpan.FromSeconds(2));
        Regex phoneRegex = new Regex("^[\\+]?[0-9]*$", RegexOptions.None, TimeSpan.FromSeconds(2));
        public EmployeeRequestDtoValidator()
        {
            RuleFor(x => x.Title).NotNull()
                                    .Length(2, 100)
                                    .Matches(titleRegex)
                                    .WithMessage("Title should contain only numbers and letters and minimum 2 characters long!");

            RuleFor(x => x.Phone).NotNull()
                                 .Matches(phoneRegex)
                                 .WithMessage("Please, enter valid phone number!");

            RuleFor(x => x.RatePerHour).NotNull()
                                       .InclusiveBetween(0, decimal.MaxValue)
                                       .WithMessage("Rate per hour should be a positive decimal number!");

            RuleFor(x => x.HireDate).NotNull();

            RuleFor(x => x.ProviderId).NotNull();

        }
    }
}
