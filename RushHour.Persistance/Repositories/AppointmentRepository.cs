using Microsoft.EntityFrameworkCore;
using RushHour.Domain.DTOs.Appointments;
using RushHour.Persistance.Entities;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Pagination;
using RushHour.Domain.Exceptions;
using AutoMapper;

namespace RushHour.Persistance.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        protected readonly RushHourDBContext context;
        private readonly IMapper _mapper;

        public AppointmentRepository(RushHourDBContext context, IMapper mapper)
        {
            this.context = context;
            this._mapper = mapper;
        }

        public async Task<AppointmentsAndPriceResponseDto> CreateAsync(AppointmentCreateRequestDto dto)
        {
            var employee = await context.Employees.FindAsync(dto.EmployeeId);

            if (employee is null)
            {
                throw new NotFoundException($"Employee was not found!");
            }

            var client = await context.Clients.FindAsync(dto.ClientId);

            if (client is null)
            {
                throw new NotFoundException($"Client was not found!");
            }

            var activities = context.Activities
           .Where(e => dto.ActivityIds
           .Contains(e.Id)).ToList();

            if (activities.Count() != dto.ActivityIds.Count())
            {
                throw new NotFoundException($"One or more activities were not found!");
            }

            List<AppointmentResponseDto> appointmentResponseDtos = new List<AppointmentResponseDto>();

            decimal sumPrices = 0;

            var startDate = dto.StartDate;

            foreach ( var activity in activities) 
            {
                var entity = new Appointment();

                entity.StartDate = startDate;
                entity.EndDate = startDate.AddMinutes(activity.Duration);

                startDate = entity.EndDate;

                entity.Employee = employee;
                entity.Client = client;
                entity.Activity = activity;

                sumPrices += entity.Activity.Price;

                var entityEntry = context.Appointments.Add(entity);
                await context.SaveChangesAsync();

                AppointmentResponseDto responseDto = _mapper.Map<AppointmentResponseDto>(entityEntry.Entity);

                appointmentResponseDtos.Add(responseDto);
            }

            AppointmentsAndPriceResponseDto appointmentsAndPriceResponseDto = new AppointmentsAndPriceResponseDto();

            appointmentsAndPriceResponseDto.Appointments = appointmentResponseDtos;
            appointmentsAndPriceResponseDto.TotalPrice = sumPrices;

            return appointmentsAndPriceResponseDto;
        }

        public async Task<AppointmentResponseDto> UpdateAsync(int id, AppointmentRequestDto dto)
        {
            var entity = await context.Appointments
              .FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
            {
                throw new NotFoundException($"Appointment was not found!");
            }

            var employee = await context.Employees.FindAsync(dto.EmployeeId);

            if (employee is null)
            {
                throw new NotFoundException($"Employee was not found!");
            }

            var client = await context.Clients.FindAsync(dto.ClientId);

            if (client is null)
            {
                throw new NotFoundException($"Client was not found!");
            }

            var activity = await context.Activities.FindAsync(dto.ActivityId);

            if (activity is null)
            {
                throw new NotFoundException($"Activity was not found!");
            }

            _mapper.Map(dto, entity);

            entity.Id = id;
            entity.EndDate = dto.StartDate.AddMinutes(activity.Duration);
            entity.Employee = employee;
            entity.Client = client;
            entity.Activity = activity;

            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();

            AppointmentResponseDto responseDto = _mapper.Map<AppointmentResponseDto>(entity);

            return responseDto;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.Appointments.AnyAsync(p => p.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            Appointment? entityForDeletion = await context.Appointments.FindAsync(id);

            if (entityForDeletion is null)
            {
                throw new ArgumentException($"Appointment was not found!");
            }

            context.Appointments.Remove(entityForDeletion);
            await context.SaveChangesAsync();
        }

        public async Task<AppointmentResponseDto?> GetByIdAsync(int id)
        {
            Appointment? entity = await context.Appointments
                .Include(a => a.Employee)
                .Include(a => a.Client)
                .Include(a => a.Activity)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (entity is null)
            {
                throw new NotFoundException("Appointment was not found!");
            }

            AppointmentResponseDto responseDto = _mapper.Map<AppointmentResponseDto>(entity);

            return responseDto;
        }

        public async Task<List<AppointmentResponseDto>> GetAllAsync(PagingInfo pagingInfo)
        {
            List<Appointment> entities = await context.Appointments
             .Include(a => a.Employee)
             .Include(a => a.Client)
             .Include(a => a.Activity)
             .Skip((pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
             .Take(pagingInfo.PageSize)
             .ToListAsync();

            var responseDtos = new List<AppointmentResponseDto>();

            foreach (var entity in entities)
            {
                AppointmentResponseDto responseDto = _mapper.Map<AppointmentResponseDto>(entity);

                responseDtos.Add(responseDto);
            }

            return responseDtos;
        }

        public async Task<bool> DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(int appointmentId, int accountId)
        {
            return await context.Appointments
                .Where(a => a.Id == appointmentId)
                .AnyAsync(e => e.Employee.Account.Id == accountId);
        }

        public async Task<bool> AreAppointmentAndAccountPartOfTheSameProviderAsync(int appointmentId, int accountId)
        {
            var employeeProviderId = context.Appointments
                .Where(a => a.Id == appointmentId).Select(a => a.Employee.Provider.Id);

            return await context.Employees.Where(u => u.Account.Id == accountId).AnyAsync(e => employeeProviderId.Contains(e.Provider.Id));
        }

        public async Task<bool> DoesAppointmentClientAccountMatchLoggedUserAccountAsync(int appointmentId, int accountId)
        {
            return await context.Appointments
                .Where(a => a.Id == appointmentId)
                .AnyAsync(e => e.Client.Account.Id == accountId);
        }
    }
}
