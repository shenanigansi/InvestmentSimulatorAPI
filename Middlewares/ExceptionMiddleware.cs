using System.Text.Json;
using InvestmentSimulatorAPI.Exceptions;

namespace InvestmentSimulatorAPI.Middlewares;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, 
                $"Ошибка, возникщая при запросе " +
                $"{context.Request.Method}" +
                $" {context.Request.Path}");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            // Ошибка неверного аргумена передаваемого пользователем
            ArgumentException => new ErrorResponse(
                StatusCodes.Status400BadRequest,
                exception.Message),

            // Ошибка поиска сущности
            KeyNotFoundException => new ErrorResponse(
                StatusCodes.Status404NotFound,
                exception.Message),

            // Нет доступа к определннному ресурсу на пк
            UnauthorizedAccessException => new ErrorResponse(
                StatusCodes.Status401Unauthorized,
                exception.Message),
            
            //Нет доступа к ресурсу сервера
            ForbiddenException => new ErrorResponse(
                StatusCodes.Status403Forbidden,
                exception.Message),
                
            // Ошибка состояния системы
            InvalidOperationException => new ErrorResponse(
                StatusCodes.Status409Conflict,
                exception.Message), 
                    
            // Пользователь не авторизован
            UnauthorizedException => new ErrorResponse(
                StatusCodes.Status401Unauthorized,
                exception.Message),
                
            // Возврат Problem как варриант
            _ => new ErrorResponse(
                StatusCodes.Status500InternalServerError,
                "Internal server error.")
        };

        context.Response.StatusCode = response.StatusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }

    private sealed record ErrorResponse(
        int StatusCode,
        string Message);
}