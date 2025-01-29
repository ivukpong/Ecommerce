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
               var response = new
               {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred. Please try again later."
               };

               var jsonResponse = JsonSerializer.Serialize(response);
               context.Response.ContentType = "application/json";
               context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

               return context.Response.WriteAsync(jsonResponse);
          }
     }
}
