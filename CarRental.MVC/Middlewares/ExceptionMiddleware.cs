using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using CarRental.MVC.Models;
using CarRental.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using CarRental.MVC.Models; // Add this using statement

namespace CarRental.MVC.Middlewares
{
    /// <summary>
    /// Global exception handling middleware for MVC applications with Razor views.
    /// Supports returning HTML views for normal requests and JSON for AJAX/API requests.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IHostApplicationLifetime _lifetime;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IWebHostEnvironment environment,
            IHostApplicationLifetime lifetime)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        }

        /// <summary>
        /// Invokes the middleware to process the incoming HTTP request and catch any unhandled exceptions,
        /// ensuring they are logged and appropriately handled before sending a response to the client.
        /// </summary>
        /// <param name="context">The HttpContext for the current request, providing access to the HTTP request and response.</param>
        /// <returns>A Task representing the completion of middleware execution for the current HTTP request.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles an exception encountered during the HTTP request processing and sends an appropriate response
        /// based on the request type (normal or AJAX/API).
        /// </summary>
        /// <param name="context">The HttpContext for the current request, which contains information about the HTTP request and response.</param>
        /// <param name="exception">The exception that was thrown during request processing.</param>
        /// <returns>A Task that represents the asynchronous operation of handling the exception.</returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
                return;

            bool isAjax = IsAjaxRequest(context.Request) || RequestExpectsJson(context.Request);

            if (isAjax)
            {
                // Send JSON response for AJAX/API calls
                var response = BuildErrorResponse(context, exception);
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = _environment.IsDevelopment(),
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = response.StatusCode;
                await context.Response.WriteAsync(JsonSerializer.Serialize(response.Body, jsonOptions));
            }
            else
            {
                // Redirect to an error page for normal MVC requests
                context.Response.Clear();
                context.Response.StatusCode = GetHttpStatusCode(exception);

                var result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Error.cshtml",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                        new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                        new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                    {
                        Model = new ErrorViewModel // Now using CarRental.MVC.Models.ErrorViewModel
                        {
                            RequestId = context.TraceIdentifier,
                            Message = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.",
                            Details = _environment.IsDevelopment() ? exception.ToString() : null
                        }
                    }
                };

                var actionContext = new ActionContext(context, new Microsoft.AspNetCore.Routing.RouteData(), new ControllerActionDescriptor());
                await result.ExecuteResultAsync(actionContext);
            }
        }

        /// <summary>
        /// Builds an error response based on the provided exception and current HTTP context.
        /// Returns an appropriate HTTP status code and a response object detailing the error.
        /// </summary>
        /// <param name="context">The HttpContext for the current request, which contains details about the HTTP request, response, and TraceIdentifier.</param>
        /// <param name="exception">The exception that occurred during request processing, used to determine the response content and status code.</param>
        /// <returns>A tuple containing the HTTP status code and the response body object with error details.</returns>
        private (int StatusCode, object Body) BuildErrorResponse(HttpContext context, Exception exception)
        {
            var response = new ApiResponse<object>
            {
                Success = false,
                Timestamp = DateTime.UtcNow,
                TraceId = context.TraceIdentifier
            };

            switch (exception)
            {
                case ValidationException validationEx:
                    response.Message = "Validation failed";
                    response.Errors = new List<string> { validationEx.Message };
                    return ((int)HttpStatusCode.BadRequest, response);

                case KeyNotFoundException:
                    response.Message = "Resource not found";
                    response.Errors = new List<string> { exception.Message };
                    return ((int)HttpStatusCode.NotFound, response);

                case UnauthorizedAccessException:
                    response.Message = "Unauthorized access";
                    response.Errors = new List<string> { "You do not have permission to access this resource" };
                    return ((int)HttpStatusCode.Unauthorized, response);

                case BusinessLogicException businessEx:
                    response.Message = "Business logic error";
                    response.Errors = new List<string> { businessEx.Message };
                    return ((int)HttpStatusCode.BadRequest, response);

                default:
                    response.Message = "An internal server error occurred";
                    if (_environment.IsDevelopment())
                    {
                        response.Errors = new List<string> { exception.Message, exception.StackTrace ?? "No stack trace" };
                        response.Data = new
                        {
                            Type = exception.GetType().Name,
                            Source = exception.Source,
                            Inner = exception.InnerException?.Message
                        };
                    }
                    else
                    {
                        response.Errors = new List<string> { "An unexpected error occurred. Please contact support." };
                    }

                    return ((int)HttpStatusCode.InternalServerError, response);
            }
        }

        /// <summary>
        /// Determines the appropriate HTTP status code for a given exception.
        /// </summary>
        /// <param name="ex">The exception for which to determine the corresponding HTTP status code.</param>
        /// <returns>An integer representing the HTTP status code that corresponds to the provided exception.</returns>
        private static int GetHttpStatusCode(Exception ex) =>
            ex switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                BusinessLogicException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

        /// <summary>
        /// Determines whether the specified HTTP request is an AJAX request by inspecting the "X-Requested-With" header.
        /// </summary>
        /// <param name="request">The HTTP request to check for the presence of the "X-Requested-With" header.</param>
        /// <returns>True if the request is an AJAX request; otherwise, false.</returns>
        private static bool IsAjaxRequest(HttpRequest request) =>
            request.Headers["X-Requested-With"] == "XMLHttpRequest";

        /// <summary>
        /// Determines whether the HTTP request expects a JSON response based on the "Accept" header.
        /// </summary>
        /// <param name="request">The HTTP request to evaluate, containing headers and other metadata.</param>
        /// <returns>True if the "Accept" header includes "application/json", otherwise false.</returns>
        private static bool RequestExpectsJson(HttpRequest request) =>
            request.GetTypedHeaders().Accept.Any(h => h.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase));
    }

    public class BusinessLogicException : Exception
    {
        public BusinessLogicException(string message) : base(message)
        {
        }

        public BusinessLogicException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}