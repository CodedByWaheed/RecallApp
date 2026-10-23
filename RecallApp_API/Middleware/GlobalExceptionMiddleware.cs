
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

namespace RecallApp_API.Middleware
{
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next,ILogger<GlobalExceptionMiddleware> logger)
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
            catch (Exception exception)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogError(exception,"Exception after response started. TraceId: {TraceId}",context.TraceIdentifier);
                    throw;
                }

                (int statusCode, string message) = GetErrorResponse(exception);

                if (statusCode >= 500)
                {
                    _logger.LogError(exception,"Unhandled exception processing {Method} {Path}. TraceId: {TraceId}",
                        context.Request.Method,context.Request.Path,context.TraceIdentifier);
                }
                else
                {
                    _logger.LogWarning("Request failed with status {StatusCode}. Method: {Method}, Path: {Path}, TraceId: {TraceId}",
                        statusCode,context.Request.Method,context.Request.Path,context.TraceIdentifier);
                }

                context.Response.Clear();
                context.Response.StatusCode = statusCode;

                await context.Response.WriteAsJsonAsync(new
                {
                    message,
                    traceId = context.TraceIdentifier
                });
            }
        }

        private static (int StatusCode, string Message) GetErrorResponse(Exception exception)
        {
            if (exception is SqlException sqlException)
            {
                if (sqlException.Number == 50022)
                {
                    return (StatusCodes.Status409Conflict,"Email is already in use.");
                }

                if (sqlException.Number is 2601 or 2627)
                {
                    return (StatusCodes.Status409Conflict,"A record with the same unique value already exists.");
                }
                if (sqlException.Number is 50002)
                {
                    return (StatusCodes.Status409Conflict, "A tag with this name already exists.");
                }

                if (sqlException.Number is 50021 or 50023)
                {
                    return (StatusCodes.Status400BadRequest,sqlException.Message);
                }

                if (sqlException.Number == 50024)
                {
                    return (StatusCodes.Status404NotFound,"User not found.");
                }
            }

            if (exception is ArgumentException)
            {
                return (StatusCodes.Status400BadRequest,exception.Message);
            }

            return (StatusCodes.Status500InternalServerError,"An unexpected error occurred.");
        }
    }
}
