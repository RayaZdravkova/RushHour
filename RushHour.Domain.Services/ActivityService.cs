using FluentValidation;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Activities;
using RushHour.Domain.Enums;
using RushHour.Domain.Exceptions;
using RushHour.Domain.Pagination;
using ValidationException = RushHour.Domain.Exceptions.ValidationException;

namespace RushHour.Domain.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _activityRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private IHttpContextAccessorWrapper _httpContextAccessor;
        private readonly IValidationExtension _validationExtension;
        private readonly IValidator<ActivityRequestDto> _activityRequestDtoValidator;
        string? userRole;

        public ActivityService(IActivityRepository activityRepository, IEmployeeRepository employeeRepository, IHttpContextAccessorWrapper httpContextAccessor,
            IValidationExtension validationExtension, IValidator<ActivityRequestDto> activityRequestDtoValidator)
        {
            _activityRepository = activityRepository;
            _employeeRepository = employeeRepository;
            _httpContextAccessor = httpContextAccessor;
            _validationExtension = validationExtension;
            _activityRequestDtoValidator = activityRequestDtoValidator;

            userRole = _httpContextAccessor.GetUserRole();
        }

        public async Task<ActivityResponseDto> CreateAsync(ActivityRequestDto dto)
        {
            var loggedUserId = _httpContextAccessor.GetLoggedUserId();
            var loggedUserProviderId = await _employeeRepository.GetEmployeeProviderIdByAccountIdAsync(loggedUserId);

            CheckEmployeeProvidersPartOfTheSameProvider(dto.EmployeeIds, loggedUserProviderId);

            var result = _activityRequestDtoValidator.Validate(dto);

            _validationExtension.ValidateValidationResult(result);

            ActivityResponseDto createdActivity = await _activityRepository.CreateAsync(dto, loggedUserProviderId);

            return createdActivity;
        }

        public async Task<ActivityResponseDto> UpdateAsync(int id, ActivityRequestDto dto)
        {
            var loggedUserId = _httpContextAccessor.GetLoggedUserId();
            var loggedUserProviderId = await _employeeRepository.GetEmployeeProviderIdByAccountIdAsync(loggedUserId);

            CheckEmployeeProvidersPartOfTheSameProvider(dto.EmployeeIds, loggedUserProviderId);

            await AuthorizeProviderAdminByActivityProviderAsync(id);

            var result = _activityRequestDtoValidator.Validate(dto);

            _validationExtension.ValidateValidationResult(result);

            ActivityResponseDto updatedActivity = await _activityRepository.UpdateAsync(id, dto);

            return updatedActivity;
        }

        public async Task DeleteAsync(int id)
        {
            await AuthorizeProviderAdminByActivityProviderAsync(id);

            await _activityRepository.DeleteAsync(id);
        }

        public async Task<ActivityResponseDto> GetByIdAsync(int id)
        {
            await AuthorizeProviderAdminByActivityProviderAsync(id);

            ActivityResponseDto activity = await _activityRepository.GetByIdAsync(id);

            return activity;
        }

        public async Task<List<ActivityResponseDto>> GetAllAsync(PagingInfo pagingInfo)
        {
            List<ActivityResponseDto> activities = await _activityRepository.GetAllAsync(pagingInfo);

            return activities;
        }

        public async Task AuthorizeProviderAdminByActivityProviderAsync(int activityId)
        {
            if (userRole == Roles.PROVIDER_ADMIN.ToString())
            {
                var loggedUserId = _httpContextAccessor.GetLoggedUserId();

                var activityProviderId = await _activityRepository.GetActivityProviderAsync(activityId);

                if (!await _employeeRepository.IsAccountPartOfProviderAsync(loggedUserId, activityProviderId))
                {
                    throw new UnauthorizedException("You can not perform this action on a provider you don't administrate!");
                }
            }
        }
        public void CheckEmployeeProvidersPartOfTheSameProvider(List<int> employeeIds, int loggedUserProviderId)
        {
            if (!_employeeRepository.CheckEmployeeProvidersPartOfTheSameProvider(employeeIds, loggedUserProviderId))
            {
                throw new ValidationException("One or more employee/s are not from the logged user's provider!");
            }
        }
    }
}
