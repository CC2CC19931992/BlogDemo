using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDemo.Api.Extensions
{
    public static class ExceptionHandlingExtensions
    {
        public static void UseMyExceptionHandler(this IApplicationBuilder app,ILoggerFactory loggerFactory)//加了this表示扩展方法
        {
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context => {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";//关于错误信息返回的问题，之后再考虑，当前先考虑未返回json，以后可以返回一个对象

                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        var logger = loggerFactory.CreateLogger("ExceptionHandlingExtensions");
                        logger.LogError(500,ex.Error,ex.Error.Message);
                    }

                    await context.Response.WriteAsync(ex?.Error?.Message ?? "An Error Occuped");
                });
            });
        }
    }
}
