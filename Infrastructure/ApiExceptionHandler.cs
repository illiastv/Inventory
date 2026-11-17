using InventoryLab3.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InventoryLab3.Infrastructure;

public class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int status;
        string title;
        string? detail;

        if (exception is NotFoundException)
        {
            status = StatusCodes.Status404NotFound;
            title = "Ресурс не знайдено";
            detail = exception.Message;
        }
        else if (exception is UnauthorizedException)
        {
            status = StatusCodes.Status401Unauthorized;
            title = "Не автентифіковано";
            detail = exception.Message;
        }
        else if (exception is ForbiddenException)
        {
            status = StatusCodes.Status403Forbidden;
            title = "Доступ заборонено";
            detail = exception.Message;
        }
        else if (exception is ConflictException)
        {
            status = StatusCodes.Status409Conflict;
            title = "Конфлікт";
            detail = exception.Message;
        }
        else if (exception is ArgumentException)
        {
            status = StatusCodes.Status400BadRequest;
            title = "Невалідні дані";
            detail = exception.Message;
        }
        else
        {
            _logger.LogError(exception, "Необроблена помилка");
            status = StatusCodes.Status500InternalServerError;
            title = "Внутрішня помилка сервера";
            detail = null;
        }

        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail };

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
