using FluentValidation;
using Moq;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Providers;
using RushHour.Domain.Enums;
using RushHour.Domain.Exceptions;
using RushHour.Domain.FluentValidations.Providers;
using RushHour.Domain.Pagination;
using RushHour.Domain.Services;

namespace RushHour.Services.Tests
{
    public class ProviderServiceTest
    {
        private readonly Mock<IProviderRepository> repositoryMock;
        private readonly Mock<IEmployeeRepository> employeeRepository;
        private readonly Mock<IHttpContextAccessorWrapper> httpContextAccessor;
        private readonly Mock<IValidationExtension> validationExtension;
        private readonly IValidator<ProviderRequestDto> providerRequestDtoValidator;

        public ProviderServiceTest()
        {
            repositoryMock = new Mock<IProviderRepository>();
            employeeRepository = new Mock<IEmployeeRepository>();
            httpContextAccessor = new Mock<IHttpContextAccessorWrapper>();
            validationExtension = new Mock<IValidationExtension>();
            providerRequestDtoValidator = new ProviderRequestDtoValidator();
        }

        private void CompareProperties(ProviderResponseDto responseDto, ProviderResponseDto actual)
        {
            Assert.Equal(responseDto.Name, actual.Name);
            Assert.Equal(responseDto.Website, actual.Website);
            Assert.Equal(responseDto.BusinessDomain, actual.BusinessDomain);
            Assert.Equal(responseDto.Phone, actual.Phone);
            Assert.Equal(responseDto.StartTimeOfTheWorkingDay, actual.StartTimeOfTheWorkingDay);
            Assert.Equal(responseDto.EndTimeOfTheWorkingDay, actual.EndTimeOfTheWorkingDay);
            Assert.Equal(responseDto.WorkingDays, actual.WorkingDays);
        }

        [Fact]
        public async Task CreateAsync_SuccessfullyCreateProvider_ReturnsTheNewProvider()
        {
            ProviderRequestDto requestDto = new ProviderRequestDto()
            {
                Name = "Raya", Website = "www.prime.com", BusinessDomain = "prime", Phone = "0888888888",
                StartTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 8, 0, 0, 0), EndTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 18, 0, 0, 0), WorkingDays = DaysOfWeek.Monday
            };

            ProviderResponseDto providerResponseDto = new ProviderResponseDto();

            var service = new ProviderService(repositoryMock.Object, httpContextAccessor.Object, employeeRepository.Object, validationExtension.Object, providerRequestDtoValidator);

            repositoryMock.Setup(s => s.CreateAsync(requestDto))
                .ReturnsAsync(providerResponseDto);

            var actual = await service.CreateAsync(requestDto);

            CompareProperties(providerResponseDto, actual);
        }

        [Fact]
        public async Task UpdateAsync_SuccessfullyUpdateProvider_ReturnsTheUpdatedProvider()
        {
            ProviderRequestDto requestDto = new ProviderRequestDto()
            {
                Name = "Raya",
                Website = "www.prime.com",
                BusinessDomain = "prime",
                Phone = "0888888888",
                StartTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 8, 0, 0, 0),
                EndTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 18, 0, 0, 0),
                WorkingDays = DaysOfWeek.Monday
            };

            int providerId = 15;

            ProviderResponseDto providerResponseDto = new ProviderResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            var service = new ProviderService(repositoryMock.Object, httpContextAccessor.Object, employeeRepository.Object, validationExtension.Object, providerRequestDtoValidator);

            repositoryMock.Setup(s => s.UpdateAsync(providerId, requestDto))
                .ReturnsAsync(providerResponseDto);

            employeeRepository.Setup(s => s.IsAccountPartOfProviderAsync(10, 15))
                .ReturnsAsync(true);

            var actual = await service.UpdateAsync(providerId, requestDto);

            CompareProperties(providerResponseDto, actual);
        }

        [Fact]
        public async Task DeleteAsync_SuccessfullyDeleteProvider_VerifyThatDeleteWasInvockedOnce()
        {
            int providerId = 15;

            var service = new ProviderService(repositoryMock.Object, httpContextAccessor.Object, employeeRepository.Object, validationExtension.Object, providerRequestDtoValidator);

            repositoryMock.Setup(s => s.DeleteAsync(providerId))
             .Returns(Task.CompletedTask);

            await service.DeleteAsync(providerId);

            repositoryMock.Verify(x => x.DeleteAsync(providerId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_SuccessfullyGetProviderById_ReturnASpecificProviderById()
        {
            int providerId = 15;

            ProviderResponseDto providerResponseDto = new ProviderResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
           .Returns(10);

            var service = new ProviderService(repositoryMock.Object, httpContextAccessor.Object, employeeRepository.Object, validationExtension.Object, providerRequestDtoValidator);

            repositoryMock.Setup(s => s.GetByIdAsync(providerId))
              .ReturnsAsync(providerResponseDto);

            employeeRepository.Setup(s => s.IsAccountPartOfProviderAsync(10, 15))
               .ReturnsAsync(true);

            var actual = await service.GetByIdAsync(providerId);

            CompareProperties(providerResponseDto, actual);
        }

        [Fact]
        public async Task GetAllAsync_SuccessfullyGetAllProviders_ReturnNumberOfProvidersFromChoosedPage()
        {
            PagingInfo pagingInfo = new PagingInfo();

            List<ProviderResponseDto> providerResponseDtos = new List<ProviderResponseDto>();

            var service = new ProviderService(repositoryMock.Object, httpContextAccessor.Object, employeeRepository.Object, validationExtension.Object, providerRequestDtoValidator);

            repositoryMock.Setup(s => s.GetAllAsync(pagingInfo))
                .ReturnsAsync(providerResponseDtos);

            var actual = await service.GetAllAsync(pagingInfo);

            for (int i = 0; i < providerResponseDtos.Count; i++)
            {
                Assert.Equal(providerResponseDtos[i].Name, actual[i].Name);
                Assert.Equal(providerResponseDtos[i].Phone, actual[i].Phone);
                Assert.Equal(providerResponseDtos[i].Website, actual[i].Website);
                Assert.Equal(providerResponseDtos[i].BusinessDomain, actual[i].BusinessDomain);
                Assert.Equal(providerResponseDtos[i].WorkingDays, actual[i].WorkingDays);
                Assert.Equal(providerResponseDtos[i].StartTimeOfTheWorkingDay, actual[i].StartTimeOfTheWorkingDay);
                Assert.Equal(providerResponseDtos[i].EndTimeOfTheWorkingDay, actual[i].EndTimeOfTheWorkingDay);
            }
        }

        [Fact]
        public async Task UpdateAsync_EmployeeRelatedToDiffrentProvider_ThrowsUnauthorizedException()
        {
            ProviderRequestDto requestDto = new ProviderRequestDto()
            {
                Name = "Raya",
                Website = "www.prime.com",
                BusinessDomain = "prime",
                Phone = "0888888888",
                StartTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 8, 0, 0, 0),
                EndTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 18, 0, 0, 0),
                WorkingDays = DaysOfWeek.Monday
            };

            int providerId = 15;

            ProviderResponseDto providerResponseDto = new ProviderResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);
            
            httpContextAccessor.Setup(s => s.GetUserRole())
              .Returns("PROVIDER_ADMIN");

            var service = new ProviderService(repositoryMock.Object, httpContextAccessor.Object, employeeRepository.Object, validationExtension.Object, providerRequestDtoValidator);

            repositoryMock.Setup(s => s.UpdateAsync(providerId, requestDto))
                .ReturnsAsync(providerResponseDto);

            employeeRepository.Setup(s => s.IsAccountPartOfProviderAsync(10, 15))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.UpdateAsync(providerId, requestDto));
        }

        [Fact]
        public async Task GetByIdAsync_EmployeeRelatedToDiffrentProvider_ThrowsUnauthorizedException()
        {
            int providerId = 15;

            ProviderResponseDto providerResponseDto = new ProviderResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
           .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
            .Returns("PROVIDER_ADMIN");

            var service = new ProviderService(repositoryMock.Object, httpContextAccessor.Object, employeeRepository.Object, validationExtension.Object, providerRequestDtoValidator);

            repositoryMock.Setup(s => s.GetByIdAsync(providerId))
              .ReturnsAsync(providerResponseDto);

            employeeRepository.Setup(s => s.IsAccountPartOfProviderAsync(10, 15))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetByIdAsync(providerId));
        }
    }
}
