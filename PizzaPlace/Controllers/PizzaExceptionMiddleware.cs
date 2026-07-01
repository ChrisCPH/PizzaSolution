using Microsoft.Extensions.Logging;

namespace PizzaPlace.Middleware
{
    public class PizzaExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PizzaExceptionMiddleware> _logger;

        public PizzaExceptionMiddleware(RequestDelegate next, ILogger<PizzaExceptionMiddleware> logger)
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
            catch (PizzaException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (NotSupportedException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing request {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
            }
        }
    }
}