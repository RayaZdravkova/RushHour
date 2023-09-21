using FluentValidation;
using Moq;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Accounts;
using RushHour.Domain.DTOs.Clients;
using RushHour.Domain.Exceptions;
using RushHour.Domain.FluentValidations.Accounts;
using RushHour.Domain.FluentValidations.Clients;
using RushHour.Domain.Pagination;
using RushHour.Domain.Services;

namespace RushHour.Services.Tests
{
    public class ClientServiceTest
    {
        private readonly Mock<IClientRepository> repositoryMock;
        private readonly Mock<IAuthService> authService;
        private readonly Mock<IHttpContextAccessorWrapper> httpContextAccessor;
        private readonly Mock<IValidationExtension> validationExtension;
        private readonly IValidator<ClientRequestDto> clientRequestDtoValidator;
        private readonly IValidator<ClientRequestDtoForUpdate> clientRequestDtoForUpdateValidator;
        private readonly IValidator<AccountRequestDtoForClient> accountRequestDtoForClientValidator;
        private readonly IValidator<AccountRequestDtoForClientUpdate> accountRequestDtoForClientUpdateValidator;

        public ClientServiceTest()
        {
            repositoryMock = new Mock<IClientRepository>();
            authService = new Mock<IAuthService>();
            httpContextAccessor = new Mock<IHttpContextAccessorWrapper>();
            validationExtension = new Mock<IValidationExtension>();
            clientRequestDtoValidator = new ClientRequestDtoValidator();
            clientRequestDtoForUpdateValidator = new ClientRequestDtoForUpdateValidator();
            accountRequestDtoForClientValidator = new AccountRequestDtoForClientValidator();
            accountRequestDtoForClientUpdateValidator = new AccountRequestDtoForClientUpdateValidator();
        }

        private void CompareProperties(ClientResponseDto responseDto, ClientResponseDto actual)
        {
            Assert.Equal(responseDto.Phone, actual.Phone);
            Assert.Equal(responseDto.Address, actual.Address);
            Assert.Equal(responseDto.Email, actual.Email);
            Assert.Equal(responseDto.FullName, actual.FullName);
            Assert.Equal(responseDto.Username, actual.Username);
        }

        [Fact]
        public async Task CreateAsync_SuccessfullyCreateClient_ReturnsTheNewClient()
        {
            ClientRequestDto requestDto = new ClientRequestDto() 
            {
                Phone = "088888888",
                Address = "Sofia",
                Email = "raya@prime.com",
                FullName = "Raya",
                Password = "Raya123!",
                Username = "raya"
            };

            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };

            ClientResponseDto clientResponseDto = new ClientResponseDto();

            var service = new ClientService(repositoryMock.Object, authService.Object, httpContextAccessor.Object, validationExtension.Object, clientRequestDtoValidator,
               clientRequestDtoForUpdateValidator, accountRequestDtoForClientValidator, accountRequestDtoForClientUpdateValidator);

            authService.Setup(s => s.HashPasword(requestDto.Password, out salt))
                .Returns(requestDto.Password);

            repositoryMock.Setup(s => s.CreateAsync(requestDto, salt))
                .ReturnsAsync(clientResponseDto);

            var actual = await service.CreateAsync(requestDto);

            CompareProperties(clientResponseDto, actual);
        }

        [Fact]
        public async Task UpdateAsync_SuccessfullyUpdateClient_ReturnsTheUpdatedClient()
        {
            int clientId = 20;

            ClientRequestDtoForUpdate requestDto = new ClientRequestDtoForUpdate()
            {
                Phone = "088888888",
                Address = "Sofia",
                Email = "raya@prime.com",
                FullName = "Raya",
                Username = "raya"
            };

            ClientResponseDto clientResponseDto = new ClientResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
             .Returns(10);

            var service = new ClientService(repositoryMock.Object, authService.Object, httpContextAccessor.Object, validationExtension.Object, clientRequestDtoValidator,
               clientRequestDtoForUpdateValidator, accountRequestDtoForClientValidator, accountRequestDtoForClientUpdateValidator);

            repositoryMock.Setup(s => s.UpdateAsync(clientId, requestDto))
                .ReturnsAsync(clientResponseDto);

            repositoryMock.Setup(s => s.DoesClientMatchAccountAsync(clientId, 10))
                .ReturnsAsync(true);

            var actual = await service.UpdateAsync(clientId, requestDto);

            CompareProperties(clientResponseDto, actual);
        }

        [Fact]
        public async Task DeleteAsync_SuccessfullyDeleteClient_VerifyThatDeleteWasInvockedOnce()
        {
            int clientId = 20;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(10);

            var service = new ClientService(repositoryMock.Object, authService.Object, httpContextAccessor.Object, validationExtension.Object, clientRequestDtoValidator,
               clientRequestDtoForUpdateValidator, accountRequestDtoForClientValidator, accountRequestDtoForClientUpdateValidator);

            repositoryMock.Setup(s => s.DeleteAsync(clientId))
             .Returns(Task.CompletedTask);

            repositoryMock.Setup(s => s.DoesClientMatchAccountAsync(clientId, 10))
               .ReturnsAsync(true);

            await service.DeleteAsync(clientId);

            repositoryMock.Verify(x => x.DeleteAsync(clientId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_SuccessfullyGetClientById_ReturnASpecificClientById()
        {
            int clientId = 20;

            ClientResponseDto? clientResponseDto = new ClientResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
           .Returns(10);

            var service = new ClientService(repositoryMock.Object, authService.Object, httpContextAccessor.Object, validationExtension.Object, clientRequestDtoValidator,
               clientRequestDtoForUpdateValidator, accountRequestDtoForClientValidator, accountRequestDtoForClientUpdateValidator);

            repositoryMock.Setup(s => s.GetByIdAsync(clientId))
              .ReturnsAsync(clientResponseDto);

            repositoryMock.Setup(s => s.DoesClientMatchAccountAsync(clientId, 10))
               .ReturnsAsync(true);

            var actual = await service.GetByIdAsync(clientId);

            CompareProperties(clientResponseDto, actual);
        }

        [Fact]
        public async Task GetAllAsync_SuccessfullyGetAllClients_ReturnNumberOfClientsFromChoosedPage()
        {
            PagingInfo pagingInfo = new PagingInfo();

            List<ClientResponseDto> clientResponseDtos = new List<ClientResponseDto>();

            var service = new ClientService(repositoryMock.Object, authService.Object, httpContextAccessor.Object, validationExtension.Object, clientRequestDtoValidator,
               clientRequestDtoForUpdateValidator, accountRequestDtoForClientValidator, accountRequestDtoForClientUpdateValidator);

            repositoryMock.Setup(s => s.GetAllAsync(pagingInfo))
                .ReturnsAsync(clientResponseDtos);

            var actual = await service.GetAllAsync(pagingInfo);

            for (int i = 0; i < clientResponseDtos.Count; i++)
            {
                Assert.Equal(clientResponseDtos[i].Phone, actual[i].Phone);
                Assert.Equal(clientResponseDtos[i].Address, actual[i].Address);
                Assert.Equal(clientResponseDtos[i].Email, actual[i].Email);
                Assert.Equal(clientResponseDtos[i].FullName, actual[i].FullName);
                Assert.Equal(clientResponseDtos[i].Username, actual[i].Username);
            }
        }

        [Fact]
        public async Task UpdateAsync_ClientNotRelatedToTheClient_ThrowsUnauthorizedException()
        {
            int clientId = 20;

            ClientRequestDtoForUpdate requestDto = new ClientRequestDtoForUpdate()
            {
                Phone = "088888888",
                Address = "Sofia",
                Email = "raya@prime.com",
                FullName = "Raya",
                Username = "raya"
            };

            ClientResponseDto clientResponseDto = new ClientResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
             .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
             .Returns("CLIENT");

            var service = new ClientService(repositoryMock.Object, authService.Object, httpContextAccessor.Object, validationExtension.Object, clientRequestDtoValidator,
               clientRequestDtoForUpdateValidator, accountRequestDtoForClientValidator, accountRequestDtoForClientUpdateValidator);

            repositoryMock.Setup(s => s.UpdateAsync(clientId, requestDto))
                .ReturnsAsync(clientResponseDto);

            repositoryMock.Setup(s => s.DoesClientMatchAccountAsync(clientId, 10))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.UpdateAsync(clientId, requestDto));
        }

        [Fact]
        public async Task DeleteAsync_ClientNotRelatedToTheClient_ThrowsUnauthorizedException()
        {
            int clientId = 20;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
            .Returns("CLIENT");

            var service = new ClientService(repositoryMock.Object, authService.Object, httpContextAccessor.Object, validationExtension.Object, clientRequestDtoValidator,
               clientRequestDtoForUpdateValidator, accountRequestDtoForClientValidator, accountRequestDtoForClientUpdateValidator);

            repositoryMock.Setup(s => s.DeleteAsync(clientId))
             .Returns(Task.CompletedTask);

            repositoryMock.Setup(s => s.DoesClientMatchAccountAsync(clientId, 10))
               .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.DeleteAsync(clientId));
        }

        [Fact]
        public async Task GetByIdAsync_ClientNotRelatedToTheClient_ThrowsUnauthorizedException()
        {
            int clientId = 20;

            ClientResponseDto? clientResponseDto = new ClientResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
           .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
            .Returns("CLIENT");

            var service = new ClientService(repositoryMock.Object, authService.Object, httpContextAccessor.Object, validationExtension.Object, clientRequestDtoValidator,
                clientRequestDtoForUpdateValidator, accountRequestDtoForClientValidator, accountRequestDtoForClientUpdateValidator);

            repositoryMock.Setup(s => s.GetByIdAsync(clientId))
              .ReturnsAsync(clientResponseDto);

            repositoryMock.Setup(s => s.DoesClientMatchAccountAsync(clientId, 10))
               .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetByIdAsync(clientId));
        }

    }
}
