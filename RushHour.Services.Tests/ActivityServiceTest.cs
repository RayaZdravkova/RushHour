using FluentValidation;
using FluentValidation.Results;
using Moq;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Abstractions.Repositories;
using RushHour.Domain.Abstractions.Wrappers;
using RushHour.Domain.DTOs.Activities;
using RushHour.Domain.Exceptions;
using RushHour.Domain.FluentValidations.Activities;
using RushHour.Domain.Pagination;
using RushHour.Domain.Services;
using ValidationException = RushHour.Domain.Exceptions.ValidationException;

namespace RushHour.Services.Tests
{
    public class ActivityServiceTest
    {
        private readonly Mock<IActivityRepository> repositoryMock;
        private readonly Mock<IEmployeeRepository> employeeRepository;
        private readonly Mock<IHttpContextAccessorWrapper> httpContextAccessor;
        private readonly Mock<IValidationExtension> validationExtension;
        private readonly IValidator<ActivityRequestDto> activityRequestDtoValidator;

        public ActivityServiceTest()
        {
            repositoryMock = new Mock<IActivityRepository>();
            employeeRepository = new Mock<IEmployeeRepository>();
            httpContextAccessor = new Mock<IHttpContextAccessorWrapper>();
            validationExtension = new Mock<IValidationExtension>();
            activityRequestDtoValidator = new ActivityRequestDtoValidator();
        }
        private void CompareProperties(ActivityResponseDto responseDto,ActivityResponseDto actual)
        {
            Assert.Equal(responseDto.Name, actual.Name);
            Assert.Equal(responseDto.Price, actual.Price);
            Assert.Equal(responseDto.Duration, actual.Duration);
            Assert.Equal(responseDto.ProviderId, actual.ProviderId);
            Assert.Equal(responseDto.EmployeeIds, actual.EmployeeIds);
        }

        [Fact]
        public async Task CreateAsync_SuccessfullyCreateActivity_ReturnsTheNewActivity()
        {
            var employeeIds = new List<int>() { 1,2,3,4};

            ActivityRequestDto requestDto = new ActivityRequestDto()
            {
                Name = "Cleaning",
                Price = 30,
                Duration = 120,
                EmployeeIds = employeeIds
            };    
            
            ActivityResponseDto responseDto = new ActivityResponseDto()
            {
                Name = "Cleaning",
                Price = 30,
                Duration = 120,
                ProviderId = 2,
                EmployeeIds = employeeIds
            };

            var loggedUserId = 10;
            var loggedUserProviderId = 20;

            var service = new ActivityService(repositoryMock.Object, employeeRepository.Object, httpContextAccessor.Object, validationExtension.Object, activityRequestDtoValidator);

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(loggedUserId);

            employeeRepository.Setup(s => s.GetEmployeeProviderIdByAccountIdAsync(loggedUserId))
            .ReturnsAsync(loggedUserProviderId);

            employeeRepository.Setup(s => s.CheckEmployeeProvidersPartOfTheSameProvider(employeeIds, loggedUserProviderId))
            .Returns(true);

            repositoryMock.Setup(s => s.CreateAsync(requestDto, loggedUserProviderId))
            .ReturnsAsync(responseDto);

            var actual = await service.CreateAsync(requestDto);

            CompareProperties(responseDto, actual);
        }

        [Fact]
        public async Task UpdateAsync_SuccessfullyUpdatedActivity_ReturnsTheUpdatedActivity()
        {
            var activityId = 18;
            var employeeIds = new List<int>() { 1,2,3,4};

            ActivityRequestDto requestDto = new ActivityRequestDto()
            {
                Name = "Cleaning",
                Price = 30,
                Duration = 120,
                EmployeeIds = employeeIds
            };    
            
            ActivityResponseDto responseDto = new ActivityResponseDto()
            {
                Name = "Cleaning",
                Price = 30,
                Duration = 120,
                ProviderId = 2,
                EmployeeIds = employeeIds
            };

            var loggedUserId = 10;
            var loggedUserProviderId = 20;
            var loggedUserActivityProviderId = 5;

            httpContextAccessor.Setup(s => s.GetUserRole())
            .Returns("PROVIDER_ADMIN");

            var service = new ActivityService(repositoryMock.Object, employeeRepository.Object, httpContextAccessor.Object, validationExtension.Object, activityRequestDtoValidator);

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(loggedUserId);

            employeeRepository.Setup(s => s.GetEmployeeProviderIdByAccountIdAsync(loggedUserId))
            .ReturnsAsync(loggedUserProviderId);

            employeeRepository.Setup(s => s.CheckEmployeeProvidersPartOfTheSameProvider(employeeIds, loggedUserProviderId))
            .Returns(true);

            repositoryMock.Setup(s => s.GetActivityProviderAsync(activityId))
            .ReturnsAsync(loggedUserActivityProviderId);

            employeeRepository.Setup(s => s.IsAccountPartOfProviderAsync(loggedUserId, loggedUserActivityProviderId))
            .ReturnsAsync(true);

            repositoryMock.Setup(s => s.UpdateAsync(activityId, requestDto))
            .ReturnsAsync(responseDto);

            var actual = await service.UpdateAsync(activityId, requestDto);

            CompareProperties(responseDto, actual);
        }

        [Fact]
        public async Task DeleteAsync_SuccessfullyDeleteActivity_VerifyThatDeleteWasInvockedOnce()
        {
            int activityId = 10;
            var loggedUserId = 10;
            var loggedUserProviderId = 20;
            var loggedUserActivityProviderId = 5;

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(loggedUserId);

            httpContextAccessor.Setup(s => s.GetUserRole())
            .Returns("PROVIDER_ADMIN");

            var service = new ActivityService(repositoryMock.Object, employeeRepository.Object, httpContextAccessor.Object, validationExtension.Object, activityRequestDtoValidator);

            repositoryMock.Setup(s => s.GetActivityProviderAsync(activityId))
            .ReturnsAsync(loggedUserActivityProviderId);

            employeeRepository.Setup(s => s.IsAccountPartOfProviderAsync(loggedUserId, loggedUserActivityProviderId))
            .ReturnsAsync(true);

            repositoryMock.Setup(s => s.DeleteAsync(activityId))
             .Returns(Task.CompletedTask);

            await service.DeleteAsync(activityId);

            repositoryMock.Verify(x => x.DeleteAsync(activityId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_SuccessfullyGetActivtyById_ReturnASpecificActivitById()
        {
            int activityId = 10;
            var loggedUserId = 10;
            var loggedUserProviderId = 20;
            var loggedUserActivityProviderId = 5;
            var employeeIds = new List<int>() { 1, 2, 3, 4 };

            ActivityResponseDto responseDto = new ActivityResponseDto()
            {
                Id = activityId,
                Name = "Cleaning",
                Price = 30,
                Duration = 120,
                ProviderId = 2,
                EmployeeIds = employeeIds
            };

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
            .Returns("PROVIDER_ADMIN");

            var service = new ActivityService(repositoryMock.Object, employeeRepository.Object, httpContextAccessor.Object, validationExtension.Object, activityRequestDtoValidator);

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(loggedUserId);

            repositoryMock.Setup(s => s.GetActivityProviderAsync(activityId))
            .ReturnsAsync(loggedUserActivityProviderId);

            employeeRepository.Setup(s => s.IsAccountPartOfProviderAsync(loggedUserId, loggedUserActivityProviderId))
            .ReturnsAsync(true);  
            
            repositoryMock.Setup(s => s.GetByIdAsync(activityId))
            .ReturnsAsync(responseDto);

            var actual = await service.GetByIdAsync(activityId);

            CompareProperties(responseDto, actual);
        }

        [Fact]
        public async Task GetAllAsync_SuccessfullyGetAllActivities_ReturnNumberOfActivitiesFromChoosedPage()
        {
            PagingInfo pagingInfo = new PagingInfo();

            List<ActivityResponseDto> activityResponseDtos = new List<ActivityResponseDto>();

            var service = new ActivityService(repositoryMock.Object, employeeRepository.Object, httpContextAccessor.Object, validationExtension.Object, activityRequestDtoValidator);

            repositoryMock.Setup(s => s.GetAllAsync(pagingInfo))
                .ReturnsAsync(activityResponseDtos);

            var actual = await service.GetAllAsync(pagingInfo);

            for (int i = 0; i < activityResponseDtos.Count; i++)
            {
                Assert.Equal(activityResponseDtos[i].Name, actual[i].Name);
                Assert.Equal(activityResponseDtos[i].Price, actual[i].Price);
                Assert.Equal(activityResponseDtos[i].Duration, actual[i].Duration);
                Assert.Equal(activityResponseDtos[i].ProviderId, actual[i].ProviderId);
                Assert.Equal(activityResponseDtos[i].EmployeeIds, actual[i].EmployeeIds);
            }
        }

        [Fact]
        public async Task CreateAsync_OneEmployeeIsNotRelatedToProvider_ThrowsValidationException()
        {
            var employeeIds = new List<int>() { 1, 2, 3, 4 };

            ActivityRequestDto requestDto = new ActivityRequestDto()
            {
                Name = "Cleaning",
                Price = 30,
                Duration = 120,
                EmployeeIds = employeeIds
            };

            var loggedUserId = 10;
            var loggedUserProviderId = 20;

            var service = new ActivityService(repositoryMock.Object, employeeRepository.Object, httpContextAccessor.Object, validationExtension.Object, activityRequestDtoValidator);

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(loggedUserId);

            employeeRepository.Setup(s => s.GetEmployeeProviderIdByAccountIdAsync(loggedUserId))
            .ReturnsAsync(loggedUserProviderId);

            employeeRepository.Setup(s => s.CheckEmployeeProvidersPartOfTheSameProvider(employeeIds, loggedUserProviderId))
            .Returns(false);

            await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(requestDto));
        }
        
        [Fact]
        public async Task UpdateAsync_OneEmployeeIsNotRelatedToProvider_ThrowsValidationException()
        {
            var activityId = 6;
            var employeeIds = new List<int>() { 1, 2, 3, 4 };

            ActivityRequestDto requestDto = new ActivityRequestDto()
            {
                Name = "Cleaning",
                Price = 30,
                Duration = 120,
                EmployeeIds = employeeIds
            };

            var loggedUserId = 10;
            var loggedUserProviderId = 20;

            var service = new ActivityService(repositoryMock.Object, employeeRepository.Object, httpContextAccessor.Object, validationExtension.Object, activityRequestDtoValidator);

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(loggedUserId);

            employeeRepository.Setup(s => s.GetEmployeeProviderIdByAccountIdAsync(loggedUserId))
            .ReturnsAsync(loggedUserProviderId);

            employeeRepository.Setup(s => s.CheckEmployeeProvidersPartOfTheSameProvider(employeeIds, loggedUserProviderId))
            .Returns(false);

            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(activityId, requestDto));
        }
        
        [Fact]
        public async Task UpdateAsync_EmployeeNotRelatedToLoggedEmployeeProvider_ThrowsUnauthorizedException()
        {
            var activityId = 6;
            var employeeIds = new List<int>() { 1, 2, 3, 4 };

            ActivityRequestDto requestDto = new ActivityRequestDto()
            {
                Name = "Cleaning",
                Price = 30,
                Duration = 120,
                EmployeeIds = employeeIds
            };

            var loggedUserId = 10;
            var loggedUserProviderId = 20;
            var loggedUserActivityProviderId = 5;

            httpContextAccessor.Setup(s => s.GetUserRole())
            .Returns("PROVIDER_ADMIN");

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(loggedUserId);

            var service = new ActivityService(repositoryMock.Object, employeeRepository.Object, httpContextAccessor.Object, validationExtension.Object, activityRequestDtoValidator);

            employeeRepository.Setup(s => s.GetEmployeeProviderIdByAccountIdAsync(loggedUserId))
            .ReturnsAsync(loggedUserProviderId);

            employeeRepository.Setup(s => s.CheckEmployeeProvidersPartOfTheSameProvider(employeeIds, loggedUserProviderId))
            .Returns(true);

            repositoryMock.Setup(s => s.GetActivityProviderAsync(activityId))
            .ReturnsAsync(loggedUserActivityProviderId);

            employeeRepository.Setup(s => s.IsAccountPartOfProviderAsync(loggedUserId, loggedUserActivityProviderId))
            .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.UpdateAsync(activityId, requestDto));
        }

        [Fact]
        public async Task DeleteAsync_EmployeeNotRelatedToLoggedEmployeeProvider_ThrowsUnauthorizedException()
        {
            int activityId = 10;
            var loggedUserId = 10;
            var loggedUserActivityProviderId = 5;

            httpContextAccessor.Setup(s => s.GetUserRole())
            .Returns("PROVIDER_ADMIN");

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(loggedUserId);

            var service = new ActivityService(repositoryMock.Object, employeeRepository.Object, httpContextAccessor.Object, validationExtension.Object, activityRequestDtoValidator);

            repositoryMock.Setup(s => s.GetActivityProviderAsync(activityId))
            .ReturnsAsync(loggedUserActivityProviderId);

            employeeRepository.Setup(s => s.IsAccountPartOfProviderAsync(loggedUserId, loggedUserActivityProviderId))
            .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.DeleteAsync(activityId));
        }

        [Fact]
        public async Task GetByIdAsync_EmployeeNotRelatedToLoggedEmployeeProvider_ThrowsUnauthorizedException()
        {
            int activityId = 10;
            var loggedUserId = 10;
            var loggedUserProviderId = 20;
            var loggedUserActivityProviderId = 5;
            var employeeIds = new List<int>() { 1, 2, 3, 4 };

            ActivityResponseDto responseDto = new ActivityResponseDto()
            {
                Id = activityId,
                Name = "Cleaning",
                Price = 30,
                Duration = 120,
                ProviderId = 2,
                EmployeeIds = employeeIds
            };

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(10);

            httpContextAccessor.Setup(s => s.GetUserRole())
            .Returns("PROVIDER_ADMIN");

            var service = new ActivityService(repositoryMock.Object, employeeRepository.Object, httpContextAccessor.Object, validationExtension.Object, activityRequestDtoValidator);

            httpContextAccessor.Setup(s => s.GetLoggedUserId())
            .Returns(loggedUserId);

            repositoryMock.Setup(s => s.GetActivityProviderAsync(activityId))
            .ReturnsAsync(loggedUserActivityProviderId);

            employeeRepository.Setup(s => s.IsAccountPartOfProviderAsync(loggedUserId, loggedUserActivityProviderId))
            .ReturnsAsync(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetByIdAsync(activityId));
        }
    }
}
