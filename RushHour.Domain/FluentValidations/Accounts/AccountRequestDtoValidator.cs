using FluentValidation;
using RushHour.Domain.DTOs.Accounts;
using System.Text.RegularExpressions;

namespace RushHour.Domain.FluentValidations.Accounts
{
    public class AccountRequestDtoValidator : AbstractValidator<AccountRequestDto>
    {
        Regex fullNameRegex = new Regex("^[A-Za-z'-]+\\z", RegexOptions.None, TimeSpan.FromSeconds(2));
        Regex passwordRegex = new Regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*.+?&])[A-Za-z\\d@$!%.*+?&]{8,}$", RegexOptions.None, TimeSpan.FromSeconds(2));
        public AccountRequestDtoValidator()
        {
            RuleFor(x => x.Email).NotNull()
                                 .EmailAddress();

            RuleFor(x => x.FullName).NotNull()
                                    .Length(3, 100)
                                    .Matches(fullNameRegex)
                                    .WithMessage("Full name should contain only letters, hyphens, and apostrophes and should be minimum 3 characters");

            RuleFor(x => x.Password).NotNull()
                                    .Matches(passwordRegex)
                                    .WithMessage("Password should be at least 8 characters, one uppercase, one lowercase, one digit, and a special symbol!");

            RuleFor(x => x.Role).NotNull();

            RuleFor(x => x.Username).NotNull();
        }
    }
}
