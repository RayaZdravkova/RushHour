using FluentValidation.Results;
using RushHour.Domain.Abstractions.Extensions;
using RushHour.Domain.Exceptions;

namespace RushHour.Domain.Services
{
    public class ValidationExtension : IValidationExtension
    {
        public void ValidateValidationResult(ValidationResult result)
        {
            if (!result.IsValid)
            {
                var message = string.Join(";  ", result.Errors.Select(x => x.ErrorMessage));
                throw new ValidationException(message);
            }
        }
    }
}

