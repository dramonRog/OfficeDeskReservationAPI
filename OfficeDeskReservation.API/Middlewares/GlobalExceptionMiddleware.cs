using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace OfficeDeskReservation.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Internal Server Error";
            
            switch (exception)
            {
                case KeyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    title = "Not Found";
                    break;
                case ArgumentException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    title = "Bad Request";
                    break;
                case InvalidOperationException:
                    statusCode = (int)HttpStatusCode.Conflict;
                    title = "Conflict";
                    break;
            }

            context.Response.StatusCode = statusCode;

            string detail = statusCode == (int)HttpStatusCode.InternalServerError
                ? "An unexpected error occurred. Please try again later."
                : exception.Message;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            };

            return context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
