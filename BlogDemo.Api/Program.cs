using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BlogDemo.Infrastructure.Database;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace BlogDemo.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //配置Serilog
            Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Debug()//最小日志输出级别 Debug
              //
              .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
              .Enrich.FromLogContext()
              .WriteTo.Console()//输出到控制台
              //输出到文件。每天创建一个
              .WriteTo.File(Path.Combine("logs", @"log.txt"), rollingInterval: RollingInterval.Day)
              .CreateLogger();
            var host = CreateWebHostBuilder(args).Build();
            using(var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    var myContext = services.GetRequiredService<MyContext>();//MyContext在CreateWebHostBuilder的UseStartup这步就已注入到了容器了 所以这里能够这样使用
                    MyContextSeed.SeedAsync(myContext, loggerFactory).Wait();
                }
                catch(Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "Error occured seeding the Database");
                    //Console.WriteLine(ex);
                    //throw;
                }
            }
            host.Run();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)//源码里有对日志进行配置初始化
            //下面的这个UserStartup是选择用哪个环境来启动。
            //typeof(Startup).GetTypeInfo().Assembly.FullName是获得程序集的全称“BlogDemo.Api, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null”
            //然后根据launchSettings.json里配置的环境变量去找对应的Startup的。现在我的launchSetting里配置的是Production,则进StartupProduction.cs这个类
            //里进行一些组件或配置的初始化
            .UseStartup(typeof(Startup).GetTypeInfo().Assembly.FullName)
            .UseSerilog();//使用Serilog,这里使用了Serilog后会覆盖CreateDefaultBuilder里的日志配置
                          //.UseStartup<Startup>();
    }
}
