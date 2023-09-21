using FluentValidation;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Accounts;
using RushHour.Domain.DTOs.Clients;
using RushHour.Domain.Enums;
using RushHour.Domain.Exceptions;
using RushHour.Domain.Pagination;

namespace RushHour.Domain.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IAuthService _authService;
        private IHttpContextAccessorWrapper _httpContextAccessor;
        private readonly IValidationExtension _validationExtension;
        private readonly IValidator<ClientRequestDto> _clientRequestDtoValidator;
        private readonly IValidator<ClientRequestDtoForUpdate> _clientRequestDtoForUpdateValidator;
        private readonly IValidator<AccountRequestDtoForClient> _accountRequestDtoForClientValidator;
        private readonly IValidator<AccountRequestDtoForClientUpdate> _accountRequestDtoForClientUpdateValidator;
        string? userRole;

        public ClientService(IClientRepository clientRepository, IAuthService authService, IHttpContextAccessorWrapper httpContextAccessor,
            IValidationExtension validationExtension, IValidator<ClientRequestDto> clientRequestDtoValidator, IValidator<ClientRequestDtoForUpdate> clientRequestDtoForUpdateValidator,
            IValidator<AccountRequestDtoForClient> accountRequestDtoForClientValidator, IValidator<AccountRequestDtoForClientUpdate> accountRequestDtoForClientUpdateValidator)
        {
            _clientRepository = clientRepository;
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
            _validationExtension = validationExtension;
            _clientRequestDtoValidator = clientRequestDtoValidator;
            _clientRequestDtoForUpdateValidator = clientRequestDtoForUpdateValidator;
            _accountRequestDtoForClientValidator = accountRequestDtoForClientValidator;
            _accountRequestDtoForClientUpdateValidator = accountRequestDtoForClientUpdateValidator;

            userRole = _httpContextAccessor.GetUserRole();
        }

        public async Task<ClientResponseDto> CreateAsync(ClientRequestDto dto)
        {
            var resultClient = _clientRequestDtoValidator.Validate(dto);
            var result = _accountRequestDtoForClientValidator.Validate(dto);
            result.Errors.AddRange(resultClient.Errors);

            _validationExtension.ValidateValidationResult(result);

            dto.Password = _authService.HashPasword(dto.Password, out var salt);

            ClientResponseDto createdClient = await _clientRepository.CreateAsync(dto, salt);

            return createdClient;
        }

        public async Task<ClientResponseDto> UpdateAsync(int id, ClientRequestDtoForUpdate dto)
        {
            await AuthorizeClient(id);

            var resultClient = _clientRequestDtoForUpdateValidator.Validate(dto);
            var result = _accountRequestDtoForClientUpdateValidator.Validate(dto);
            result.Errors.AddRange(resultClient.Errors);

            _validationExtension.ValidateValidationResult(result);

            ClientResponseDto updatedClient = await _clientRepository.UpdateAsync(id, dto);

            return updatedClient;
        }

        public async Task DeleteAsync(int id)
        {
            await AuthorizeClient(id);

            await _clientRepository.DeleteAsync(id);
        }

        public async Task<ClientResponseDto> GetByIdAsync(int id)
        {
            await AuthorizeClient(id);

            ClientResponseDto client = await _clientRepository.GetByIdAsync(id);

            return client;
        }

        public async Task<List<ClientResponseDto>> GetAllAsync(PagingInfo pagingInfo)
        {
            List<ClientResponseDto> clients = await _clientRepository.GetAllAsync(pagingInfo);

            return clients;
        }

        public async Task AuthorizeClient(int clientId)
        {
            if (userRole == Roles.CLIENT.ToString())
            {
                var loggedUserId = _httpContextAccessor.GetLoggedUserId();

                if (!await _clientRepository.DoesClientMatchAccountAsync(clientId, loggedUserId))
                {
                    throw new UnauthorizedException("You can perform this action only on your information!");
                }
            }
        }
    }
}
