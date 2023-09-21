using Microsoft.EntityFrameworkCore;
using RushHour.Domain.DTOs.Providers;
using RushHour.Persistance.Entities;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Pagination;
using Microsoft.Data.SqlClient;
using RushHour.Domain.Exceptions;
using AutoMapper;

namespace RushHour.Persistance.Repositories
{
    public class ProviderRepository : IProviderRepository
    {
        protected readonly RushHourDBContext context;
        private readonly IMapper _mapper;

        public ProviderRepository(RushHourDBContext context, IMapper mapper)
        {
            this.context = context;
            this._mapper = mapper;
        }

        public async Task<ProviderResponseDto> CreateAsync(ProviderRequestDto dto)
        {
            try
            {
                Provider entity = _mapper.Map<Provider>(dto);

                var entityEntry = context.Providers.Add(entity);
                await context.SaveChangesAsync();

                ProviderResponseDto responseDto = _mapper.Map<ProviderResponseDto>(entityEntry.Entity);

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
                            throw new ValidationException("The name and the business domain should be unique!");
                        case 2601:  // Duplicated key row error
                            throw new ValidationException("A provider with that name or business domain already exists!");
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

        public async Task<ProviderResponseDto> UpdateAsync(int id, ProviderRequestDto dto)
        {
            var entity = await context.Providers
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
            {
                throw new NotFoundException($"Provider was not found!");
            }

            try
            {
                _mapper.Map(dto, entity);
                entity.Id = id;

                context.Entry(entity).State = EntityState.Modified;
                await context.SaveChangesAsync();

                ProviderResponseDto responseDto = _mapper.Map<ProviderResponseDto>(entity);

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
                            throw new ValidationException("The name and the business domain should be unique!");
                        case 2601:  // Duplicated key row error
                            throw new ValidationException("A provider with that name or business domain already exists!");
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
            return await context.Providers.AnyAsync(p => p.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    Provider? entityForDeletion = await context.Providers.FindAsync(id);

                    if (entityForDeletion is null)
                    {
                        throw new NotFoundException($"Provider was not found!");
                    }

                    var employeesForDeletion = context.Employees
                             .Include(e => e.Account)
                             .Where(p => p.Provider.Id == id);

                    var activitiesForDeletion = context.Activities
                             .Where(p => p.Provider.Id == id);

                    var activityEmployees = context.ActivityEmployees
                         .Where(ae => employeesForDeletion
                         .Any(x => x.Id == ae.EmployeeId));

                    var appointments = context.Appointments
                         .Where(ae => activitiesForDeletion
                         .Any(x => x.Id == ae.Activity.Id));

                    context.ActivityEmployees.RemoveRange(activityEmployees);

                    var accounts = context.Accounts
                        .Where(a => employeesForDeletion
                        .Any(e => e.Account.Id == a.Id));

                    context.Appointments.RemoveRange(appointments);

                    context.Activities.RemoveRange(activitiesForDeletion);

                    context.Employees.RemoveRange(employeesForDeletion);

                    context.Accounts.RemoveRange(accounts);

                    context.Providers.Remove(entityForDeletion);
                    await context.SaveChangesAsync();

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

        }

        public async Task<ProviderResponseDto> GetByIdAsync(int id)
        {
            Provider? entity = await context.Providers.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (entity is null)
            {
                throw new NotFoundException("Provider was not found!");
            }

            ProviderResponseDto responseDto = _mapper.Map<ProviderResponseDto>(entity);

            return responseDto;
        }

        public async Task<List<ProviderResponseDto>> GetAllAsync(PagingInfo pagingInfo)
        {
            List<Provider> entities = await context.Providers
                .AsNoTracking()
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize)
                .ToListAsync();

            var responseDtos = new List<ProviderResponseDto>();

            foreach (var entity in entities)
            {
                ProviderResponseDto responseDto = _mapper.Map<ProviderResponseDto>(entity);

                responseDtos.Add(responseDto);
            }
            return responseDtos;
        }
    }
}
