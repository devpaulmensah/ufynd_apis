using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ufynd.Arrivals.Api.Middlewares;

public static class GlobalExceptionHandler
{
    public static void ConfigureGlobalHandler(this IApplicationBuilder app, ILogger logger)
    {
        app.UseExceptionHandler(error =>
        {
            error.Run(async context =>
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionHandlerFeature is not null)
                {
                    var baseException = exceptionHandlerFeature.Error;
                    logger.LogError(baseException, "Something went wrong");

                    var response = new
                    {
                        status = "Something bad happened, try again later"
                    };

                    context.Response.ContentLength = JsonConvert.SerializeObject(response).Length;
                    await context.Response.WriteAsJsonAsync(response);
                }
            });
        });
    }
}