using System.Net;
using System.Text.Json;
using UserManagement.Application.Wrappers;
using UserManagement.Domain.Exceptions;

namespace UserManagement.API.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            await HandleExceptionAsync(context, error);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception error)
    {
        var response = context.Response;
        response.ContentType = "application/json";
        var responseModel = new ApiResponse<string>(error.Message) { Succeeded = false };

        switch (error)
        {
            case NotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case ValidationException:
            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            default:
                _logger.LogError(error, "An unhandled exception has occurred.");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                responseModel.Message = "Internal Server Error. Please contact support.";
                break;
        }

        var result = JsonSerializer.Serialize(responseModel);
        await response.WriteAsync(result);
    }
}