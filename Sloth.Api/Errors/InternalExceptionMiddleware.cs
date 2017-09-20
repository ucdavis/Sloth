using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Sloth.Api.Logging;

namespace Sloth.Api.Errors
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class InternalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public InternalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                if (httpContext.Response.HasStarted)
                {
                    throw;
                }

                httpContext.Response.Clear();
                httpContext.Response.StatusCode = 500;
                httpContext.Response.ContentType = "application/json";

                var response = JsonConvert.SerializeObject(new
                {
                    correlationId = httpContext.Items[CorrelationIdMiddleware.HeaderKey],
                    message = ex.Message
                });
                await httpContext.Response.WriteAsync(response);
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class InternalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseInternalExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<InternalExceptionMiddleware>();
        }
    }
}
