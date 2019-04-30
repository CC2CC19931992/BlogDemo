using AutoMapper;
using BlogDemo.Api.Extensions;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.Database;
using BlogDemo.Infrastructure.Repositoies;
using BlogDemo.Infrastructure.Resources;
using BlogDemo.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDemo.Api
{
    public class StartupDevelopment
    {
        public static IConfiguration Configuration { get; set; }

        public StartupDevelopment(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            //注册mvc中间件
            services.AddMvc(
                options => {
                    options.ReturnHttpNotAcceptable = true;//设置为true表示只能返回指定格式的内容，否则就会报406的错误
                    options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());//新增输出的格式（指定返回的格式类型为xml）
                })
                .AddJsonOptions(options=> {
                    //所有的返回实体输出格式为前端规范的首字母小写的CamelCase规范
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                })
                ;

            //将自定义的MyContext注册到容器里，使用的时候在使用的类里面进行依赖注入即可
            services.AddDbContext<MyContext>(options =>
            {
                //var connectionString = Configuration["ConnectionStrings:DefaultConnection"];//使用此方法获取配置信息
                var connectionString = Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlite(connectionString);//这里的连接串一定要写正确 不然使用程序包管理控制器的命令进行迁移的时候会报错
            });

            //注册HttpsRedirection中间件
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 5001;
            });
            //注册PostRepository,类似于这样的在.NET FrameWork里是在config文件里写，然后unity组件来进行读取，效果和这里类似
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddAutoMapper();//注册AutoMapper
            services.AddTransient<IValidator<PostResource>, PostResourceValidator>();//注册

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();//注册IActionContextAccessor
            //注册IUrlHelper
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });

            //注册属性映射容器
            var propertyMappingContainer = new PropertyMappingContainer();
            propertyMappingContainer.Register<PostPropertyMapping>();
            services.AddSingleton<IPropertyMappingContainer>(propertyMappingContainer);

            services.AddTransient<ITypeHelperService, TypeHelperService>();//注册TypeHelperService服务
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env,ILoggerFactory loggerFactory)
        {
            //由于这里是直接返回一个错误页面，这个对于mvc是用的，但是对于webApi显然是不友好的，因而需要重新定义个错误下面的
            //先注释
            //app.UseDeveloperExceptionPage();

            //下面这段写完后，发现可将这段放到拓展方法里去
            //app.UseExceptionHandler(builder=>
            //{
            //    builder.Run(async context => {
            //        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            //        context.Response.ContentType = "application/json";
            //        var ex = context.Features.Get<IExceptionHandlerFeature>();
            //        if (ex != null)
            //        {

            //        }
            //        await context.Response.WriteAsync(ex?.Error?.Message ?? "An Error Occuped");
            //    });
            //});

            app.UseMyExceptionHandler(loggerFactory);

            //使用https重定向中间件
            //重定向后，当访问http://localhost:5000/api/values时,
            //会默认跳到https://localhost:5001/api/values,这便是重定向的作用
            app.UseHttpsRedirection();
            app.UseMvc();//使用mvc中间件
        }
    }
}
