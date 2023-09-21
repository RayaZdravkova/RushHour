using FluentValidation;
using RushHour.Domain.DTOs.Clients;
using System.Text.RegularExpressions;

namespace RushHour.Domain.FluentValidations.Clients
{
    public class ClientRequestDtoForUpdateValidator : AbstractValidator<ClientRequestDtoForUpdate>
    {
        Regex phoneRegex = new Regex("^[\\+]?[0-9]*$", RegexOptions.None, TimeSpan.FromSeconds(2));

        public ClientRequestDtoForUpdateValidator()
        {
            RuleFor(x => x.Phone).NotNull()
                             .Matches(phoneRegex)
                             .WithMessage("Phone should contain only numbers and optionally could start with a +!");

            RuleFor(x => x.Address).NotNull()
                                   .MinimumLength(3)
                                   .WithMessage("Address should be minimum 3 characters long!");
        }
    }
}
