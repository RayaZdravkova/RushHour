using Microsoft.EntityFrameworkCore;
using RushHour.Domain.DTOs.Accounts;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Exceptions;
using AutoMapper;

namespace RushHour.Persistance.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        protected readonly RushHourDBContext context;
        private readonly IMapper _mapper;

        public AccountRepository(RushHourDBContext context, IMapper mapper)
        {
            this.context = context;
            this._mapper = mapper;
        }

        public async Task<AccountDto> GetAccountByEmailAsync(string email)
        {
            var entity = await context.Accounts.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

            if (entity is null)
            {
                throw new NotFoundException("Account with this email was not found!");
            }

            AccountDto responseDto = _mapper.Map<AccountDto>(entity);

            return responseDto;
        }
        public async Task<AccountDto> GetAccountByIdAsync(int id)
        {
            var entity = await context.Accounts.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            if (entity is null)
            {
                throw new NotFoundException("Account with this id was not found!");
            }

            AccountDto responseDto = _mapper.Map<AccountDto>(entity);

            return responseDto;
        }

        public async Task UpdatePasswordAsync(int id, string newPassword, byte[] salt)
        {
            var entity = await context.Accounts
             .FirstOrDefaultAsync(e => e.Id == id);

            entity.Id = id;
            entity.Password = newPassword;
            entity.Salt = salt;

            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }
    }
}
