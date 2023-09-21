using Microsoft.EntityFrameworkCore;
using RushHour.Domain.DTOs.Activities;
using RushHour.Persistance.Entities;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Pagination;
using RushHour.Domain.Exceptions;
using AutoMapper;

namespace RushHour.Persistance.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        protected readonly RushHourDBContext context;
        private readonly IMapper _mapper;
        public ActivityRepository(RushHourDBContext context, IMapper mapper)
        {
            this.context = context;
            this._mapper = mapper;
        }

        public async Task<ActivityResponseDto> CreateAsync(ActivityRequestDto dto, int loggedUserProviderId)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var provider = await context.Providers.FindAsync(loggedUserProviderId);

                    if (provider is null)
                    {
                        throw new NotFoundException($"Provider was not found!");
                    }

                    var employeeCount = context.Employees.Where(e => dto.EmployeeIds.Contains(e.Id)).Count();

                    if (employeeCount != dto.EmployeeIds.Count())
                    {
                        throw new NotFoundException($"One or more employee/s ids were not found!");
                    }

                    Activity entity = _mapper.Map<Activity>(dto);
                    entity.Provider = provider;

                    var entityEntry = context.Activities.Add(entity);
                    await context.SaveChangesAsync();

                    foreach (var employeeId in dto.EmployeeIds)
                    {
                        var activityEmployeeEntity = new ActivityEmployee();

                        activityEmployeeEntity.ActivityId = entityEntry.Entity.Id;
                        activityEmployeeEntity.EmployeeId = employeeId;

                        context.ActivityEmployees.Add(activityEmployeeEntity);
                    }

                    await context.SaveChangesAsync();

                    transaction.Commit();

                    ActivityResponseDto responseDto = _mapper.Map<ActivityResponseDto>(entityEntry.Entity);

                    responseDto.EmployeeIds = dto.EmployeeIds;

                    return responseDto;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            
            }
        }

        public async Task<ActivityResponseDto> UpdateAsync(int id, ActivityRequestDto dto)
        {
            var entity = await context.Activities
                .Include(a => a.Provider)
            .FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
            {
                throw new NotFoundException($"Activity was not found!");
            }

            var employees = await context.Employees.Where(e => dto.EmployeeIds.Contains(e.Id)).ToListAsync();

            if (employees.Count() != dto.EmployeeIds.Count())
            {
                throw new NotFoundException($"One or more employee/s ids were not found!");
            }

            _mapper.Map(dto, entity);
            entity.Id = id;

            context.Entry(entity).State = EntityState.Modified;
        
            foreach (var employeeId in dto.EmployeeIds)
            {
                if (!context.ActivityEmployees.Any(ae => ae.ActivityId == entity.Id && ae.EmployeeId == employeeId))
                {
                    var activityEmployeeEntity = new ActivityEmployee();
                    activityEmployeeEntity.ActivityId = entity.Id;
                    activityEmployeeEntity.EmployeeId = employeeId;

                    context.ActivityEmployees.Add(activityEmployeeEntity);
                }

            }

            var baseEmployees = context.ActivityEmployees.Where(ae => ae.ActivityId == id);
            foreach (var baseEmplo in baseEmployees)
            {
                if (!dto.EmployeeIds.Contains(baseEmplo.EmployeeId))
                {
                    context.ActivityEmployees.Remove(baseEmplo);
                }
            }

            await context.SaveChangesAsync();

            ActivityResponseDto responseDto = _mapper.Map<ActivityResponseDto>(entity);
            responseDto.ProviderId = entity.Provider.Id;
            responseDto.EmployeeIds = dto.EmployeeIds;

            return responseDto;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.Activities.AnyAsync(p => p.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            Activity? entityForDeletion = await context.Activities.FindAsync(id);

            if (entityForDeletion is null)
            {
                throw new NotFoundException($"Activity was not found!");
            }
            var appointmentsForDeletion = context.Appointments.Where(a => a.Activity.Id == id);

            context.Appointments.RemoveRange(appointmentsForDeletion);
            context.Activities.Remove(entityForDeletion);
            await context.SaveChangesAsync();
        }

        public async Task<ActivityResponseDto> GetByIdAsync(int id)
        {
            Activity? entity = await context.Activities
                .AsNoTracking()
                .Include(a => a.Provider)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (entity is null)
            {
                throw new NotFoundException("Activity was not found!");
            }

            ActivityResponseDto responseDto = _mapper.Map<ActivityResponseDto>(entity);

            responseDto.ProviderId = entity.Provider.Id;
            responseDto.EmployeeIds = context.ActivityEmployees.Where(ae => ae.ActivityId == id).Select(ae => ae.EmployeeId).ToList();

            return responseDto;
        }

        public async Task<List<ActivityResponseDto>> GetAllAsync(PagingInfo pagingInfo)
        {
       
            List<Activity> entities = await context.Activities
                .AsNoTracking()
                //.Include(a => a.Employees)
                .Include(a => a.Provider)
                .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize)
                .ToListAsync();

            var responseDtos = new List<ActivityResponseDto>();

            foreach (var entity in entities)
            {
                ActivityResponseDto responseDto = _mapper.Map<ActivityResponseDto>(entity);

                responseDto.ProviderId = entity.Provider.Id;
                responseDto.EmployeeIds = context.ActivityEmployees.Where(ae => ae.ActivityId == entity.Id).Select(ae => ae.EmployeeId).ToList();

                responseDtos.Add(responseDto);
            }

            return responseDtos;
        }

        public async Task<int> GetActivityProviderAsync(int activityId)
        {
            var activityProvider = await context.Activities
                .Where(a => a.Id == activityId)
                .Select(a => a.Provider.Id)
                .FirstOrDefaultAsync();
            return activityProvider;
        }
        public bool CheckIfEmployeeIsPartOfActivities(int employeeId, List<int> activityIds)
        {
            return context.ActivityEmployees.Where(e => activityIds.Contains(e.ActivityId) && e.EmployeeId == employeeId).Count() == activityIds.Count();

        }
        public bool CheckIfEmployeeIsPartOfActivity(int emloyeeId, int activityId)
        {
            return context.ActivityEmployees.Where(e => e.ActivityId == activityId).Select(e => e.EmployeeId).Contains(emloyeeId);

        }
    }
}
