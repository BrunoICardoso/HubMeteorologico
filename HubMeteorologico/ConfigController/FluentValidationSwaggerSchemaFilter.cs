using FluentValidation;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Validators;

namespace MotoRental.API.ConfigController
{
    public class FluentValidationSwaggerSchemaFilter : ISchemaFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public FluentValidationSwaggerSchemaFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {

            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();
                Enum.GetValues(context.Type).Cast<Enum>().ToList().ForEach(en =>
                    schema.Enum.Add(new OpenApiString($"{en.ToString()} ({Convert.ToInt32(en)})")));
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var validatorType = typeof(IValidator<>).MakeGenericType(context.Type);
                var validator = serviceProvider.GetService(validatorType) as IValidator;

                if (validator == null) return;

                var descriptor = validator.CreateDescriptor();
                var membersWithValidators = descriptor.GetMembersWithValidators();

                var propertyNamesWithValidators = membersWithValidators
                    .Select(m => m.Key)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var property in schema.Properties)
                {
                    var propertyName = property.Key;

                    var matchingPropertyName = propertyNamesWithValidators
                        .FirstOrDefault(p => string.Equals(p, propertyName, StringComparison.OrdinalIgnoreCase));

                    if (matchingPropertyName == null) continue;

                    var validators = membersWithValidators[matchingPropertyName];

                    var hasNotNullOrNotEmpty = validators.Any(v =>
                        v.Validator is INotNullValidator ||
                        v.Validator is INotEmptyValidator);

                    if (hasNotNullOrNotEmpty && !schema.Required.Contains(propertyName))
                    {
                        schema.Required.Add(propertyName);
                        property.Value.Nullable = false;
                    }

                    var hasMinLengthValidator = validators.Any(v => v.Validator is ILengthValidator lengthValidator && lengthValidator.Min > 0);
                    var hasMaxLengthValidator = validators.Any(v => v.Validator is ILengthValidator lengthValidator && lengthValidator.Max < int.MaxValue);

                    if (hasMinLengthValidator)
                    {
                        var minLength = validators.OfType<ILengthValidator>().Max(v => v.Min); 
                        schema.Properties[matchingPropertyName].MinLength = minLength;
                    }

                    if (hasMaxLengthValidator)
                    {
                        var maxLength = validators.OfType<ILengthValidator>().Min(v => v.Max); 
                        schema.Properties[matchingPropertyName].MaxLength = maxLength;
                    }

                    var hasMinValueValidator = validators.Any(v => v.Validator is IComparisonValidator comparisonValidator && comparisonValidator.ValueToCompare != null);
                    var hasMaxValueValidator = validators.Any(v => v.Validator is IComparisonValidator comparisonValidator && comparisonValidator.ValueToCompare != null);

                    if (hasMinValueValidator && schema.Properties.ContainsKey(matchingPropertyName))
                    {
                        var minValue = validators.OfType<IComparisonValidator>()
                            .Where(v => v.Comparison == Comparison.GreaterThanOrEqual)
                            .Max(v => v.ValueToCompare);
                        schema.Properties[matchingPropertyName].Minimum = Convert.ToDecimal(minValue);
                    }


                    if (hasMaxValueValidator && schema.Properties.ContainsKey(matchingPropertyName))
                    {
                        var maxValue = validators.OfType<IComparisonValidator>()
                            .Where(v => v.Comparison == Comparison.GreaterThanOrEqual)
                            .Max(v => v.ValueToCompare);
                        schema.Properties[matchingPropertyName].Maximum = Convert.ToDecimal(maxValue);
                    }

                    var regexValidator = validators.FirstOrDefault(v => v.Validator is IRegularExpressionValidator);

                    if (regexValidator.Validator != null && schema.Properties.ContainsKey(matchingPropertyName))
                    {
                        var pattern = ((IRegularExpressionValidator)regexValidator.Validator).Expression;
                        schema.Properties[matchingPropertyName].Pattern = pattern;
                    }

                }
            }
        }
    }
}