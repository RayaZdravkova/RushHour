using FluentValidation;
using RushHour.Domain.DTOs.Providers;
using System.Text.RegularExpressions;

namespace RushHour.Domain.FluentValidations.Providers
{
    public class ProviderRequestDtoValidator : AbstractValidator<ProviderRequestDto>
    {
        Regex phoneRegex = new Regex("^[\\+]?[0-9]*$", RegexOptions.None, TimeSpan.FromSeconds(2));
        Regex businessDomainRegex = new Regex("^[A-Za-z0-9]*$", RegexOptions.None, TimeSpan.FromSeconds(2));
        Regex websiteRegex = new Regex("https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]+\\.[^\\s]{2,}", RegexOptions.None, TimeSpan.FromSeconds(2));
        public ProviderRequestDtoValidator()
        {
            RuleFor(x => x.Name).NotNull()
                               .Length(3, 100);

            RuleFor(x => x.Phone).NotNull()
                                 .Matches(phoneRegex)
                                 .WithMessage("Please, enter valid phone number!");

            RuleFor(x => x.Website).NotNull()
                                   .Matches(websiteRegex)
                                   .WithMessage("Website should be a valid URL!");

            RuleFor(x => x.BusinessDomain).NotNull()
                                          .Length(2, 100)
                                          .Matches(businessDomainRegex)
                                          .WithMessage("Business domain should contain only letters and numbers with minimum lenght of 2 characters!");

            RuleFor(x => x.StartTimeOfTheWorkingDay).NotNull()
                                                    .WithMessage("Please, fill in the field for start time of working day!");

            RuleFor(x => x.EndTimeOfTheWorkingDay).NotNull()
                                                  .WithMessage("Please, fill in the field for end time of working day!");

            RuleFor(x => x.WorkingDays).NotNull();
        }
    }
}
