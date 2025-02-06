using System.Net;
using System.Text.Json;
using Serilog;

namespace Ecommerce.API.Middlewares
{
     public class ExceptionHandlingMiddleware
     {
          private readonly RequestDelegate _next;

          public ExceptionHandlingMiddleware(RequestDelegate next)
          {
               _next = next;
          }

          public async Task Invoke(HttpContext context)
          {
               try
               {
                    await _next(context);
               }
               catch (Exception ex)
               {
                    Log.Error(ex, "Unhandled exception: {Message}", ex.Message);
                    await HandleExceptionAsync(context, ex);
               }
          }

          private static Task HandleExceptionAsync(HttpContext context, Exception exception)
          {
               HttpStatusCode statusCode;
               string message;

               switch (exception)
               {
                    case ArgumentException:
                         statusCode = HttpStatusCode.BadRequest;
                         message = "Invalid request. Please check your input.";
                         break;
                    case UnauthorizedAccessException:
                         statusCode = HttpStatusCode.Unauthorized;
                         message = "You are not authorized to perform this action.";
                         break;
                    case NotImplementedException:
                         statusCode = HttpStatusCode.NotImplemented;
                         message = "This feature is not implemented.";
                         break;
                    default:
                         statusCode = HttpStatusCode.InternalServerError;
                         message = "An unexpected error occurred. Please try again later.";
                         break;
               }

               var response = new
               {
                    StatusCode = (int)statusCode,
                    Message = message
               };

               var jsonResponse = JsonSerializer.Serialize(response);
               context.Response.ContentType = "application/json";
               context.Response.StatusCode = (int)statusCode;

               return context.Response.WriteAsync(jsonResponse);
          }
     }
}
