using Microsoft.EntityFrameworkCore;
using RushHour.Domain.DTOs.Clients;
using RushHour.Persistance.Entities;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Pagination;
using Microsoft.Data.SqlClient;
using RushHour.Domain.Exceptions;
using AutoMapper;

namespace RushHour.Persistance.Repositories
{
    public class ClientRepository : IClientRepository
    {
        protected readonly RushHourDBContext context;
        private readonly IMapper _mapper;
        public ClientRepository(RushHourDBContext context, IMapper mapper)
        {
            this.context = context;
            this._mapper = mapper;
        }

        public async Task<ClientResponseDto> CreateAsync(ClientRequestDto dto, byte[] salt)
        {
            try
            {
                Client entity = _mapper.Map<Client>(dto);
                entity.Account.Salt = salt;
                entity.Account.Role = Domain.Enums.Roles.CLIENT;

                var entityEntry = context.Clients.Add(entity);
                await context.SaveChangesAsync();

                ClientResponseDto responseDto = _mapper.Map<ClientResponseDto>(entityEntry.Entity);

                return responseDto;
            }
            catch (Exception ex)
            {
                if (ex.GetBaseException() is SqlException)
                {
                    SqlException? innerException = ex.InnerException as SqlException;

                    int ErrorCode = innerException.Number;
                    switch (ErrorCode)
                    {
                        case 2627:  // Unique constraint error
                            throw new ValidationException("The email should be unique!");
                        case 2601:  // Duplicated key row error
                            throw new ValidationException("An account with this email already exists!");
                        default:
                            throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<ClientResponseDto> UpdateAsync(int id, ClientRequestDtoForUpdate dto)
        {
            var entity = await context.Clients
               .Include(e => e.Account)
               .FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
            {
                throw new NotFoundException($"Client was not found!");
            }

            try
            {
                _mapper.Map(dto, entity);

                entity.Id = id;

                context.Entry(entity).State = EntityState.Modified;
                await context.SaveChangesAsync();

                ClientResponseDto responseDto = _mapper.Map<ClientResponseDto>(entity);

                return responseDto;
            }
            catch (Exception ex)
            {
                if (ex.GetBaseException() is SqlException)
                {
                    SqlException? innerException = ex.InnerException as SqlException;

                    int ErrorCode = innerException.Number;
                    switch (ErrorCode)
                    {
                        case 2627:  // Unique constraint error
                            throw new ValidationException("The email should be unique!");
                        case 2601:  // Duplicated key row error
                            throw new ValidationException("An account with this email already exists!");
                        default:
                            throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.Clients.AnyAsync(p => p.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            Client? entityForDeletion = await context.Clients
              .Include(e => e.Account)
              .FirstOrDefaultAsync(e => e.Id == id);

            if (entityForDeletion is null)
            {
                throw new NotFoundException($"Client was not found!");
            }

            var accountForDeletion = entityForDeletion.Account;

            context.Clients.Remove(entityForDeletion);
            context.Accounts.Remove(accountForDeletion);

            await context.SaveChangesAsync();
        }

        public async Task<ClientResponseDto> GetByIdAsync(int id)
        {
            Client? entity = await context.Clients
                .AsNoTracking()
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (entity is null)
            {
                throw new NotFoundException("Client was not found!");
            }

            ClientResponseDto responseDto = _mapper.Map<ClientResponseDto>(entity);

            return responseDto;
        }

        public async Task<List<ClientResponseDto>> GetAllAsync(PagingInfo pagingInfo)
        {
            List<Client> entities = await context.Clients
              .AsNoTracking()
              .Include(c => c.Account)
              .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
              .Take(pagingInfo.PageSize)
              .ToListAsync();

            var responseDtos = new List<ClientResponseDto>();

            foreach (var entity in entities)
            {
                ClientResponseDto responseDto = _mapper.Map<ClientResponseDto>(entity);

                responseDtos.Add(responseDto);
            }

            return responseDtos;
        }


        public async Task<bool> DoesClientMatchAccountAsync(int clientId, int accountId)
        {
            return await context.Clients.Include(c => c.Account)
                .AnyAsync(c => c.Id == clientId && c.Account.Id == accountId);
        }
    }
}
