using System.Net;
using System.Text.Json;
using ConstructionPayment.Api.Models;
using ConstructionPayment.Application.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Hosting;

namespace ConstructionPayment.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse();
        var isDevelopment = false;

        try
        {
            var environmentName = (context.RequestServices.GetService(typeof(IHostEnvironment)) as IHostEnvironment)?.EnvironmentName;
            isDevelopment = string.Equals(environmentName, Environments.Development, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            isDevelopment = false;
        }

        switch (exception)
        {
            case AppException appException:
                context.Response.StatusCode = appException.StatusCode;
                response.StatusCode = appException.StatusCode;
                response.Message = appException.Message;
                break;

            case ValidationException validationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusCode = context.Response.StatusCode;
                response.Message = "Dữ liệu không hợp lệ.";
                response.Errors = validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray());
                break;

            default:
                _logger.LogError(exception, "Unhandled exception");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.StatusCode = context.Response.StatusCode;
                response.Message = isDevelopment
                    ? $"[DEV] {exception.GetType().Name}: {exception.Message}"
                    : "Có lỗi hệ thống xảy ra. Vui lòng thử lại.";

                if (isDevelopment)
                {
                    response.Errors = new
                    {
                        exceptionType = exception.GetType().FullName,
                        stackTrace = exception.StackTrace
                    };
                }
                break;
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
