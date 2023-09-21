using Microsoft.EntityFrameworkCore;
using RushHour.Domain.DTOs.Employees;
using RushHour.Persistance.Entities;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Pagination;
using Microsoft.Data.SqlClient;
using RushHour.Domain.Exceptions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Principal;

namespace RushHour.Persistance.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        protected readonly RushHourDBContext context;
        private readonly IMapper _mapper;
        public EmployeeRepository(RushHourDBContext context, IMapper mapper)
        {
            this.context = context;
            this._mapper = mapper;
        }
        public async Task<bool> DoesEmployeeMatchAccountAsync(int employeeId, int accountId)
        {
            return await context.Employees
                .AnyAsync(u => u.Id == employeeId && u.Account.Id == accountId);
        }

        public async Task<bool> IsAccountPartOfProviderAsync(int accountId, int providerId)
        {
            return await context.Employees
                .AnyAsync(u => u.Account.Id == accountId && u.Provider.Id == providerId);
        }

        public async Task<bool> AreEmployeeAndAccountPartOfTheSameProviderAsync(int accountId, int employeeId)
        {
            var employeeProviders = context.Employees.Where(u => u.Account.Id == accountId).Select(e => e.Provider.Id);

            var employees = context.Employees
           .Where(e => employeeProviders
           .Contains(e.Provider.Id));

            return await employees.AnyAsync(e => e.Id == employeeId);
        }

        public async Task<EmployeeResponseDto> GetEmployeeByAccountIdAsync(int id)
        {
            var entity = await context.Employees.AsNoTracking().Include(o => o.Provider).FirstOrDefaultAsync(u => u.Account.Id == id);

            if (entity is null)
            {
                throw new NotFoundException("Employee with this account was not found!");
            }

            var responseDto = new EmployeeResponseDto();

            responseDto.Id = entity.Id;
            responseDto.Title = entity.Title;
            responseDto.RatePerHour = entity.RatePerHour;
            responseDto.ProviderId = entity.Provider.Id;
            responseDto.HireDate = entity.HireDate;
            responseDto.Phone = entity.Phone;

            return responseDto;
        }
        public async Task<int> GetEmployeeProviderIdByAccountIdAsync(int id)
        {
            var loggedUserProvider = await context.Employees
                .Where(o => o.Account.Id == id)
                .Select(a => a.Provider.Id)
                .FirstOrDefaultAsync();

            return loggedUserProvider;
        }

        public bool CheckEmployeeProvidersPartOfTheSameProvider(List<int> employeeIds, int loggedUserProviderId)
        {
            return context.Employees.Where(e => employeeIds.Contains(e.Id))
                .All(ep => ep.Provider.Id == loggedUserProviderId);
        }

        public async Task<bool> CheckIfEmployeeIsFreeInSpecificTimeAsync(DateTime startTime, int employeeId, List<int> activityIds)
        {
            var provider = await context.Employees.Where(e => e.Id == employeeId).Select(a => a.Provider).FirstOrDefaultAsync();

            var startTimeProvider = provider.StartTimeOfTheWorkingDay;
            var endTimeProvider = provider.EndTimeOfTheWorkingDay;
            var workingDays = provider.WorkingDays.ToString();

            if (!workingDays.Contains(startTime.DayOfWeek.ToString()))
            {
                return false;
            }

            DateTime endTime = new DateTime();

            var durationSum = await context.Activities.Where(a => activityIds.Contains(a.Id)).SumAsync(a => a.Duration);
            endTime = startTime.AddMinutes(durationSum);

            if (!(startTimeProvider.TimeOfDay <= startTime.TimeOfDay && endTime.TimeOfDay <= endTimeProvider.TimeOfDay))
            {
                return false;
            }

            return !(await context.Appointments
                .Where(a => a.Employee.Id == employeeId)
                .AnyAsync(a => startTime < a.EndDate && endTime > a.StartDate));
        }

        public async Task<bool> CheckIfEmployeeIsFreeInSpecificTimeForUpdateAsync(int appointmentId, DateTime startTime, int employeeId, int activityId)
        {
            var appointment = await context.Appointments.AsNoTracking().Where(a => a.Id == appointmentId).Select(a => new { ActivityId = a.Activity.Id, StartDate = a.StartDate, EmployeeId = a.Employee.Id }).FirstOrDefaultAsync();
            var provider = await context.Employees.Where(e => e.Id == employeeId).Select(a => a.Provider).FirstOrDefaultAsync();

            var startTimeProvider = provider.StartTimeOfTheWorkingDay;
            var endTimeProvider = provider.EndTimeOfTheWorkingDay;
            var workingDays = provider.WorkingDays.ToString();

            if (!workingDays.Contains(startTime.DayOfWeek.ToString()))
            {
                return false;
            }
            if (activityId != appointment.ActivityId || startTime != appointment.StartDate || employeeId != appointment.EmployeeId)
            {
                DateTime endTime = new DateTime();

                var duration = await context.Activities.Where(a => a.Id == activityId).Select(a => a.Duration).FirstOrDefaultAsync();
                endTime = startTime.AddMinutes(duration);

                if (!(startTimeProvider.TimeOfDay <= startTime.TimeOfDay && endTime.TimeOfDay <= endTimeProvider.TimeOfDay))
                {
                    return false;
                }

                return !(await context.Appointments
                    .Where(a => a.Employee.Id == employeeId && a.Id != appointmentId)
                    .AnyAsync(a => startTime < a.EndDate && endTime > a.StartDate));
            }
            return true;
        }

        public async Task<EmployeeResponseDto> CreateAsync(EmployeeRequestDto dto, byte[] salt)
        {
            try
            {
                var provider = await context.Providers.FindAsync(dto.ProviderId);

                Employee entity = _mapper.Map<Employee>(dto);
                entity.Provider = provider;
                entity.Account.Salt = salt;

                var entityEntry = context.Employees.Add(entity);
                await context.SaveChangesAsync();

                EmployeeResponseDto responseDto = _mapper.Map<EmployeeResponseDto>(entityEntry.Entity);

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

        public async Task<EmployeeResponseDto> UpdateAsync(int id, EmployeeRequestDtoForUpdate dto)
        {
            var entity = await context.Employees
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
            {
                throw new NotFoundException($"Employee was not found!");
            }

            var provider = await context.Providers.FindAsync(dto.ProviderId);

            if (provider is null)
            {
                throw new NotFoundException($"Provider was not found!");
            }

            try
            {
                _mapper.Map(dto,entity);
                entity.Id = id;
                entity.Provider = provider;

                context.Entry(entity).State = EntityState.Modified;
                await context.SaveChangesAsync();

                EmployeeResponseDto responseDto = _mapper.Map<EmployeeResponseDto>(entity);

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
            return await context.Employees.AnyAsync(p => p.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    Employee? entityForDeletion = await context.Employees
                               .Include(e => e.Account)
                               .FirstOrDefaultAsync(e => e.Id == id);

                    if (entityForDeletion is null)
                    {
                        throw new NotFoundException($"Employee was not found!");
                    }

                    var appointments = await context.Appointments.Where(x => x.Employee.Id == id).ToListAsync();

                    context.Appointments.RemoveRange(appointments);

                    var activityEmployees = await context.ActivityEmployees.Where(x => x.EmployeeId == id).ToListAsync();

                    context.ActivityEmployees.RemoveRange(activityEmployees);

                    context.Employees.Remove(entityForDeletion);

                    var account = await context.Accounts.FirstOrDefaultAsync(x => x.Id == entityForDeletion.Account.Id);
                    if (account != null)
                    {
                        context.Accounts.Remove(account);
                    }

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

        public async Task<EmployeeResponseDto> GetByIdAsync(int id)
        {
            Employee? entity = await context.Employees
                .AsNoTracking()
                .Include(e => e.Provider)
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
            {
                throw new NotFoundException("Employee was not found!");
            }

            EmployeeResponseDto responseDto = _mapper.Map<EmployeeResponseDto>(entity);

            return responseDto;
        }

        public async Task<List<EmployeeResponseDto>> GetAllAsync(PagingInfo pagingInfo)
        {
            List<Employee> entities = await context.Employees
               .AsNoTracking()
               .Include(e => e.Provider)
               .Include(e => e.Account)
               .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
               .Take(pagingInfo.PageSize)
               .ToListAsync();

            var responseDtos = new List<EmployeeResponseDto>();

            foreach (var entity in entities)
            {
                EmployeeResponseDto responseDto = _mapper.Map<EmployeeResponseDto>(entity);

                responseDtos.Add(responseDto);
            }

            return responseDtos;
        }
    }
}
