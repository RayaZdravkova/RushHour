using FluentValidation;
using RushHour.Domain.DTOs.Activities;

namespace RushHour.Domain.FluentValidations.Activities
{
    public class ActivityRequestDtoValidator : AbstractValidator<ActivityRequestDto>
    {
        public ActivityRequestDtoValidator() 
        {
            RuleFor(x => x.Name).NotNull()
                                .Length(2, 100)
                                .WithMessage("Name should be minimum 2 characters long!");

            RuleFor(x => x.Price).NotNull()
                                 .InclusiveBetween(0, decimal.MaxValue)
                                 .WithMessage("Price should be a positive number!");

            RuleFor(x => x.Duration).NotNull()
                                    .InclusiveBetween(0, int.MaxValue)
                                    .WithMessage("Duration should be a positive number!");
        }
    }
}
