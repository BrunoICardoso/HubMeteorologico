using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MotoRental.API.ConfigController
{
    public class SwaggerDefaultValues : IOperationFilter
    {
        
        private bool IsEndpointDeprecated(ApiDescription apiDescription)
        {
            if (apiDescription.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                return descriptor.MethodInfo.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any()
                    || descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any();
            }

            return false;
        }

        
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            operation.Deprecated |= IsEndpointDeprecated(apiDescription);

            if (operation.Parameters == null)
            {
                return;
            }

            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.FirstOrDefault(p => p.Name == parameter.Name);

                if (description == null)
                {
                    throw new InvalidOperationException($"Parameter {parameter.Name} not found in API description.");
                }

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                if (parameter.Schema.Default == null && description.DefaultValue != null)
                {
                    parameter.Schema.Default = new OpenApiString(description.DefaultValue.ToString());
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }


}
