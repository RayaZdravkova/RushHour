using RushHour.Domain.DTOs.Accounts;

namespace RushHour.Domain.Abstractions.Services
{
    public interface IAuthService
    {
       string HashPasword(string password, out byte[] salt);

       bool VerifyPassword(string password, string hash, byte[] salt);

       Task<string> GetTokenAsync(AccountRequestDtoForLogin dto);
    }
}
