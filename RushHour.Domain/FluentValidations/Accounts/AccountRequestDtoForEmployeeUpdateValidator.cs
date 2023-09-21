﻿using FluentValidation;
using RushHour.Domain.DTOs.Accounts;
using System.Text.RegularExpressions;

namespace RushHour.Domain.FluentValidations.Accounts
{
    public class AccountRequestDtoForEmployeeUpdateValidator : AbstractValidator<AccountRequestDtoForEmployeeUpdate>
    {
        Regex fullNameRegex = new Regex("^[A-Za-z'-]+\\z", RegexOptions.None, TimeSpan.FromSeconds(2));
        public AccountRequestDtoForEmployeeUpdateValidator()
        {
            RuleFor(x => x.Email).NotNull()
                                 .EmailAddress();

            RuleFor(x => x.FullName).NotNull()
                                    .Length(3, 100)
                                    .Matches(fullNameRegex)
                                    .WithMessage("Full name should contain only letters, hyphens, and apostrophes and should be minimum 3 characters");

            RuleFor(x => x.Role).NotNull();

            RuleFor(x => x.Username).NotNull();
        }
    }
}
