using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.DTOs.Providers;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Pagination;
using RushHour.Domain.Exceptions;
using RushHour.Domain.Enums;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.FluentValidations.Providers;
using RushHour.Domain.Abstractions.Extensions;
using FluentValidation;

namespace RushHour.Domain.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _providerRepository;
        private IHttpContextAccessorWrapper _httpContextAccessor;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IValidationExtension _validationExtension;
        private readonly IValidator<ProviderRequestDto> _providerRequestDtoValidator;
        string? userRole;
        public ProviderService(IProviderRepository providerRepository, IHttpContextAccessorWrapper httpContextAccessor, IEmployeeRepository employeeRepository,
            IValidationExtension validationExtension, IValidator<ProviderRequestDto> providerRequestDtoValidator)
        {
            _providerRepository = providerRepository;
            _httpContextAccessor = httpContextAccessor;
            _employeeRepository = employeeRepository;
            _validationExtension = validationExtension;
            _providerRequestDtoValidator = providerRequestDtoValidator;

            userRole = _httpContextAccessor.GetUserRole();
        }

        public async Task<ProviderResponseDto> CreateAsync(ProviderRequestDto dto)
        {
            var result = _providerRequestDtoValidator.Validate(dto);

            _validationExtension.ValidateValidationResult(result);

            ProviderResponseDto createdProvider = await _providerRepository.CreateAsync(dto);

            return createdProvider;
        }

        public async Task<ProviderResponseDto> UpdateAsync(int id, ProviderRequestDto dto)
        {
            await AuthorizeProviderAdminAsync(id);

            var providerValidator = new ProviderRequestDtoValidator();
            var result = providerValidator.Validate(dto);

            _validationExtension.ValidateValidationResult(result);

            ProviderResponseDto updatedProvider = await _providerRepository.UpdateAsync(id, dto);

            return updatedProvider;
        }

        public async Task DeleteAsync(int id)
        {
            await _providerRepository.DeleteAsync(id);
        }

        public async Task<ProviderResponseDto> GetByIdAsync(int id)
        {
            await AuthorizeProviderAdminAsync(id);

            ProviderResponseDto provider = await _providerRepository.GetByIdAsync(id);

            return provider;
        }

        public async Task<List<ProviderResponseDto>> GetAllAsync(PagingInfo pagingInfo)
        {
            List<ProviderResponseDto> providers = await _providerRepository.GetAllAsync(pagingInfo);

            return providers;
        }

        public async Task AuthorizeProviderAdminAsync(int providerId)
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
    }
}
