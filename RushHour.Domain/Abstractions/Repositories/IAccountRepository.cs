using RushHour.Domain.DTOs.Accounts;
namespace RushHour.Domain.Abstractions.Repositories
{
    public interface IAccountRepository
    {
        //Task<AccountResponseDto> CreateAsync(AccountRequestDto dto);
        //Task DeleteAsync(int id);
        //Task<bool> ExistsAsync(int id);
        //Task<List<AccountResponseDto>> GetAllAsync(PagingInfo pagingInfo);
        //Task<AccountResponseDto?> GetByIdAsync(int id);
        //Task<AccountResponseDto> UpdateAsync(int id, AccountRequestDto dto);
        Task<AccountDto> GetAccountByEmailAsync(string email);
        Task<AccountDto> GetAccountByIdAsync(int id);
        Task UpdatePasswordAsync(int id, string newPassword, byte[] salt);
    }
}
