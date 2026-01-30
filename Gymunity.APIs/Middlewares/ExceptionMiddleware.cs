using Gymunity.APIs.Responses;
using Gymunity.APIs.Responses.Errors;

namespace Gymunity.APIs.Middlewares
{
    /// <summary>
    /// Middleware to handle global exceptions and return consistent API responses.
    /// </summary>
    /// <param name="next">The next request delegate in the pipeline.</param>
    /// <param name="logger">The logger for capturing error details.</param>
    /// <param name="env">The hosting environment to check for Development/Production.</param>
    public class ExceptionMiddleware(RequestDelegate? next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        private readonly ILogger<ExceptionMiddleware> _logger = logger;
        private readonly IWebHostEnvironment _env = env;

        /// <summary>
        /// Invokes the middleware to process the request.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                // Fix: Ensure StackTrace is never null by using the null-coalescing operator ??
                var response = _env.IsDevelopment()
                    ? new ApiExceptionResponse(StatusCodes.Status500InternalServerError, ex.Message, ex.StackTrace ?? "No Stack Trace available")
                    : new ApiResponse(StatusCodes.Status500InternalServerError);

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}