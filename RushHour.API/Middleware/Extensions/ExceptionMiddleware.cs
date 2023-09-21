using Microsoft.AspNetCore.Http;
using RushHour.Domain.Exceptions;
using RushHour.Domain.Middleware.Models;
using System.Net;

namespace RushHour.API.Middleware.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (ValidationException ex)
            {
                var code = HttpStatusCode.BadRequest;
                string message = ex.Message;
                await HandleExceptionAsync(httpContext, ex, code, message);
            }
            catch (NotFoundException ex)
            {
                var code = HttpStatusCode.NotFound;
                string message = ex.Message;
                await HandleExceptionAsync(httpContext, ex, code, message);
            }
            catch (UnauthorizedException ex)
            {
                var code = HttpStatusCode.Forbidden;
                string message = ex.Message;
                await HandleExceptionAsync(httpContext, ex, code, message);
            }
            catch (Exception ex)
            {
                var code = HttpStatusCode.InternalServerError;
                string message = "Internal Server Error from the custom middleware.";
                await HandleExceptionAsync(httpContext, ex, code, message);
            }

        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode code, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            await context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = message
            }.ToString());

        }
    }
}
