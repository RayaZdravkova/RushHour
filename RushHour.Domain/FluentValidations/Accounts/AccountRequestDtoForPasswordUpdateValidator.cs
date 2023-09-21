using FluentValidation;
using RushHour.Domain.DTOs.Accounts;
using System.Text.RegularExpressions;

namespace RushHour.Domain.FluentValidations.Accounts
{
    public class AccountRequestDtoForPasswordUpdateValidator : AbstractValidator<AccountRequestDtoForPasswordUpdate>
    {
        Regex passwordRegex = new Regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*.+?&])[A-Za-z\\d@$!%.*+?&]{8,}$", RegexOptions.None, TimeSpan.FromSeconds(2));
        public AccountRequestDtoForPasswordUpdateValidator()
        {
            RuleFor(x => x.OldPassword).NotNull();

            RuleFor(x => x.NewPassword).NotNull()
                                       .Matches(passwordRegex)
                                       .WithMessage("New password should be at least 8 characters, one uppercase, one lowercase, one digit, and a special symbol!");
        }
    }
}
