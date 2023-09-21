using FluentValidation.Results;

namespace RushHour.Domain.Abstractions.Extensions
{
    public interface IValidationExtension
    {
        public void ValidateValidationResult(ValidationResult result);
    }
}
