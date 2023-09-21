using FluentValidation;
using RushHour.Domain.DTOs.Appointments;

namespace RushHour.Domain.FluentValidations.Appointments
{
    public class AppointmentRequestDtoValidator : AbstractValidator<AppointmentRequestDto>
    {
        public AppointmentRequestDtoValidator()
        {
            RuleFor(x => x.StartDate).NotNull();

            RuleFor(x => x.EmployeeId).NotNull();

            RuleFor(x => x.ClientId).NotNull();

            RuleFor(x => x.ActivityId).NotNull();
        }
    }
}
