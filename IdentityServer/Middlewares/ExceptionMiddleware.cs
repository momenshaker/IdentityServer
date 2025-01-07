using App.Core.Domain.Result;
using System.Net;

namespace IdenitityServer.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex.Message}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }


        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var result = new Result<string>("")
            {
                ErrorMessages = new List<string>() { exception.Message }
            };
            result.StatusCode = result.GetStatus(context.Response.StatusCode);

            return context.Response.WriteAsync(result.ToString());



        }
    }
}
