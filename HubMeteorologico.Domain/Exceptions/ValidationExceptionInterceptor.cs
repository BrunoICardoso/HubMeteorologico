using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace HubMeteorologico.Domain.Exceptions;

public class ValidationExceptionInterceptor : IValidatorInterceptor
{

    public FluentValidation.IValidationContext BeforeAspNetValidation(ActionContext actionContext, FluentValidation.IValidationContext commonContext)
    {
        return commonContext;
    }

    public FluentValidation.Results.ValidationResult AfterAspNetValidation(ActionContext actionContext, FluentValidation.IValidationContext validationContext, FluentValidation.Results.ValidationResult result)
    {
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            throw new FluentValidationException(errors);
        }
        return result;
    }
}
