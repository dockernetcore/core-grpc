using Com.Ctrip.Framework.Apollo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Overt.Core.Grpc;
using Overt.GrpcExample.Client.Tracer;
using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Grpc.Core;
using System.Threading.Tasks;
using static Overt.GrpcExample.Service.Grpc.GrpcExampleService;

namespace Overt.GrpcExample.Client
{
    class Program
    {
        static IServiceProvider provider;
        private static readonly ConcurrentDictionary<string, Channel> _channels = new ConcurrentDictionary<string, Channel>();

        static Program()
        {

            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(AppContext.BaseDirectory)
            //    .AddJsonFile("appsettings.json", true);

            //var configuration = builder.Build();

          
            //var taskWall = new List<System.Threading.Tasks.Task>();
            //for (var i = 0; i < 1; i++)
            //{
            //    taskWall.Add(Task.Run(() =>
            //    {
            //        System.Console.WriteLine("create");
            //        ChannelTest();
            //    }));
            //}
            //System.Threading.Tasks.Task.WaitAll(taskWall.ToArray());
            //System.Console.ReadLine();

            // var services = new ServiceCollection();
            // services.AddSingleton<IConfiguration>(configuration);
            // services.AddOptions();

            // // 注入GrpcClient
            // services.AddGrpcClient();
            // services.Configure<GrpcClientOptions>(options =>
            // {
            //     options.Tracer = new ConsoleTracer();
            //     options.Interceptors.Add(new ClientLoggerInterceptor());
            // });
            //// 第三方配置，启动可用
            // services.AddGrpcConfig(config =>
            // {
            //     config.AddApollo(configuration.GetSection("apollo")).AddDefault();
            // });
            // services.Configure<GrpcClientOptions<GrpcExampleServiceClient>>(cfg =>
            // {
            //     cfg.ConfigPath = "";
            // });

            // provider = services.BuildServiceProvider();
        
        }

        static async Task Main(string[] args)
        {
            //await Task.Run(() =>
            //{
            //    System.Console.WriteLine("create");
            //    ChannelTest();
            //});
            var taskWall = new List<System.Threading.Tasks.Task>();
            for (var i = 0; i < 10; i++)
            {
                taskWall.Add(Task.Run(() =>
                {
                    System.Console.WriteLine("create");
                    ChannelTest(i.ToString());
                }));
            }
            System.Threading.Tasks.Task.WaitAll(taskWall.ToArray());

            System.Console.ReadLine();

            //do
            //{
            //    var key = Console.ReadKey();
            //    if (key.Key == ConsoleKey.A)
            //        break;

            //    try
            //    {
            //        var service = provider.GetService<IGrpcClient<Service.Grpc.GrpcExampleService.GrpcExampleServiceClient>>();
            //        var client = service.CreateClient((invokers) =>
            //        {
            //            return invokers[0];
            //        });
            //        var res = client.Ask(new Service.Grpc.AskRequest() { Key = "abc" });
            //        Console.WriteLine(DateTime.Now + " - " + res.Content ?? "abc");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"{DateTime.Now} - 异常");
            //    }

            //    Thread.Sleep(200);

            //} while (true);

            //Console.WriteLine("over");
            //Console.ReadLine();
        }

        static void ChannelTest(string suffix)
        {


            System.Console.WriteLine("开始");

            for (var i = 0; i < 100000; i++)
            {
                try
                {
                    System.Console.WriteLine($"开始第{i}次执行");
                    var channel = new Channel("172.20.40.97:10004", ChannelCredentials.Insecure, Constants.DefaultChannelOptions);
                    _channels.AddOrUpdate(suffix+i.ToString(), channel, (key, value) => channel);
                    //System.Threading.Thread.Sleep(1000);
                    Channel failedChannel;
                    _channels.TryRemove(suffix+i.ToString(), out failedChannel);
                    failedChannel?.ShutdownAsync();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("异常");

                }

            }
            System.Console.WriteLine("结束");
        }
    }
}
