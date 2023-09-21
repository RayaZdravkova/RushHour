using FluentValidation;
using Moq;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Accounts;
using RushHour.Domain.Enums;
using RushHour.Domain.FluentValidations.Accounts;
using RushHour.Domain.Services;
using ValidationException = RushHour.Domain.Exceptions.ValidationException;

namespace RushHour.Services.Tests
{
    public class AccountServiceTest
    {
        private readonly Mock<IAccountRepository> repositoryMock;
        private readonly Mock<IAuthService> authService;
        private readonly Mock<IHttpContextAccessorWrapper> httpContextAccessor;
        private readonly Mock<IValidationExtension> validationExtension;
        private readonly IValidator<AccountRequestDtoForPasswordUpdate> accountRequestDtoForPasswordUpdateValidator;

        public AccountServiceTest()
        {
            repositoryMock = new Mock<IAccountRepository>();
            authService = new Mock<IAuthService>();
            httpContextAccessor = new Mock<IHttpContextAccessorWrapper>();
            validationExtension = new Mock<IValidationExtension>();
            accountRequestDtoForPasswordUpdateValidator = new AccountRequestDtoForPasswordUpdateValidator();
        }

        [Fact]
        public async Task ChangePassword_SuccessfullyChangePassword_ChangePasswordWasInvockedOnce()
        {
            var loggedUserId = 10;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(loggedUserId);

            var service = new AccountService(repositoryMock.Object, httpContextAccessor.Object, authService.Object, validationExtension.Object, accountRequestDtoForPasswordUpdateValidator);

            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };
            var newHashedPassword = "aaaaaaa123";

            AccountRequestDtoForPasswordUpdate requestDto = new AccountRequestDtoForPasswordUpdate()
            {
                OldPassword = "Raya123!",
                NewPassword = "Raya123!+"
            };

            AccountDto? responseDto = new AccountDto()
            {
                Email = "raya@prime.com",
                FullName = "Raya",
                Password = "Raya123!",
                Username = "raya",
                Role = Roles.EMPLOYEE,
                Salt = salt
            };

            repositoryMock.Setup(s => s.GetAccountByIdAsync(loggedUserId))
            .ReturnsAsync(responseDto);

            authService.Setup(s => s.VerifyPassword(requestDto.OldPassword, responseDto.Password, responseDto.Salt))
            .Returns(true);

            authService.Setup(s => s.HashPasword(requestDto.NewPassword, out salt))
            .Returns(newHashedPassword);

            await service.ChangePassword(requestDto);

            repositoryMock.Verify(x => x.UpdatePasswordAsync(loggedUserId, newHashedPassword, salt), Times.Once);
        }

        [Fact]
        public void ChangePassword_InvalidOldPassword_ThrowsValidationException()
        {
            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(10);

            var loggedUserId = 10;
            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };
            var newHashedPassword = "aaaaaaaa123";

            var service = new AccountService(repositoryMock.Object, httpContextAccessor.Object, authService.Object, validationExtension.Object, accountRequestDtoForPasswordUpdateValidator);

            AccountRequestDtoForPasswordUpdate requestDto = new AccountRequestDtoForPasswordUpdate()
            {
                OldPassword = "Raya123!",
                NewPassword = "Raya123!+"
            };

            AccountDto? responseDto = new AccountDto()
            {
                Email = "raya@prime.com",
                FullName = "Raya",
                Password = "Raya123!",
                Username = "raya",
                Role = Roles.EMPLOYEE,
                Salt = salt
            };

            repositoryMock.Setup(s => s.GetAccountByIdAsync(loggedUserId))
            .ReturnsAsync(responseDto);

            authService.Setup(s => s.VerifyPassword(requestDto.OldPassword, responseDto.Password, responseDto.Salt))
            .Returns(false);

            Assert.ThrowsAsync<ValidationException>(() => service.ChangePassword(requestDto));
        }
    }
}
