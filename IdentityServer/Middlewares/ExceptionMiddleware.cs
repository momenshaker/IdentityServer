using App.Core.Result;
using System.Net;

namespace IdentityServer.Middlewares;

/// <summary>
/// Middleware to handle exceptions globally in the application.
/// Logs the error and sends a structured error response to the client.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger for logging exceptions.</param>
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the next middleware in the pipeline. If an exception occurs, it will be caught and handled.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            // Proceed to the next middleware in the pipeline
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            // Log the exception details
            _logger.LogError($"Something went wrong: {ex.Message}, Stack Trace: {ex.StackTrace}");

            // Handle the exception and send a structured response to the client
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    /// <summary>
    /// Handles the exception by setting the response status code and returning a structured error response.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="exception">The exception that occurred.</param>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Set the response status code
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        // Create a structured error response using the Result class
        var result = new Result<string>("")
        {
            ErrorMessages = new List<string> { exception.Message }, // Fixed the issue with list initialization
            StatusCode = new Result<string>("").GetStatus(context.Response.StatusCode)
        };

        // Return the error response as JSON
        return context.Response.WriteAsync(result.ToString()!);
    }
}
