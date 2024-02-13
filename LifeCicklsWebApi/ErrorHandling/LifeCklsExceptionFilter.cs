using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LifeCicklsWebApi.ErrorHandling
{
    public class LifeCklsExceptionFilter : IExceptionFilter
    {
        private readonly IHostEnvironment _hostEnvironment;

        public LifeCklsExceptionFilter(IHostEnvironment hostEnvironment) =>
            _hostEnvironment = hostEnvironment;

        public void OnException(ExceptionContext context)
        {
            // Create a custom error response
            var result = new ObjectResult(new
            {
                StatusCode = 500,
                Message = "An internal server error occurred.",
                ExceptionMessage = context.Exception.ToString(),
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };

            // Set the result in the context
            context.Result = result;

            // Mark the exception as handled
            context.ExceptionHandled = true;
        }
    }
}
