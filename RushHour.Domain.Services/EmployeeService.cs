using FluentValidation;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Accounts;
using RushHour.Domain.DTOs.Employees;
using RushHour.Domain.Enums;
using RushHour.Domain.Exceptions;
using RushHour.Domain.Pagination;
using System.Text.RegularExpressions;
using ValidationException = RushHour.Domain.Exceptions.ValidationException;

namespace RushHour.Domain.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly IAuthService _authService; 
        private IHttpContextAccessorWrapper _httpContextAccessor;
        private readonly IValidationExtension _validationExtension;
        private readonly IValidator<EmployeeRequestDto> _employeeRequestDtoValidator;
        private readonly IValidator<EmployeeRequestDtoForUpdate> _employeeRequestDtoForUpdateValidator;
        private readonly IValidator<AccountRequestDto> _accountRequestDtoValidatorValidator;
        private readonly IValidator<AccountRequestDtoForEmployeeUpdate> _accountRequestDtoForEmployeeUpdateValidator;

        string? userRole;

        public EmployeeService(IEmployeeRepository employeeRepository, IProviderRepository providerRepository, IAuthService authService, IHttpContextAccessorWrapper httpContextAccessor,
            IValidationExtension validationExtension, IValidator<EmployeeRequestDto> employeeRequestDtoValidator, IValidator<EmployeeRequestDtoForUpdate> employeeRequestDtoForUpdateValidator,
            IValidator<AccountRequestDto> accountRequestDtoValidatorValidator, IValidator<AccountRequestDtoForEmployeeUpdate> accountRequestDtoForEmployeeUpdateValidator)
        {
            _employeeRepository = employeeRepository;
            _providerRepository = providerRepository;
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
            _validationExtension = validationExtension;
            _employeeRequestDtoValidator = employeeRequestDtoValidator;
            _employeeRequestDtoForUpdateValidator = employeeRequestDtoForUpdateValidator;
            _accountRequestDtoValidatorValidator = accountRequestDtoValidatorValidator;
            _accountRequestDtoForEmployeeUpdateValidator = accountRequestDtoForEmployeeUpdateValidator;

            userRole = _httpContextAccessor.GetUserRole();
        }

        public async Task<EmployeeResponseDto> CreateAsync(EmployeeRequestDto dto)
        {
            await AuthorizeProviderAdminByProviderAsync(dto.ProviderId);

            var provider = await _providerRepository.GetByIdAsync(dto.ProviderId);

            var resultEmployee = _employeeRequestDtoValidator.Validate(dto);
            var result = _accountRequestDtoValidatorValidator.Validate(dto);
            result.Errors.AddRange(resultEmployee.Errors);

            _validationExtension.ValidateValidationResult(result);

            dto.Password = _authService.HashPasword(dto.Password, out var salt);

            Regex regex = new Regex($"^[\\w-\\.]+@({provider.BusinessDomain}\\.)+[\\w-]{{2,4}}$");
            bool isMatch = regex.IsMatch(dto.Email);

            if (!isMatch)
            {
                throw new ValidationException("The email domain should match the provider business domain!");
            }

            if ((dto.Role == Roles.CLIENT) || (dto.Role == Roles.ADMIN))
            {
                throw new ValidationException("Employee can not be created with a client or an admin role!");
            }

            EmployeeResponseDto createdEmployee = await _employeeRepository.CreateAsync(dto, salt);

            return createdEmployee;
        }

        public async Task<EmployeeResponseDto> UpdateAsync(int id, EmployeeRequestDtoForUpdate dto)
        {
            await AuthorizeProviderAdminByProviderAsync(id);

            await AuthorizeEmployeeAsync(id);

            var provider = await _providerRepository.GetByIdAsync(dto.ProviderId);

            var resultEmployee = _employeeRequestDtoForUpdateValidator.Validate(dto);
            var result = _accountRequestDtoForEmployeeUpdateValidator.Validate(dto);
            result.Errors.AddRange(resultEmployee.Errors);

            _validationExtension.ValidateValidationResult(result);

            Regex regex = new Regex($"^[\\w-\\.]+@({provider.BusinessDomain}\\.)+[\\w-]{{2,4}}$");
            bool isMatch = regex.IsMatch(dto.Email);

            if (!isMatch)
            {
                throw new ValidationException("The email domain should match the provider business domain!");
            }

            if ((dto.Role == Roles.CLIENT) || (dto.Role == Roles.ADMIN))
            {
                throw new ValidationException("Employee can not be updated to a client or an admin role!");
            }

            EmployeeResponseDto updatedEmployee = await _employeeRepository.UpdateAsync(id, dto);

            return updatedEmployee;
        }

        public async Task DeleteAsync(int id)
        {
            await AuthorizeProviderAdminByEmployeeAsync(id);

            await _employeeRepository.DeleteAsync(id);
        }

        public async Task<EmployeeResponseDto> GetByIdAsync(int id)
        {
            await AuthorizeProviderAdminByEmployeeAsync(id);

            await AuthorizeEmployeeAsync(id);

            EmployeeResponseDto employee = await _employeeRepository.GetByIdAsync(id);

            return employee;
        }

        public async Task<List<EmployeeResponseDto>> GetAllAsync(PagingInfo pagingInfo)
        {
            List<EmployeeResponseDto> employees = await _employeeRepository.GetAllAsync(pagingInfo);

            return employees;
        }

        public async Task AuthorizeProviderAdminByProviderAsync(int providerId)
        {
            if (userRole == Roles.PROVIDER_ADMIN.ToString())
            {
                var loggedUserId = _httpContextAccessor.GetLoggedUserId();

                if (!await _employeeRepository.IsAccountPartOfProviderAsync(loggedUserId, providerId))
                {
                    throw new UnauthorizedException("You can not perform this action on a provider you don't administrate!");
                }
            }
        }

        public async Task AuthorizeEmployeeAsync(int employeeId)
        {
            if (userRole == Roles.EMPLOYEE.ToString())
            {
                var loggedUserId = _httpContextAccessor.GetLoggedUserId();

                if (!await _employeeRepository.DoesEmployeeMatchAccountAsync(employeeId, loggedUserId))
                {
                    throw new UnauthorizedException("You can perform this action only on your account!");
                }
            }
        }
        public async Task AuthorizeProviderAdminByEmployeeAsync(int employeeId)
        {
            if (userRole == Roles.PROVIDER_ADMIN.ToString())
            {
                var loggedUserId = _httpContextAccessor.GetLoggedUserId();

                if (!await _employeeRepository.AreEmployeeAndAccountPartOfTheSameProviderAsync(loggedUserId, employeeId))
                {
                    throw new UnauthorizedException("You can perform this action only on your account!");
                }
            }
        }
    }
}
