using FluentValidation;
using RushHour.Domain.DTOs.Accounts;

namespace RushHour.Domain.FluentValidations.Accounts
{
    public class AccountRequestDtoForLoginValidator : AbstractValidator<AccountRequestDtoForLogin>
    {
        public AccountRequestDtoForLoginValidator()
        {
            RuleFor(x => x.Email).NotNull();

            RuleFor(x => x.Password).NotNull();
        }
    }
}
