using FluentValidation;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Appointments;
using RushHour.Domain.Enums;
using RushHour.Domain.Exceptions;
using RushHour.Domain.Pagination;
using ValidationException = RushHour.Domain.Exceptions.ValidationException;

namespace RushHour.Domain.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IClientRepository _clientRepository;
        private IHttpContextAccessorWrapper _httpContextAccessor;
        private readonly IValidationExtension _validationExtension;
        private readonly IValidator<AppointmentRequestDto> _appointmentRequestDtoValidator;
        private readonly IValidator<AppointmentCreateRequestDto> _appointmentCreateRequestDtoValidator;
        string? userRole;

        public AppointmentService(IAppointmentRepository appointmentRepository, IActivityRepository activityRepository, IClientRepository clientRepository, IEmployeeRepository employeeRepository,
            IHttpContextAccessorWrapper httpContextAccessor, IValidationExtension validationExtension, IValidator<AppointmentRequestDto> appointmentRequestDtoValidator, IValidator<AppointmentCreateRequestDto> appointmentCreateRequestDtoValidator)
        {
            _appointmentRepository = appointmentRepository;
            _activityRepository = activityRepository;
            _employeeRepository = employeeRepository;
            _clientRepository = clientRepository;
            _httpContextAccessor = httpContextAccessor;
            _validationExtension = validationExtension;
            _appointmentRequestDtoValidator = appointmentRequestDtoValidator;
            _appointmentCreateRequestDtoValidator = appointmentCreateRequestDtoValidator;

            userRole = _httpContextAccessor.GetUserRole();
        }

        public async Task<AppointmentsAndPriceResponseDto> CreateAsync(AppointmentCreateRequestDto dto)
        {
            await AuthorizeEmployeeCreationAsync(dto.EmployeeId);
            await AuthorizeClientCreationAsync(dto.ClientId);
            await AuthorizeProviderAdminAsync(dto.EmployeeId);
            CheckIfEmployeeIsPartOfActivities(dto.EmployeeId, dto.ActivityIds);
            await CheckIfEmployeeIsFreeInSpecificTimeAsync(dto.StartDate, dto.EmployeeId, dto.ActivityIds);

            var result = _appointmentCreateRequestDtoValidator.Validate(dto);

            _validationExtension.ValidateValidationResult(result);

            AppointmentsAndPriceResponseDto createdAppointments = await _appointmentRepository.CreateAsync(dto);

            return createdAppointments;
        }

        public async Task<AppointmentResponseDto> UpdateAsync(int id, AppointmentRequestDto dto)
        {
            await AuthorizeEmployeeAsync(id);
            await AuthorizeClientAsync(id);
            await AuthorizeProviderAdminForUpdateAndDeleteAndGetByIdAsync(id);
            await AuthorizeProviderAdminAsync(dto.EmployeeId);
            CheckIfEmployeeIsPartOfActivity(dto.EmployeeId, dto.ActivityId);
            await CheckIfEmployeeIsFreeInSpecificTimeForUpdateAsync(id, dto.StartDate, dto.EmployeeId, dto.ActivityId);

            var result = _appointmentRequestDtoValidator.Validate(dto);

            _validationExtension.ValidateValidationResult(result);

            AppointmentResponseDto updatedAppointment = await _appointmentRepository.UpdateAsync(id, dto);

            return updatedAppointment;
        }

        public async Task DeleteAsync(int id)
        {
            await AuthorizeEmployeeAsync(id);
            await AuthorizeClientAsync(id);
            await AuthorizeProviderAdminForUpdateAndDeleteAndGetByIdAsync(id);

            await _appointmentRepository.DeleteAsync(id);
        }

        public async Task<AppointmentResponseDto> GetByIdAsync(int id)
        {
            await AuthorizeEmployeeAsync(id);
            await AuthorizeClientAsync(id);
            await AuthorizeProviderAdminForUpdateAndDeleteAndGetByIdAsync(id);

            AppointmentResponseDto? appointment = await _appointmentRepository.GetByIdAsync(id);

            return appointment;
        }

        public async Task<List<AppointmentResponseDto>> GetAllAsync(PagingInfo pagingInfo)
        {
            List<AppointmentResponseDto> appointments = await _appointmentRepository.GetAllAsync(pagingInfo);

            return appointments;
        }

        private void CheckIfEmployeeIsPartOfActivities(int employeeId, List<int> activityIds)
        {
            if (!_activityRepository.CheckIfEmployeeIsPartOfActivities(employeeId, activityIds))
            {
                throw new ValidationException("One or more employees are not part of this activity!");
            }
        }

        private void CheckIfEmployeeIsPartOfActivity(int employeeId, int activityId)
        {
            if (!_activityRepository.CheckIfEmployeeIsPartOfActivity(employeeId, activityId))
            {
                throw new ValidationException("The employee is not part of this activity!");
            }
        }

        private async Task AuthorizeEmployeeCreationAsync(int employeeId)
        {
            if (userRole == Roles.EMPLOYEE.ToString())
            {
                var loggedUserId = _httpContextAccessor.GetLoggedUserId();

                if (!await _employeeRepository.DoesEmployeeMatchAccountAsync(employeeId, loggedUserId))
                {
                    throw new UnauthorizedException("You can create an appointment only if you are logged as the employee that provides the service!");
                }
            }
        }
        private async Task AuthorizeClientCreationAsync(int clientId)
        {
            if (userRole == Roles.CLIENT.ToString())
            {
                var loggedUserId = _httpContextAccessor.GetLoggedUserId();

                if (!await _clientRepository.DoesClientMatchAccountAsync(clientId, loggedUserId))
                {
                    throw new UnauthorizedException("You can create an appointment only for you!");
                }
            }
        }

        private async Task AuthorizeEmployeeAsync(int appointmentId)
        {
            if (userRole == Roles.EMPLOYEE.ToString())
            {
                var loggedUserId = _httpContextAccessor.GetLoggedUserId();

                if (!await _appointmentRepository.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, loggedUserId))
                {
                    throw new UnauthorizedException("You can perform this action only on an appointment that you are related to!");
                }
            }
        }

        private async Task AuthorizeClientAsync(int appointmentId)
        {
            if (userRole == Roles.CLIENT.ToString())
            {
                var loggedUserId = _httpContextAccessor.GetLoggedUserId();

                if (!await _appointmentRepository.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, loggedUserId))
                {
                    throw new UnauthorizedException("You can perform this action only on an appointment that you are related to!");
                }
            }
        }

        private async Task AuthorizeProviderAdminAsync(int employeeId)
        {
            if (userRole == Roles.PROVIDER_ADMIN.ToString())
            {
                var loggedUserId = _httpContextAccessor.GetLoggedUserId();

                if (!await _employeeRepository.AreEmployeeAndAccountPartOfTheSameProviderAsync(loggedUserId, employeeId))
                {
                    throw new UnauthorizedException("You can perform this action with an employee that has the same provider as you!");
                }
            }
        }

        private async Task AuthorizeProviderAdminForUpdateAndDeleteAndGetByIdAsync(int appointmentId)
        {
            if (userRole == Roles.PROVIDER_ADMIN.ToString())
            {
                var loggedUserId = _httpContextAccessor.GetLoggedUserId();

                if (!await _appointmentRepository.AreAppointmentAndAccountPartOfTheSameProviderAsync(appointmentId, loggedUserId))
                {
                    throw new UnauthorizedException("You can perform this action only on an appointment that has the same provider as you!");
                }
            }
        }

        private async Task CheckIfEmployeeIsFreeInSpecificTimeAsync(DateTime startTime, int employeeId, List<int> activityIds)
        {
            if (!await _employeeRepository.CheckIfEmployeeIsFreeInSpecificTimeAsync(startTime, employeeId, activityIds))
            {
                throw new ValidationException("The employee is busy in that time or is not working!");
            }
        }

        private async Task CheckIfEmployeeIsFreeInSpecificTimeForUpdateAsync(int appointmentId, DateTime startTime, int employeeId, int activityId)
        {
            if (!await _employeeRepository.CheckIfEmployeeIsFreeInSpecificTimeForUpdateAsync(appointmentId, startTime, employeeId, activityId))
            {
                throw new ValidationException("The employee is busy in that time or is not working!");
            }
        }
    }
}
