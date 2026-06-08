using HubMeteorologico.Domain.Exceptions;
using HubMeteorologico.Domain.ResponseDefault;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace MotoRental.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;


        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;

        }
        public Task Invoke(HttpContext context) => this.InvokeAsync(context);

        async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);

            }
            catch (Exception exception)
            {
                if (exception is FluentValidationException validationException)
                {
                    await HandleValidationExceptionAsync(context, validationException);
                    _logger.LogError(exception, "An error occurred while processing validation.");
                }
                else
                {
                    await HandleExceptionAsync(context, exception);
                    _logger.LogError(exception, "An error occurred while processing the request.");

                }

            }
        }

        [ExcludeFromCodeCoverage]
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var statusCode = MapHttpStatusCode(exception);

            context.Response.StatusCode = (int)statusCode;

            var response = GetErrorResponse(exception, statusCode);

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }

        private async Task HandleValidationExceptionAsync(HttpContext context, FluentValidationException exception)
        {
            var response = new ReturnDefault
            {
                StatusCode = HttpStatusCode.BadRequest,
                ModelState = exception.Errors,
                Message = exception.Message
            };
            
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }

        public ReturnDefault GetErrorResponse(Exception exception, HttpStatusCode statusCode)
        {
            return new ReturnDefault(statusCode)
            {
                Message = exception.Message,
                Exception = exception
            };
        }

        public HttpStatusCode MapHttpStatusCode(Exception exception) => exception switch
        {
            var e when e is FluentValidationException => HttpStatusCode.BadRequest,
            var e when e is BadRequestException => HttpStatusCode.BadRequest,
            var e when e is NotFoundException => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };
    }
}
