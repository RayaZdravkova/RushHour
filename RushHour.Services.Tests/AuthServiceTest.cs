using FluentValidation;
using Microsoft.Extensions.Configuration;
using Moq;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Accounts;
using RushHour.Domain.Enums;
using RushHour.Domain.Exceptions;
using RushHour.Domain.FluentValidations.Accounts;
using RushHour.Domain.Services;
using System.Security.Cryptography;
using ValidationException = RushHour.Domain.Exceptions.ValidationException;

namespace RushHour.Services.Tests
{
    public class AuthServiceTest
    {
        const int keySize = 64;
        const int iterations = 350000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

        private readonly Mock<IAccountRepository> accountRepositoryMock;
        private readonly Mock<IConfiguration> configuration;
        public readonly Mock<IRfc2898DeriveBytesWrapper> rfc2898DeriveBytesWrapper;
        public readonly Mock<IConvertWrapper> convertWrapper;
        public readonly Mock<IEncodingWrapper> encodingWrapper;
        public readonly Mock<IRandomNumberGeneratorWrapper> randomNumberGeneratorWrapper;
        public readonly Mock<IEnumerableWrapper> enumerableWrapper;
        public readonly Mock<IAuthService> authService;
        public readonly Mock<IDateTimeWrapper> dateTimeWrapper;
        public readonly Mock<IGuidWrapper> guidWrapper;
        private readonly Mock<IValidationExtension> validationExtension;
        private readonly IValidator<AccountRequestDtoForLogin> accountRequestDtoForLoginValidator;

        public AuthServiceTest()
        {
            accountRepositoryMock = new Mock<IAccountRepository>();
            configuration = new Mock<IConfiguration>();
            rfc2898DeriveBytesWrapper = new Mock<IRfc2898DeriveBytesWrapper>();
            convertWrapper = new Mock<IConvertWrapper>();
            randomNumberGeneratorWrapper = new Mock<IRandomNumberGeneratorWrapper>();
            encodingWrapper = new Mock<IEncodingWrapper>();
            enumerableWrapper = new Mock<IEnumerableWrapper>();
            authService = new Mock<IAuthService>();
            dateTimeWrapper = new Mock<IDateTimeWrapper>();
            guidWrapper = new Mock<IGuidWrapper>();
            validationExtension = new Mock<IValidationExtension>();
            accountRequestDtoForLoginValidator = new AccountRequestDtoForLoginValidator();
        }

        [Fact]
        public async Task GetTokenAsync_SuccessfullyGetToken_ReturnsJWTToken()
        {
            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };
            var password = "Raya123!";
            var hashedPassword = "92FF07D5CE6B8572E2C2C4FC88275B72429FCB25B8C358CBFD77F4A595648608B1DCA58A7BC177560F40808B86F9735B6FF5432EEADA517F6576560AEACBBE23";
            byte[] hashedPasswordInBytes = { 80, 65, 78, 75, 65, 74 };
            var key = "FC97CED9-B354-4D61-ABA8-76B01685DF02";
            var issuer = "JWTAuthenticationServer";
            var audience = "JWTServicePostmanClient";
            var expirationTime = "30";
            var arraySize = 128;
            byte[] keyBytes = new byte[arraySize];

            for (int i = 0; i < 128; i++)
            {
                keyBytes[i] = (byte)i;
            }

            AccountRequestDtoForLogin requestDto = new AccountRequestDtoForLogin()
            {
                Email = "raya@prime.com",
                Password = "Raya123!"
            };

            AccountDto? responseDto = new AccountDto()
            {
                Email = "raya@prime.com",
                FullName = "Raya",
                Password = hashedPassword,
                Username = "raya",
                Role = Roles.EMPLOYEE,
                Salt = salt
            };

            accountRepositoryMock.Setup(s => s.GetAccountByEmailAsync(requestDto.Email))
            .ReturnsAsync(responseDto);

            rfc2898DeriveBytesWrapper.Setup(s => s.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize))
            .Returns(hashedPasswordInBytes);

            enumerableWrapper.Setup(s => s.SequenceEqual(hashedPassword, hashedPasswordInBytes))
            .Returns(true);

            var service = new AuthService(accountRepositoryMock.Object, configuration.Object, rfc2898DeriveBytesWrapper.Object,
            convertWrapper.Object, encodingWrapper.Object, randomNumberGeneratorWrapper.Object, enumerableWrapper.Object, dateTimeWrapper.Object,
            guidWrapper.Object, validationExtension.Object, accountRequestDtoForLoginValidator);

            configuration.SetupGet(x => x["Jwt:Key"]).Returns(key);
            guidWrapper.Setup(x => x.NewGuid()).Returns(new Guid("EB50309A-7C25-4D10-9A2B-50C9FA2E2EAF"));
            dateTimeWrapper.Setup(x => x.UtcNow()).Returns(new DateTime(2023, 4, 22, 16, 0, 0));

            encodingWrapper.Setup(s => s.GetBytes(key))
            .Returns(keyBytes);
            configuration.SetupGet(x => x["Jwt:Issuer"]).Returns(issuer);
            configuration.SetupGet(x => x["Jwt:Audience"]).Returns(audience);
            configuration.SetupGet(x => x["Jwt:ExpirationTime"]).Returns(expirationTime);

            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwIiwianRpIjoiZWI1MDMwOWEtN2MyNS00ZDEwLTlhMmItNTBjOWZhMmUyZWFmIiwiaWF0IjoiMjIvMDQvMjAyMyAxNjowMDowMCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkVNUExPWUVFIiwiZXhwIjoxNjgyMTY4NDAwLCJpc3MiOiJKV1RBdXRoZW50aWNhdGlvblNlcnZlciIsImF1ZCI6IkpXVFNlcnZpY2VQb3N0bWFuQ2xpZW50In0.mor7SxuLtkoe67xrCksR_GfzZoo9lT5pPpoDukIxui0";

            var actual = await service.GetTokenAsync(requestDto);

            Assert.Equal(token, actual);
        }

        [Fact]
        public void HashPassword_SuccessfullyHashPassword_ReturnsHashedPassword()
        {
            var password = "Raya123!";
            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };
            byte[] hashedPasswordInBytes = { 80, 65, 78, 75, 65, 74 };
            var hashedPassword = "92FF07D5CE6B8572E2C2C4FC88275B72429FCB25B8C358CBFD77F4A595648608B1DCA58A7BC177560F40808B86F9735B6FF5432EEADA517F6576560AEACBBE23";

            randomNumberGeneratorWrapper.Setup(s => s.GetBytes(keySize))
             .Returns(salt);

            rfc2898DeriveBytesWrapper.Setup(s => s.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize))
             .Returns(hashedPasswordInBytes);

            convertWrapper.Setup(s => s.ToHexString(hashedPasswordInBytes))
             .Returns(hashedPassword);

            var service = new AuthService(accountRepositoryMock.Object, configuration.Object, rfc2898DeriveBytesWrapper.Object,
            convertWrapper.Object, encodingWrapper.Object, randomNumberGeneratorWrapper.Object, enumerableWrapper.Object, dateTimeWrapper.Object,
            guidWrapper.Object, validationExtension.Object, accountRequestDtoForLoginValidator);

            var actual = service.HashPasword(password, out salt);

            Assert.Equal(hashedPassword, actual);
        }

        [Fact]
        public void VerifyPassword_SuccessfullyVerifiedPassword_ReturnsTrueOrFalse()
        {
            var password = "Raya123!";
            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };

            var hashedPassword = "92FF07D5CE6B8572E2C2C4FC88275B72429FCB25B8C358CBFD77F4A595648608B1DCA58A7BC177560F40808B86F9735B6FF5432EEADA517F6576560AEACBBE23";
            byte[] hashedPasswordInBytes = { 80, 65, 78, 75, 65, 74 };

            rfc2898DeriveBytesWrapper.Setup(s => s.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize))
            .Returns(hashedPasswordInBytes);

            enumerableWrapper.Setup(s => s.SequenceEqual(hashedPassword, hashedPasswordInBytes))
            .Returns(true);

            var service = new AuthService(accountRepositoryMock.Object, configuration.Object, rfc2898DeriveBytesWrapper.Object,
            convertWrapper.Object, encodingWrapper.Object, randomNumberGeneratorWrapper.Object, enumerableWrapper.Object, dateTimeWrapper.Object,
            guidWrapper.Object, validationExtension.Object, accountRequestDtoForLoginValidator);

            var actual = service.VerifyPassword(password, hashedPassword, salt);
            Assert.True(actual);
        }

        [Fact]
        public async Task GetTokenAsync_EmailAndPasswordAreNotValid_ThrowsValidationException()
        {
            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };

            AccountRequestDtoForLogin requestDto = new AccountRequestDtoForLogin()
            {
                Email = "raya@prime.com",
                Password = "Raya123!"
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

            var service = new AuthService(accountRepositoryMock.Object, configuration.Object, rfc2898DeriveBytesWrapper.Object,
            convertWrapper.Object, encodingWrapper.Object, randomNumberGeneratorWrapper.Object, enumerableWrapper.Object, dateTimeWrapper.Object,
            guidWrapper.Object, validationExtension.Object, accountRequestDtoForLoginValidator);

            accountRepositoryMock.Setup(s => s.GetAccountByEmailAsync(requestDto.Email))
            .ReturnsAsync(responseDto);

            authService.Setup(s => s.VerifyPassword(requestDto.Password, responseDto.Password, responseDto.Salt))
            .Returns(false);

            await Assert.ThrowsAsync<ValidationException>(() => service.GetTokenAsync(requestDto));
        }

        [Fact]
        public async Task GetTokenAsync_EmailNotFound_ThrowsValidationException()
        {
            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };

            AccountRequestDtoForLogin requestDto = new AccountRequestDtoForLogin()
            {
                Email = "raya@prime.com",
                Password = "Raya123!"
            };

            var service = new AuthService(accountRepositoryMock.Object, configuration.Object, rfc2898DeriveBytesWrapper.Object,
            convertWrapper.Object, encodingWrapper.Object, randomNumberGeneratorWrapper.Object, enumerableWrapper.Object, dateTimeWrapper.Object,
            guidWrapper.Object, validationExtension.Object, accountRequestDtoForLoginValidator);

            accountRepositoryMock.Setup(s => s.GetAccountByEmailAsync(requestDto.Email))
            .Throws<NotFoundException>();

            await Assert.ThrowsAsync<ValidationException>(() => service.GetTokenAsync(requestDto));
        }
    }
}
