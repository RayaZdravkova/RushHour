using FluentValidation;
using Moq;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Services;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Accounts;
using RushHour.Domain.DTOs.Employees;
using RushHour.Domain.DTOs.Providers;
using RushHour.Domain.Enums;
using RushHour.Domain.Exceptions;
using RushHour.Domain.FluentValidations.Accounts;
using RushHour.Domain.FluentValidations.Employees;
using RushHour.Domain.Pagination;
using RushHour.Domain.Services;
using System.Text.RegularExpressions;
using ValidationException = RushHour.Domain.Exceptions.ValidationException;

namespace RushHour.Services.Tests
{
    public class EmployeeServiceTest
    {
        private readonly Mock<IEmployeeRepository> repositoryMock;
        private readonly Mock<IProviderRepository> providerRepository;
        private readonly Mock<IAuthService> authService;
        private readonly Mock<IHttpContextAccessorWrapper> httpContextAccessor;
        private readonly Mock<IValidationExtension> validationExtension;
        private readonly IValidator<EmployeeRequestDto> employeeRequestDtoValidator;
        private readonly IValidator<EmployeeRequestDtoForUpdate> employeeRequestDtoForUpdateValidator;
        private readonly IValidator<AccountRequestDto> accountRequestDtoValidatorValidator;
        private readonly IValidator<AccountRequestDtoForEmployeeUpdate> accountRequestDtoForEmployeeUpdateValidator;

        public EmployeeServiceTest()
        {
            repositoryMock = new Mock<IEmployeeRepository>();
            providerRepository = new Mock<IProviderRepository>();
            authService = new Mock<IAuthService>();
            httpContextAccessor = new Mock<IHttpContextAccessorWrapper>();
            validationExtension = new Mock<IValidationExtension>();
            employeeRequestDtoValidator = new EmployeeRequestDtoValidator();
            employeeRequestDtoForUpdateValidator = new EmployeeRequestDtoForUpdateValidator();
            accountRequestDtoValidatorValidator = new AccountRequestDtoValidator();
            accountRequestDtoForEmployeeUpdateValidator = new AccountRequestDtoForEmployeeUpdateValidator();
        }

        private void CompareProperties(EmployeeResponseDto responseDto, EmployeeResponseDto actual)
        {
            Assert.Equal(responseDto.Title, actual.Title);
            Assert.Equal(responseDto.Phone, actual.Phone);
            Assert.Equal(responseDto.RatePerHour, actual.RatePerHour);
            Assert.Equal(responseDto.HireDate, actual.HireDate);
            Assert.Equal(responseDto.ProviderId, actual.ProviderId);
            Assert.Equal(responseDto.Email, actual.Email);
            Assert.Equal(responseDto.FullName, actual.FullName);
            Assert.Equal(responseDto.Username, actual.Username);
            Assert.Equal(responseDto.Role, actual.Role);
        }

        [Fact]
        public async Task CreateAsync_SuccessfullyCreateEmployee_ReturnsTheNewEmployee()
        {
            EmployeeRequestDto requestDto = new EmployeeRequestDto()   
            {
                Title = "Raya",
                Phone = "088888888",
                RatePerHour = 10,
                HireDate = new DateTime(2023, 06, 10, 8, 30, 0, 0),
                ProviderId = 12,
                Email = "raya@prime.com",
                FullName = "Raya",
                Password = "Raya123!",
                Username = "raya",
                Role = Roles.EMPLOYEE
            };

            ProviderResponseDto providerResponseDto = new ProviderResponseDto()
            {
                Id = 12,
                Name = "Raya",
                Website = "www.prime.com",
                BusinessDomain = "prime",
                Phone = "0888888888",
                StartTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 8, 0, 0, 0),
                EndTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 18, 0, 0, 0),
                WorkingDays = DaysOfWeek.Monday
            };

            EmployeeResponseDto employeeResponseDto = new EmployeeResponseDto();

            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };
            int providerId = 12;
            Regex regex = new Regex($"^[\\w-\\.]+@({providerResponseDto.BusinessDomain}\\.)+[\\w-]{{2,4}}$");
          
            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
           .Returns("EMPLOYEE");

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.IsAccountPartOfProviderAsync(10, providerId))
            .ReturnsAsync(true);

            providerRepository.Setup(s => s.GetByIdAsync(providerId))
            .ReturnsAsync(providerResponseDto);

            authService.Setup(s => s.HashPasword(requestDto.Password, out salt))
              .Returns(requestDto.Password);

            regex.IsMatch(requestDto.Email);

            repositoryMock.Setup(s => s.CreateAsync(requestDto, salt))
                .ReturnsAsync(employeeResponseDto);

            var actual = await service.CreateAsync(requestDto);

            CompareProperties(employeeResponseDto, actual);
        }

        [Fact]
        public async Task UpdateAsync_SuccessfullyUpdatedEmployee_ReturnsTheUpdatedEmployee()
        {
            int employeeId = 16;

            EmployeeRequestDtoForUpdate requestDto = new EmployeeRequestDtoForUpdate()   
            {
                Title = "Raya",
                Phone = "088888888",
                RatePerHour = 10,
                HireDate = new DateTime(2023, 06, 10, 8, 30, 0, 0),
                ProviderId = 12,
                Email = "raya@prime.com",
                FullName = "Raya",
                Username = "raya",
                Role = Roles.EMPLOYEE
        };

            ProviderResponseDto providerResponseDto = new ProviderResponseDto()
            {
                Id = 12,
                Name = "Raya",
                Website = "www.prime.com",
                BusinessDomain = "prime",
                Phone = "0888888888",
                StartTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 8, 0, 0, 0),
                EndTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 18, 0, 0, 0),
                WorkingDays = DaysOfWeek.Monday
            };

            EmployeeResponseDto employeeResponseDto = new EmployeeResponseDto();

            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };
            int providerId = 12;
            Regex regex = new Regex($"^[\\w-\\.]+@({providerResponseDto.BusinessDomain}\\.)+[\\w-]{{2,4}}$");
          
            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
           .Returns("EMPLOYEE");

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.IsAccountPartOfProviderAsync(10, providerId))
            .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesEmployeeMatchAccountAsync(employeeId, 10))
            .ReturnsAsync(true);

            providerRepository.Setup(s => s.GetByIdAsync(providerId))
            .ReturnsAsync(providerResponseDto);

            regex.IsMatch(requestDto.Email);

            repositoryMock.Setup(s => s.UpdateAsync(employeeId, requestDto))
                .ReturnsAsync(employeeResponseDto);

            var actual = await service.UpdateAsync(employeeId, requestDto);

            CompareProperties(employeeResponseDto, actual);
        }

        [Fact]
        public async Task DeleteAsync_SuccessfullyDeleteEmployee_VerifyThatDeleteWasInvockedOnce()
        {
            int employeeId = 10;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(10);

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.DeleteAsync(employeeId))
             .Returns(Task.CompletedTask);

            repositoryMock.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, employeeId))
              .ReturnsAsync(true);

            await service.DeleteAsync(employeeId);

            repositoryMock.Verify(x => x.DeleteAsync(employeeId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_SuccessfullyGetEmployeeById_ReturnASpecificEmployeeById()
        {
            int employeeId = 10;

            EmployeeResponseDto? employeeResponseDto = new EmployeeResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
           .Returns(10);

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.GetByIdAsync(employeeId))
              .ReturnsAsync(employeeResponseDto);

            repositoryMock.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, employeeId))
              .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesEmployeeMatchAccountAsync(employeeId, 10))
               .ReturnsAsync(true);

            var actual = await service.GetByIdAsync(employeeId);

            CompareProperties(employeeResponseDto, actual);
        }

        [Fact]
        public async Task GetAllAsync_SuccessfullyGetAllEmployees_ReturnNumberOfEmployeesFromChoosedPage()
        {
            PagingInfo pagingInfo = new PagingInfo();

            List<EmployeeResponseDto> employeeResponseDtos = new List<EmployeeResponseDto>();

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.GetAllAsync(pagingInfo))
                .ReturnsAsync(employeeResponseDtos);

            var actual = await service.GetAllAsync(pagingInfo);
     
            for (int i = 0; i < employeeResponseDtos.Count; i++)
            {
                Assert.Equal(employeeResponseDtos[i].Title, actual[i].Title);
                Assert.Equal(employeeResponseDtos[i].Phone, actual[i].Phone);
                Assert.Equal(employeeResponseDtos[i].RatePerHour, actual[i].RatePerHour);
                Assert.Equal(employeeResponseDtos[i].HireDate, actual[i].HireDate);
                Assert.Equal(employeeResponseDtos[i].ProviderId, actual[i].ProviderId);
                Assert.Equal(employeeResponseDtos[i].Email, actual[i].Email);
                Assert.Equal(employeeResponseDtos[i].FullName, actual[i].FullName);
                Assert.Equal(employeeResponseDtos[i].Username, actual[i].Username);
                Assert.Equal(employeeResponseDtos[i].Role, actual[i].Role);
            }
        }

        [Fact]
        public async Task CreateAsync_NewEmployeeNotRelatedToLoggedEmployeeProvider_ThrowsUnauthorizedException()
        {
            EmployeeRequestDto requestDto = new EmployeeRequestDto()
            {
                Title = "Raya",
                Phone = "088888888",
                RatePerHour = 10,
                HireDate = new DateTime(2023, 06, 10, 8, 30, 0, 0),
                ProviderId = 12,
                Email = "raya@prime.com",
                FullName = "Raya",
                Password = "Raya123!",
                Username = "raya"
            };

            int providerId = 12;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
           .Returns("PROVIDER_ADMIN");

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.IsAccountPartOfProviderAsync(10, providerId))
            .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.CreateAsync(requestDto));
        }

        [Fact]
        public async Task CreateAsync_BussinessDomainDoesNotMatchEmailDomain_ThrowsValidationException()
        {
            EmployeeRequestDto requestDto = new EmployeeRequestDto()
            {
                Title = "Raya",
                Phone = "088888888",
                RatePerHour = 10,
                HireDate = new DateTime(2023, 06, 10, 8, 30, 0, 0),
                ProviderId = 12,
                Email = "raya@pdental.com",
                FullName = "Raya",
                Password = "Raya123!",
                Username = "raya"
            };

            ProviderResponseDto providerResponseDto = new ProviderResponseDto()
            {
                Id = 12,
                Name = "Raya",
                Website = "www.prime.com",
                BusinessDomain = "prime",
                Phone = "0888888888",
                StartTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 8, 0, 0, 0),
                EndTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 18, 0, 0, 0),
                WorkingDays = DaysOfWeek.Monday
            };

            EmployeeResponseDto employeeResponseDto = new EmployeeResponseDto();

            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };
            int providerId = 12;
            Regex regex = new Regex($"^[\\w-\\.]+@({providerResponseDto.BusinessDomain}\\.)+[\\w-]{{2,4}}$");

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
           .Returns("PROVIDER_ADMIN");

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.IsAccountPartOfProviderAsync(10, providerId))
            .ReturnsAsync(true);

            providerRepository.Setup(s => s.GetByIdAsync(providerId))
            .ReturnsAsync(providerResponseDto);

            authService.Setup(s => s.HashPasword(requestDto.Password, out salt))
              .Returns(requestDto.Password);

            regex.IsMatch(requestDto.Email);

            await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(requestDto));
        }

        [Fact]
        public async Task CreateAsync_EmployeeWithAdminRole_ThrowsValidationException()
        {
            EmployeeRequestDto requestDto = new EmployeeRequestDto()
            {
                Title = "Raya",
                Phone = "088888888",
                RatePerHour = 10,
                HireDate = new DateTime(2023, 06, 10, 8, 30, 0, 0),
                ProviderId = 12,
                Email = "raya@prime.com",
                FullName = "Raya",
                Password = "Raya123!",
                Username = "raya",
                Role = Roles.ADMIN
            };

            ProviderResponseDto providerResponseDto = new ProviderResponseDto()
            {
                Id = 12,
                Name = "Raya",
                Website = "www.prime.com",
                BusinessDomain = "prime",
                Phone = "0888888888",
                StartTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 8, 0, 0, 0),
                EndTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 18, 0, 0, 0),
                WorkingDays = DaysOfWeek.Monday
            };

            EmployeeResponseDto employeeResponseDto = new EmployeeResponseDto();

            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };
            int providerId = 12;
            Regex regex = new Regex($"^[\\w-\\.]+@({providerResponseDto.BusinessDomain}\\.)+[\\w-]{{2,4}}$");

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
           .Returns("PROVIDER_ADMIN");

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.IsAccountPartOfProviderAsync(10, providerId))
            .ReturnsAsync(true);

            providerRepository.Setup(s => s.GetByIdAsync(providerId))
            .ReturnsAsync(providerResponseDto);

            authService.Setup(s => s.HashPasword(requestDto.Password, out salt))
              .Returns(requestDto.Password);

            regex.IsMatch(requestDto.Email);

            await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(requestDto));
        }

        [Fact]
        public async Task UpdateAsync_NewEmployeeNotRelatedToLoggedEmployeeProvider_ThrowsUnauthorizedException()
        {
            int employeeId = 16;

            EmployeeRequestDtoForUpdate requestDto = new EmployeeRequestDtoForUpdate()
            {
                Title = "Raya",
                Phone = "088888888",
                RatePerHour = 10,
                HireDate = new DateTime(2023, 06, 10, 8, 30, 0, 0),
                ProviderId = 12,
                Email = "raya@prime.com",
                FullName = "Raya",
                Username = "raya"
            };

            int providerId = 12;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
           .Returns("PROVIDER_ADMIN");

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.IsAccountPartOfProviderAsync(10, providerId))
            .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.UpdateAsync(employeeId, requestDto));
        }

        [Fact]
        public async Task UpdateAsync_EmployeeTriesToUpdateOthereEmployeeInformation_ThrowsUnauthorizedException()
        {
            int employeeId = 16;

            EmployeeRequestDtoForUpdate requestDto = new EmployeeRequestDtoForUpdate()
            {
                Title = "Raya",
                Phone = "088888888",
                RatePerHour = 10,
                HireDate = new DateTime(2023, 06, 10, 8, 30, 0, 0),
                ProviderId = 12,
                Email = "raya@prime.com",
                FullName = "Raya",
                Username = "raya"
            };

            int providerId = 12;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
           .Returns("EMPLOYEE");

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.IsAccountPartOfProviderAsync(10, providerId))
            .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesEmployeeMatchAccountAsync(employeeId, 10))
            .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.UpdateAsync(employeeId, requestDto));
        }

        [Fact]
        public async Task UpdateAsync_BussinessDomainDoNotMatchEmailDomain_ThrowsValidationException()
        {
            int employeeId = 16;

            EmployeeRequestDtoForUpdate requestDto = new EmployeeRequestDtoForUpdate()
            {
                Title = "Raya",
                Phone = "088888888",
                RatePerHour = 10,
                HireDate = new DateTime(2023, 06, 10, 8, 30, 0, 0),
                ProviderId = 12,
                Email = "raya@dental.com",
                FullName = "Raya",
                Username = "raya"
            };

            ProviderResponseDto providerResponseDto = new ProviderResponseDto()
            {
                Id = 12,
                Name = "Raya",
                Website = "www.prime.com",
                BusinessDomain = "prime",
                Phone = "0888888888",
                StartTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 8, 0, 0, 0),
                EndTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 18, 0, 0, 0),
                WorkingDays = DaysOfWeek.Monday
            };

            EmployeeResponseDto employeeResponseDto = new EmployeeResponseDto();

            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };
            int providerId = 12;
            Regex regex = new Regex($"^[\\w-\\.]+@({providerResponseDto.BusinessDomain}\\.)+[\\w-]{{2,4}}$");

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
           .Returns("EMPLOYEE");

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.IsAccountPartOfProviderAsync(10, providerId))
            .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesEmployeeMatchAccountAsync(employeeId, 10))
            .ReturnsAsync(true);

            providerRepository.Setup(s => s.GetByIdAsync(providerId))
            .ReturnsAsync(providerResponseDto);

            regex.IsMatch(requestDto.Email);

            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(employeeId, requestDto));
        }

        [Fact]
        public async Task UpdateAsync_EmployeeWithAdminRole_ThrowsValidationException()
        {
            int employeeId = 16;

            EmployeeRequestDtoForUpdate requestDto = new EmployeeRequestDtoForUpdate()
            {
                Title = "Raya",
                Phone = "088888888",
                RatePerHour = 10,
                HireDate = new DateTime(2023, 06, 10, 8, 30, 0, 0),
                ProviderId = 12,
                Email = "raya@prime.com",
                FullName = "Raya",
                Username = "raya",
                Role = Roles.ADMIN
            };

            ProviderResponseDto providerResponseDto = new ProviderResponseDto()
            {
                Id = 12,
                Name = "Raya",
                Website = "www.prime.com",
                BusinessDomain = "prime",
                Phone = "0888888888",
                StartTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 8, 0, 0, 0),
                EndTimeOfTheWorkingDay = new DateTime(2023, 06, 20, 18, 0, 0, 0),
                WorkingDays = DaysOfWeek.Monday
            };

            EmployeeResponseDto employeeResponseDto = new EmployeeResponseDto();

            byte[] salt = { 1, 2, 3, 4, 5, 6, 7 };
            int providerId = 12;
            Regex regex = new Regex($"^[\\w-\\.]+@({providerResponseDto.BusinessDomain}\\.)+[\\w-]{{2,4}}$");

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
           .Returns("EMPLOYEE");

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.IsAccountPartOfProviderAsync(10, providerId))
            .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesEmployeeMatchAccountAsync(employeeId, 10))
            .ReturnsAsync(true);

            providerRepository.Setup(s => s.GetByIdAsync(providerId))
            .ReturnsAsync(providerResponseDto);

            regex.IsMatch(requestDto.Email);

            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(employeeId, requestDto));
        }

        [Fact]
        public async Task DeleteAsync_EmployeeTriesToDeleteAnotherEmployee_ThrowsUnauthorizedException()
        {
            int employeeId = 10;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
             .Returns("PROVIDER_ADMIN");

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.DeleteAsync(employeeId))
             .Returns(Task.CompletedTask);

            repositoryMock.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, employeeId))
              .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.DeleteAsync(employeeId));
        }

        [Fact]
        public async Task GetByIdAsync_EmployeeNotRelatedToLoggedEmployeeProvider_ThrowsUnauthorizedException()
        {
            int employeeId = 10;

            EmployeeResponseDto? employeeResponseDto = new EmployeeResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
           .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
             .Returns("PROVIDER_ADMIN");

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.GetByIdAsync(employeeId))
              .ReturnsAsync(employeeResponseDto);

            repositoryMock.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, employeeId))
              .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetByIdAsync(employeeId));
        }

        [Fact]
        public async Task GetByIdAsync_EmployeeTriesToGetAnotherEmployeeInformation_ThrowsUnauthorizedException()
        {
            int employeeId = 10;

            EmployeeResponseDto? employeeResponseDto = new EmployeeResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
           .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
           .Returns("EMPLOYEE");

            var service = new EmployeeService(repositoryMock.Object, providerRepository.Object, authService.Object, httpContextAccessor.Object,
            validationExtension.Object, employeeRequestDtoValidator, employeeRequestDtoForUpdateValidator, accountRequestDtoValidatorValidator, accountRequestDtoForEmployeeUpdateValidator);

            repositoryMock.Setup(s => s.GetByIdAsync(employeeId))
              .ReturnsAsync(employeeResponseDto);

            repositoryMock.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, employeeId))
              .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesEmployeeMatchAccountAsync(employeeId, 10))
               .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetByIdAsync(employeeId));
        }
    }
}
