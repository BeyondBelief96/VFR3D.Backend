using System.Net;
using System.Text.Json;
using VFR3D.API.Models;
using VFR3D.Domain.Exceptions;

namespace VFR3D.API.Middleware;

/// <summary>
/// Middleware that handles all unhandled exceptions and converts them to structured API responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
        var (statusCode, errorResponse) = MapExceptionToResponse(context, exception);

        // Log the exception
        LogException(exception, statusCode, context.Request.Path);

        // Write the response
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, JsonOptions));
    }

    private (HttpStatusCode statusCode, ApiErrorResponse response) MapExceptionToResponse(
        HttpContext context,
        Exception exception)
    {
        var timestamp = DateTime.UtcNow.ToString("o");
        var traceId = context.TraceIdentifier;
        var path = context.Request.Path.Value;

        return exception switch
        {
            // Domain exceptions - NotFoundException hierarchy
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                CreateErrorResponse(notFoundEx.ErrorCode, notFoundEx.UserMessage, timestamp, traceId, path, exception)),

            // Domain exceptions - ConflictException hierarchy
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                CreateErrorResponse(conflictEx.ErrorCode, conflictEx.UserMessage, timestamp, traceId, path, exception)),

            // Domain exceptions - ValidationException
            Domain.Exceptions.ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                CreateValidationErrorResponse(validationEx, timestamp, traceId, path, exception)),

            // Domain exceptions - ExternalServiceException hierarchy
            ExternalServiceException serviceEx => (
                HttpStatusCode.ServiceUnavailable,
                CreateErrorResponse(serviceEx.ErrorCode, serviceEx.UserMessage, timestamp, traceId, path, exception)),

            // Domain exceptions - Other domain exceptions
            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                CreateErrorResponse(domainEx.ErrorCode, domainEx.UserMessage, timestamp, traceId, path, exception)),

            // Legacy exception types for backward compatibility
            KeyNotFoundException keyNotFoundEx => (
                HttpStatusCode.NotFound,
                CreateErrorResponse(ErrorCodes.NotFound, keyNotFoundEx.Message, timestamp, traceId, path, exception)),

            ResourceNotFoundException resourceNotFoundEx => (
                HttpStatusCode.NotFound,
                CreateErrorResponse(ErrorCodes.NotFound, resourceNotFoundEx.Message, timestamp, traceId, path, exception)),

            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                CreateErrorResponse(ErrorCodes.ValidationError, argEx.Message, timestamp, traceId, path, exception)),

            InvalidOperationException invalidOpEx => (
                HttpStatusCode.Conflict,
                CreateErrorResponse(ErrorCodes.Conflict, invalidOpEx.Message, timestamp, traceId, path, exception)),

            System.Data.DuplicateNameException duplicateEx => (
                HttpStatusCode.Conflict,
                CreateErrorResponse(ErrorCodes.Conflict, duplicateEx.Message, timestamp, traceId, path, exception)),

            InvalidDataException invalidDataEx => (
                HttpStatusCode.BadRequest,
                CreateErrorResponse(ErrorCodes.ValidationError, invalidDataEx.Message, timestamp, traceId, path, exception)),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                CreateErrorResponse(ErrorCodes.InternalError, "Unauthorized access.", timestamp, traceId, path, exception)),

            // Unhandled exceptions
            _ => (
                HttpStatusCode.InternalServerError,
                CreateErrorResponse(
                    ErrorCodes.InternalError,
                    "An unexpected error occurred. Please try again later.",
                    timestamp,
                    traceId,
                    path,
                    exception))
        };
    }

    private ApiErrorResponse CreateErrorResponse(
        string code,
        string message,
        string timestamp,
        string traceId,
        string? path,
        Exception exception)
    {
        return new ApiErrorResponse
        {
            Code = code,
            Message = message,
            Details = _environment.IsDevelopment() ? exception.ToString() : null,
            Timestamp = timestamp,
            TraceId = traceId,
            Path = path
        };
    }

    private ApiErrorResponse CreateValidationErrorResponse(
        Domain.Exceptions.ValidationException validationException,
        string timestamp,
        string traceId,
        string? path,
        Exception exception)
    {
        return new ApiErrorResponse
        {
            Code = validationException.ErrorCode,
            Message = validationException.UserMessage,
            ValidationErrors = validationException.ValidationErrors.Count > 0
                ? validationException.ValidationErrors
                : null,
            Details = _environment.IsDevelopment() ? exception.ToString() : null,
            Timestamp = timestamp,
            TraceId = traceId,
            Path = path
        };
    }

    private void LogException(Exception exception, HttpStatusCode statusCode, string path)
    {
        var logLevel = statusCode switch
        {
            HttpStatusCode.InternalServerError => LogLevel.Error,
            HttpStatusCode.ServiceUnavailable => LogLevel.Warning,
            _ => LogLevel.Information
        };

        _logger.Log(
            logLevel,
            exception,
            "HTTP {StatusCode} - {ExceptionType} at {Path}: {Message}",
            (int)statusCode,
            exception.GetType().Name,
            path,
            exception.Message);
    }
}

/// <summary>
/// Extension methods for registering the GlobalExceptionMiddleware.
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    /// <summary>
    /// Adds global exception handling middleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
