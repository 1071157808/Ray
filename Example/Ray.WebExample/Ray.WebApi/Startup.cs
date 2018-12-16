using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Ray.Core;
using Ray.Core.Client;
using Ray.Core.Messaging;
using Ray.Handler;
using Ray.IGrains;
using Ray.IGrains.Actors;
using Ray.RabbitMQ;
using System;
using System.Threading.Tasks;

namespace Ray.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            var client = StartClientWithRetries().GetAwaiter().GetResult();
            var handlerStartup = client.ServiceProvider.GetService<HandlerStartup>();
            Task.WhenAll(
                    handlerStartup.Start(SubscriberGroup.Core),
                    handlerStartup.Start(SubscriberGroup.Db),
                    handlerStartup.Start(SubscriberGroup.Rep))
                .GetAwaiter().GetResult();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddJwtEx(Configuration);
            services.AddMvc();//MVC API
            services.AddSwaggerEx();
            services.AddSingleton<IClientFactory, ClientFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwaggerEx();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
        //
        private static async Task<IClusterClient> StartClientWithRetries(int initializeAttemptsBeforeFailing = 5)
        {
            int attempt = 0;
            IClusterClient client;
            while (true)
            {
                try
                {
                    client = await ClientFactory.Build(() =>
                    {
                        var builder = new ClientBuilder()
                        .UseLocalhostClustering()
                            .ConfigureServices(services =>
                            {
                                services.AddMQHandler();//注册所有handler
                                services.AddRay();//注册Client获取方法
                                services.AddSingleton<ISerializer, ProtobufSerializer>();//注册序列化组件
                                services.AddRabbitMQ();//注册RabbitMq为默认消息队列
                                services.AddLogging(logging => logging.AddConsole());
                                services.PostConfigure<RabbitConfig>(c =>
                                {
                                    c.UserName = "admin";
                                    c.Password = "admin";
                                    c.Hosts = new[] { "127.0.0.1:5672" };
                                    c.MaxPoolSize = 100;
                                    c.VirtualHost = "/";
                                });
                            })
                        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IAccount).Assembly).WithReferences())
                        .ConfigureLogging(logging => logging.AddConsole());
                        return builder;
                    });
                    Console.WriteLine("Client successfully connect to silo host");
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;
                    Console.WriteLine($"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
                    if (attempt > initializeAttemptsBeforeFailing)
                    {
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(4));
                }
            }
            return client;
        }
    }

}
