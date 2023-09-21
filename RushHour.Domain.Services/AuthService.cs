using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Accounts;
using RushHour.Domain.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using ValidationException = RushHour.Domain.Exceptions.ValidationException;

namespace RushHour.Domain.Services
{
    public class AuthService : IAuthService
    {
        const int keySize = 64;
        const int iterations = 350000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

        private readonly IAccountRepository _accountRepository;
        public readonly IConfiguration _configuration;
        public readonly IRfc2898DeriveBytesWrapper _rfc2898DeriveBytesWrapper;
        public readonly IConvertWrapper _convertWrapper;
        public readonly IEncodingWrapper _encodingWrapper;
        public readonly IRandomNumberGeneratorWrapper _randomNumberGeneratorWrapper;
        public readonly IEnumerableWrapper _enumerablequalWrapper;
        public readonly IDateTimeWrapper _dateTimeWrapper;
        public readonly IGuidWrapper _guidWrapper;
        private readonly IValidationExtension _validationExtension;
        private readonly IValidator<AccountRequestDtoForLogin> _accountRequestDtoForLoginValidator;
        public AuthService(IAccountRepository accountRepository, IConfiguration config, IRfc2898DeriveBytesWrapper rfc2898DeriveBytesWrapper, IConvertWrapper convertWrapper, 
            IEncodingWrapper encodingWrapper, IRandomNumberGeneratorWrapper randomNumberGeneratorWrapper, IEnumerableWrapper enumerablequalWrapper, IDateTimeWrapper dateTimeWrapper,
            IGuidWrapper guidWrapper, IValidationExtension validationExtension, IValidator<AccountRequestDtoForLogin> accountRequestDtoForLoginValidator)
        {
            _accountRepository = accountRepository;
            _configuration = config;
            _rfc2898DeriveBytesWrapper = rfc2898DeriveBytesWrapper;
            _convertWrapper = convertWrapper;
            _encodingWrapper = encodingWrapper;
            _randomNumberGeneratorWrapper = randomNumberGeneratorWrapper;
            _enumerablequalWrapper = enumerablequalWrapper;
            _dateTimeWrapper = dateTimeWrapper;
            _guidWrapper = guidWrapper;
            _validationExtension = validationExtension;
            _accountRequestDtoForLoginValidator = accountRequestDtoForLoginValidator;
        }
        public async Task<string> GetTokenAsync(AccountRequestDtoForLogin dto)
        {
            try
            {
                var result = _accountRequestDtoForLoginValidator.Validate(dto);

                _validationExtension.ValidateValidationResult(result);

                var account = await _accountRepository.GetAccountByEmailAsync(dto.Email);

                var IsCorrect = VerifyPassword(dto.Password, account.Password, account.Salt);

                if (!IsCorrect)
                {
                    throw new ValidationException("Invalid email and/or password!");
                }

                var claims = new[] {
                            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                            new Claim(JwtRegisteredClaimNames.Jti, _guidWrapper.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.Iat, _dateTimeWrapper.UtcNow().ToString()),
                            new Claim(ClaimTypes.Role, account.Role.ToString())
                };

                var key = new SymmetricSecurityKey(_encodingWrapper.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: _dateTimeWrapper.UtcNow().AddMinutes(_convertWrapper.ToDouble(_configuration["Jwt:ExpirationTime"])),
                    signingCredentials: signIn);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (NotFoundException)
            {
                throw new ValidationException("Invalid email and/or password!");
            }
        }
        public string HashPasword(string password, out byte[] salt)
        {
            salt = _randomNumberGeneratorWrapper.GetBytes(keySize);
            var hash = _rfc2898DeriveBytesWrapper.Pbkdf2(
                 password,
                 salt,
                 iterations,
                 hashAlgorithm,
                 keySize);
            return _convertWrapper.ToHexString(hash);
        }

        public bool VerifyPassword(string password, string hash, byte[] salt)
        {
            var hashToCompare = _rfc2898DeriveBytesWrapper.Pbkdf2(
                 password,
                 salt,
                 iterations,
                 hashAlgorithm,
                 keySize);

            return _enumerablequalWrapper.SequenceEqual(hash, hashToCompare);
        }

    }

}
