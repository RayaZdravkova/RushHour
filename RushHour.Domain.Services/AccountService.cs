using FluentValidation;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Accounts;
using ValidationException = RushHour.Domain.Exceptions.ValidationException;

namespace RushHour.Domain.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private IHttpContextAccessorWrapper _httpContextAccessor;
        private readonly IAuthService _authService;
        private readonly IValidationExtension _validationExtension;
        private readonly IValidator<AccountRequestDtoForPasswordUpdate> _accountRequestDtoForPasswordUpdateValidator;

        public AccountService(IAccountRepository accountRepository, IHttpContextAccessorWrapper httpContextAccessor, IAuthService authService, IValidationExtension validationExtension,
            IValidator<AccountRequestDtoForPasswordUpdate> accountRequestDtoForPasswordUpdateValidator)
        {
            _accountRepository = accountRepository;
            _httpContextAccessor = httpContextAccessor;
            _authService = authService;
            _validationExtension = validationExtension;
            _accountRequestDtoForPasswordUpdateValidator = accountRequestDtoForPasswordUpdateValidator;
        }

        public async Task ChangePassword(AccountRequestDtoForPasswordUpdate dto)
        {
            var loggedUserId = _httpContextAccessor.GetLoggedUserId();

            var account = await _accountRepository.GetAccountByIdAsync(loggedUserId);
            var IsCorrect = _authService.VerifyPassword(dto.OldPassword, account.Password, account.Salt);

            if (!IsCorrect)
            {
                throw new ValidationException("Invalid old password!");
            }

            var result = _accountRequestDtoForPasswordUpdateValidator.Validate(dto);

            _validationExtension.ValidateValidationResult(result);

            var newHashedPassword = _authService.HashPasword(dto.NewPassword, out var salt);

            await _accountRepository.UpdatePasswordAsync(loggedUserId, newHashedPassword, salt);
        }
    }
}
