using FluentValidation;
using FluentValidation.Results;
using HubMeteorologico.Domain.Exceptions;

namespace HubMeteorologico.API.ConfigController
{
    public abstract class AbstractValidatorCustom<T> : AbstractValidator<T>
    {

        public override ValidationResult Validate(ValidationContext<T> context)
        {
            var validationResult = base.Validate(context);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(e => e.ErrorMessage).ToArray());

                throw new FluentValidationException(errors);
            }

            return validationResult;
        }


        protected void RaiseValidationException(ValidationContext<T> context, ValidationResult validationResult)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(e => e.ErrorMessage).ToArray());

            throw new FluentValidationException(errors);
        }
    }

}
