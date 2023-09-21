using FluentValidation;
using Moq;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Appointments;
using RushHour.Domain.Exceptions;
using RushHour.Domain.FluentValidations.Appointments;
using RushHour.Domain.Pagination;
using RushHour.Domain.Services;
using ValidationException = RushHour.Domain.Exceptions.ValidationException;

namespace RushHour.Services.Tests
{
    public class AppointmentServiceTest
    {
        private readonly Mock<IAppointmentRepository> repositoryMock;
        private readonly Mock<IActivityRepository> activityRepository;
        private readonly Mock<IClientRepository> clientRepository;
        private readonly Mock<IEmployeeRepository> employeeRepository;
        private readonly Mock<IHttpContextAccessorWrapper> httpContextAccessor;
        private readonly Mock<IValidationExtension> validationExtension;
        private readonly IValidator<AppointmentRequestDto> appointmentRequestDtoValidator;
        private readonly IValidator<AppointmentCreateRequestDto> appointmentCreateRequestDtoValidator;

        public AppointmentServiceTest()
        {
            repositoryMock = new Mock<IAppointmentRepository>();
            activityRepository = new Mock<IActivityRepository>();
            clientRepository = new Mock<IClientRepository>();
            employeeRepository = new Mock<IEmployeeRepository>();
            httpContextAccessor = new Mock<IHttpContextAccessorWrapper>();
            validationExtension = new Mock<IValidationExtension>();
            appointmentRequestDtoValidator = new AppointmentRequestDtoValidator();
            appointmentCreateRequestDtoValidator = new AppointmentCreateRequestDtoValidator();
        }

        private void CompareProperties(AppointmentResponseDto responseDto, AppointmentResponseDto actual)
        {
            Assert.Equal(responseDto.StartDate, actual.StartDate);
            Assert.Equal(responseDto.EmployeeId, actual.EmployeeId);
            Assert.Equal(responseDto.ClientId, actual.ClientId);
            Assert.Equal(responseDto.ActivityId, actual.ActivityId);
        }

        [Fact]
        public async Task CreateAsync_SuccessfullyCreateAppointment_ReturnsTheNewAppointment()
        {
            List<int> activityIds = new List<int>() { 46, 39 };

            AppointmentCreateRequestDto requestDto = new AppointmentCreateRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityIds = activityIds };

            List<AppointmentResponseDto> requestDtos = new List<AppointmentResponseDto>
            {
                new AppointmentResponseDto { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 },
                new AppointmentResponseDto { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 39 }
            };

            AppointmentsAndPriceResponseDto appointmentsAndPrice = new AppointmentsAndPriceResponseDto();
            appointmentsAndPrice.Appointments = requestDtos;
            appointmentsAndPrice.TotalPrice = 500;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.CreateAsync(requestDto))
                .ReturnsAsync(appointmentsAndPrice);

            activityRepository.Setup(s => s.CheckIfEmployeeIsPartOfActivities(30, activityIds))
                .Returns(true);

            clientRepository.Setup(s => s.DoesClientMatchAccountAsync(16, 10))
                .ReturnsAsync(true);

            employeeRepository.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, 30))
                .ReturnsAsync(true);

            employeeRepository.Setup(s => s.DoesEmployeeMatchAccountAsync(30, 10))
                .ReturnsAsync(true);

            employeeRepository.Setup(s => s.CheckIfEmployeeIsFreeInSpecificTimeAsync(new DateTime(2023, 05, 29, 7, 20, 0, 0), 30, activityIds))
                .ReturnsAsync(true);

            var actual = await service.CreateAsync(requestDto);

            Assert.Equal(appointmentsAndPrice.TotalPrice, actual.TotalPrice);
            Assert.Equal(appointmentsAndPrice.Appointments, actual.Appointments);
        }

        [Fact]
        public async Task UpdateAsync_SuccessfullyUpdateAppointment_ReturnsTheUpdatedAppointment()
        {
            AppointmentRequestDto requestDto = new AppointmentRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 };
            int appointmentId = 10;

            AppointmentResponseDto appointmentResponseDto = new AppointmentResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.UpdateAsync(appointmentId, requestDto))
                .ReturnsAsync(appointmentResponseDto);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
                .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, 10))
                .ReturnsAsync(true);

            repositoryMock.Setup(s => s.AreAppointmentAndAccountPartOfTheSameProviderAsync(appointmentId, 10))
                .ReturnsAsync(true);

            employeeRepository.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, 30))
                .ReturnsAsync(true);

            activityRepository.Setup(s => s.CheckIfEmployeeIsPartOfActivity(30, 46))
                .Returns(true);

            employeeRepository.Setup(s => s.CheckIfEmployeeIsFreeInSpecificTimeForUpdateAsync(appointmentId, new DateTime(2023, 05, 29, 7, 20, 0, 0), 30, 46))
                .ReturnsAsync(true);

            var actual = await service.UpdateAsync(appointmentId, requestDto);

            CompareProperties(appointmentResponseDto, actual);
        }

        [Fact]
        public async Task DeleteAsync_SuccessfullyDeleteAppointment_VerifyThatDeleteWasInvockedOnce()
        {
            int appointmentId = 10;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(10);

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.DeleteAsync(appointmentId))
             .Returns(Task.CompletedTask);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
              .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, 10))
               .ReturnsAsync(true);

            repositoryMock.Setup(s => s.AreAppointmentAndAccountPartOfTheSameProviderAsync(appointmentId, 10))
              .ReturnsAsync(true);

            await service.DeleteAsync(appointmentId);

            repositoryMock.Verify(x => x.DeleteAsync(appointmentId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_SuccessfullyGetAppointmentById_ReturnASpecificAppointmentById()
        {
            int appointmentId = 10;

            AppointmentResponseDto? appointmentResponseDto = new AppointmentResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
           .Returns(10);

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.GetByIdAsync(appointmentId))
              .ReturnsAsync(appointmentResponseDto);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
              .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, 10))
              .ReturnsAsync(true);

            repositoryMock.Setup(s => s.AreAppointmentAndAccountPartOfTheSameProviderAsync(appointmentId, 10))
              .ReturnsAsync(true);

            var actual = await service.GetByIdAsync(appointmentId);

            CompareProperties(appointmentResponseDto, actual);
        }

        [Fact]
        public async Task GetAllAsync_SuccessfullyGetAllAppointments_ReturnNumberOfAppointmentsFromChoosedPage()
        {
            PagingInfo pagingInfo = new PagingInfo();

            List<AppointmentResponseDto> appointmentResponseDtos = new List<AppointmentResponseDto>();

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.GetAllAsync(pagingInfo))
                .ReturnsAsync(appointmentResponseDtos);

            var actual = await service.GetAllAsync(pagingInfo);

            for (int i = 0; i < appointmentResponseDtos.Count; i++)
            {
                Assert.Equal(appointmentResponseDtos[i].StartDate, actual[i].StartDate);
                Assert.Equal(appointmentResponseDtos[i].EndDate, actual[i].EndDate);
                Assert.Equal(appointmentResponseDtos[i].ActivityId, actual[i].ActivityId);
                Assert.Equal(appointmentResponseDtos[i].ClientId, actual[i].ClientId);
                Assert.Equal(appointmentResponseDtos[i].EmployeeId, actual[i].EmployeeId);
            }
        }

        [Fact]
        public async Task CreateAsync_AppointmentNotRelatedToTheEmployee_ThrowsUnauthorizedException()
        {
            List<int> activityIds = new List<int>() { 46, 39 };

            AppointmentCreateRequestDto requestDto = new AppointmentCreateRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityIds = activityIds };

            List<AppointmentResponseDto> requestDtos = new List<AppointmentResponseDto>
            {
                new AppointmentResponseDto { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 },
                new AppointmentResponseDto { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 39 }
            };

            AppointmentsAndPriceResponseDto appointmentsAndPrice = new AppointmentsAndPriceResponseDto();
            appointmentsAndPrice.Appointments = requestDtos;
            appointmentsAndPrice.TotalPrice = 500;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
               .Returns("EMPLOYEE");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.CreateAsync(requestDto))
               .ReturnsAsync(appointmentsAndPrice);

            employeeRepository.Setup(s => s.DoesEmployeeMatchAccountAsync(30, 10))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.CreateAsync(requestDto));
        }

        [Fact]
        public async Task CreateAsync_AppointmentNotRelatedToTheClient_ThrowsUnauthorizedException()
        {
            List<int> activityIds = new List<int>() { 46, 39 };

            AppointmentCreateRequestDto requestDto = new AppointmentCreateRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityIds = activityIds };

            List<AppointmentResponseDto> requestDtos = new List<AppointmentResponseDto>
            {
                new AppointmentResponseDto { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 },
                new AppointmentResponseDto { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 39 }
            };

            AppointmentsAndPriceResponseDto appointmentsAndPrice = new AppointmentsAndPriceResponseDto();
            appointmentsAndPrice.Appointments = requestDtos;
            appointmentsAndPrice.TotalPrice = 500;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
               .Returns("CLIENT");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.CreateAsync(requestDto))
               .ReturnsAsync(appointmentsAndPrice);

            employeeRepository.Setup(s => s.DoesEmployeeMatchAccountAsync(30, 10))
               .ReturnsAsync(true);

            clientRepository.Setup(s => s.DoesClientMatchAccountAsync(16, 10))
               .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.CreateAsync(requestDto));
        }

        [Fact]
        public async Task CreateAsync_EmployeeRelatedtoDiffrentProvider_ThrowsUnauthorizedException()
        {
            List<int> activityIds = new List<int>() { 46, 39 };

            AppointmentCreateRequestDto requestDto = new AppointmentCreateRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityIds = activityIds };

            List<AppointmentResponseDto> requestDtos = new List<AppointmentResponseDto>
            {
                new AppointmentResponseDto { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 },
                new AppointmentResponseDto { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 39 }
            };

            AppointmentsAndPriceResponseDto appointmentsAndPrice = new AppointmentsAndPriceResponseDto();
            appointmentsAndPrice.Appointments = requestDtos;
            appointmentsAndPrice.TotalPrice = 500;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
               .Returns("PROVIDER_ADMIN");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.CreateAsync(requestDto))
                .ReturnsAsync(appointmentsAndPrice);

            employeeRepository.Setup(s => s.DoesEmployeeMatchAccountAsync(30, 10))
                .ReturnsAsync(true);

            clientRepository.Setup(s => s.DoesClientMatchAccountAsync(16, 10))
                .ReturnsAsync(true);

            employeeRepository.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, 30))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.CreateAsync(requestDto));
        }

        [Fact]
        public async Task CreateAsync_MultipleEmployeesRelatedtoDiffrentActivities_ThrowsValidationException()
        {
            List<int> activityIds = new List<int>() { 46, 39 };

            AppointmentCreateRequestDto requestDto = new AppointmentCreateRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityIds = activityIds };

            List<AppointmentResponseDto> requestDtos = new List<AppointmentResponseDto>
            {
                new AppointmentResponseDto { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 },
                new AppointmentResponseDto { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 39 }
            };

            AppointmentsAndPriceResponseDto appointmentsAndPrice = new AppointmentsAndPriceResponseDto();
            appointmentsAndPrice.Appointments = requestDtos;
            appointmentsAndPrice.TotalPrice = 500;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.CreateAsync(requestDto))
                .ReturnsAsync(appointmentsAndPrice);

            employeeRepository.Setup(s => s.DoesEmployeeMatchAccountAsync(30, 10))
                .ReturnsAsync(true);

            clientRepository.Setup(s => s.DoesClientMatchAccountAsync(16, 10))
                .ReturnsAsync(true);

            employeeRepository.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, 30))
                .ReturnsAsync(true);

            activityRepository.Setup(s => s.CheckIfEmployeeIsPartOfActivities(30, activityIds))
                .Returns(false);

            await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(requestDto));
        }

        [Fact]
        public async Task CreateAsync_EmployeeIsBusy_ThrowsValidationException()
        {
            List<int> activityIds = new List<int>() { 46, 39 };

            AppointmentCreateRequestDto requestDto = new AppointmentCreateRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityIds = activityIds };

            List<AppointmentResponseDto> requestDtos = new List<AppointmentResponseDto>
            {
                new AppointmentResponseDto { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 },
                new AppointmentResponseDto { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 39 }
            };

            AppointmentsAndPriceResponseDto appointmentsAndPrice = new AppointmentsAndPriceResponseDto();
            appointmentsAndPrice.Appointments = requestDtos;
            appointmentsAndPrice.TotalPrice = 500;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.CreateAsync(requestDto))
                .ReturnsAsync(appointmentsAndPrice);

            employeeRepository.Setup(s => s.DoesEmployeeMatchAccountAsync(30, 10))
                .ReturnsAsync(true);

            clientRepository.Setup(s => s.DoesClientMatchAccountAsync(16, 10))
                .ReturnsAsync(true);

            employeeRepository.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, 30))
                .ReturnsAsync(true);

            activityRepository.Setup(s => s.CheckIfEmployeeIsPartOfActivities(30, activityIds))
                .Returns(true);

            employeeRepository.Setup(s => s.CheckIfEmployeeIsFreeInSpecificTimeAsync(new DateTime(2023, 05, 29, 7, 20, 0, 0), 30, activityIds))
                 .ReturnsAsync(false);

            await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(requestDto));
        }

        [Fact]
        public async Task UpdateAsync_AppointmentNotRelatedToTheEmployee_ThrowsUnauthorizedException()
        {
            AppointmentRequestDto requestDto = new AppointmentRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 };
            int appointmentId = 10;

            AppointmentResponseDto appointmentResponseDto = new AppointmentResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
               .Returns("EMPLOYEE");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.UpdateAsync(appointmentId, requestDto))
                .ReturnsAsync(appointmentResponseDto);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.UpdateAsync(appointmentId, requestDto));
        }

        [Fact]
        public async Task UpdateAsync_AppointmentNotRelatedToTheClient_ThrowsUnauthorizedException()
        {
            AppointmentRequestDto requestDto = new AppointmentRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 };
            int appointmentId = 10;

            AppointmentResponseDto appointmentResponseDto = new AppointmentResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
               .Returns("CLIENT");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.UpdateAsync(appointmentId, requestDto))
                .ReturnsAsync(appointmentResponseDto);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
                .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, 10))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.UpdateAsync(appointmentId, requestDto));
        }

        [Fact]
        public async Task UpdateAsync_AppointmentNotRealatedToTheProviderOfTheEmployee_ThrowsUnauthorizedException()
        {
            AppointmentRequestDto requestDto = new AppointmentRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 };
            int appointmentId = 10;

            AppointmentResponseDto appointmentResponseDto = new AppointmentResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
               .Returns("PROVIDER_ADMIN");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.UpdateAsync(appointmentId, requestDto))
               .ReturnsAsync(appointmentResponseDto);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
               .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, 10))
               .ReturnsAsync(true);

            repositoryMock.Setup(s => s.AreAppointmentAndAccountPartOfTheSameProviderAsync(appointmentId, 10))
               .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.UpdateAsync(appointmentId, requestDto));
        }

        [Fact]
        public async Task UpdateAsync_AppointmentNotRealatedToTheProviderOfProviderAdmin_ThrowsUnauthorizedException()
        {
            AppointmentRequestDto requestDto = new AppointmentRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 };
            int appointmentId = 10;

            AppointmentResponseDto appointmentResponseDto = new AppointmentResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
               .Returns("PROVIDER_ADMIN");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.UpdateAsync(appointmentId, requestDto))
                .ReturnsAsync(appointmentResponseDto);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
                .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, 10))
                .ReturnsAsync(true);

            repositoryMock.Setup(s => s.AreAppointmentAndAccountPartOfTheSameProviderAsync(appointmentId, 10))
                .ReturnsAsync(true);

            employeeRepository.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, 30))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.UpdateAsync(appointmentId, requestDto));
        }

        [Fact]
        public async Task UpdateAsync_EmployeeIsNotRelatedToTheAppointmentActivity_ThrowsValidationException()
        {
            AppointmentRequestDto requestDto = new AppointmentRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 };
            int appointmentId = 10;

            AppointmentResponseDto appointmentResponseDto = new AppointmentResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.UpdateAsync(appointmentId, requestDto))
                .ReturnsAsync(appointmentResponseDto);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
                .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, 10))
                .ReturnsAsync(true);

            repositoryMock.Setup(s => s.AreAppointmentAndAccountPartOfTheSameProviderAsync(appointmentId, 10))
                .ReturnsAsync(true);

            employeeRepository.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, 30))
                .ReturnsAsync(true);

            activityRepository.Setup(s => s.CheckIfEmployeeIsPartOfActivity(30, 46))
                .Returns(false);

            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(appointmentId, requestDto));
        }

        [Fact]
        public async Task UpdateAsync_EmployeeIsBusy_ThrowsValidationException()
        {
            AppointmentRequestDto requestDto = new AppointmentRequestDto() { StartDate = new DateTime(2023, 05, 29, 7, 20, 0, 0), EmployeeId = 30, ClientId = 16, ActivityId = 46 };
            int appointmentId = 10;

            AppointmentResponseDto appointmentResponseDto = new AppointmentResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
              .Returns(10);

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.UpdateAsync(appointmentId, requestDto))
               .ReturnsAsync(appointmentResponseDto);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
               .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, 10))
               .ReturnsAsync(true);

            repositoryMock.Setup(s => s.AreAppointmentAndAccountPartOfTheSameProviderAsync(appointmentId, 10))
               .ReturnsAsync(true);

            employeeRepository.Setup(s => s.AreEmployeeAndAccountPartOfTheSameProviderAsync(10, 30))
               .ReturnsAsync(true);

            activityRepository.Setup(s => s.CheckIfEmployeeIsPartOfActivity(30, 46))
               .Returns(true);

            employeeRepository.Setup(s => s.CheckIfEmployeeIsFreeInSpecificTimeForUpdateAsync(appointmentId, new DateTime(2023, 05, 29, 7, 20, 0, 0), 30, 46))
               .ReturnsAsync(false);

            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(appointmentId, requestDto));
        }

        [Fact]
        public async Task DeleteAsync_AppointmentNotRelatedToTheEmployee_ThrowsUnauthorizedException()
        {
            int appointmentId = 10;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
          .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
             .Returns("EMPLOYEE");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.DeleteAsync(appointmentId))
              .Returns(Task.CompletedTask);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
              .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.DeleteAsync(appointmentId));
        }

        [Fact]
        public async Task DeleteAsync_AppointmentNotRelatedToTheClient_ThrowsUnauthorizedException()
        {
            int appointmentId = 10;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
             .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
             .Returns("CLIENT");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.DeleteAsync(appointmentId))
              .Returns(Task.CompletedTask);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
              .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, 10))
             .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.DeleteAsync(appointmentId));
        }

        [Fact]
        public async Task DeleteAsync_EmployeeRelatedtoDiffrentProvider_ThrowsUnauthorizedException()
        {
            int appointmentId = 10;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
             .Returns("PROVIDER_ADMIN");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.DeleteAsync(appointmentId))
              .Returns(Task.CompletedTask);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
              .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, 10))
             .ReturnsAsync(true);

            repositoryMock.Setup(s => s.AreAppointmentAndAccountPartOfTheSameProviderAsync(appointmentId, 10))
             .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.DeleteAsync(appointmentId));
        }

        [Fact]
        public async Task GetByIdAsync_AppointmentNotRelatedToTheEmployee_ThrowsUnauthorizedException()
        {
            int appointmentId = 10;

            AppointmentResponseDto? appointmentResponseDto = new AppointmentResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
             .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
             .Returns("EMPLOYEE");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.GetByIdAsync(appointmentId))
              .ReturnsAsync(appointmentResponseDto);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
              .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetByIdAsync(appointmentId));
        }

        [Fact]
        public async Task GetByIdAsync_AppointmentNotRelatedToTheClient_ThrowsUnauthorizedException()
        {
            int appointmentId = 10;

            AppointmentResponseDto? appointmentResponseDto = new AppointmentResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
             .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
             .Returns("CLIENT");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.GetByIdAsync(appointmentId))
              .ReturnsAsync(appointmentResponseDto);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
              .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, 10))
             .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetByIdAsync(appointmentId));
        }

        [Fact]
        public async Task GetByIdAsync_EmployeeRelatedtoDiffrentProvider_ThrowsUnauthorizedException()
        {
            int appointmentId = 10;

            AppointmentResponseDto? appointmentResponseDto = new AppointmentResponseDto();

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
            .Returns("PROVIDER_ADMIN");

            var service = new AppointmentService(repositoryMock.Object, activityRepository.Object, clientRepository.Object, employeeRepository.Object,
                httpContextAccessor.Object, validationExtension.Object, appointmentRequestDtoValidator, appointmentCreateRequestDtoValidator);

            repositoryMock.Setup(s => s.GetByIdAsync(appointmentId))
              .ReturnsAsync(appointmentResponseDto);

            repositoryMock.Setup(s => s.DoesAppointmentEmployeeAccountMatchLoggedUserAccountAsync(appointmentId, 10))
              .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DoesAppointmentClientAccountMatchLoggedUserAccountAsync(appointmentId, 10))
              .ReturnsAsync(true);

            repositoryMock.Setup(s => s.AreAppointmentAndAccountPartOfTheSameProviderAsync(appointmentId, 10))
              .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetByIdAsync(appointmentId));
        }
    }
}