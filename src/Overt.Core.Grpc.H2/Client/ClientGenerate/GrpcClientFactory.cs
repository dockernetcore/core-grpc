using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Overt.Core.Grpc.H2
{
    /// <summary>
    /// 客户端工厂
    /// </summary>
    public class GrpcClientFactory<T> : IGrpcClientFactory<T> where T : ClientBase
    {
        readonly GrpcClientOptions<T> _options;
        readonly GrpcChannel _channel;
        readonly IServiceProvider _serviceProvider;
        T _client;


        public GrpcClientFactory(IOptions<GrpcClientOptions<T>> options, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _options = options?.Value ?? new GrpcClientOptions<T>();
            _options.ConfigPath = GetConfigPath(_options.ConfigPath);

            _channel = BuildChannel();
        }

        /// <summary>
        /// 构造实例 单例即可
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get()
        {
            if (_client == null)
            {
                return (T)Activator.CreateInstance(typeof(T), _channel);
            }
            return _client;
        }

        /// <summary>
        /// 预热链接,预热负载均衡数据,避免不预热客户端起来流量太大Consul 服务发现还在处理导致慢
        /// </summary>
        public void WarmConnect()
        {
            _channel.ConnectAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter();
        }

        #region Private Method
        /// <summary>
        /// 构建可重用的GrpcChannel 通道
        /// </summary>
        /// <returns></returns>
        private GrpcChannel BuildChannel()
        {
            _options.GrpcChannelOptions ??= Constants.DefaultChannelOptions();
            _options.GrpcChannelOptions.ServiceConfig ??= Constants.DefaultServiceConfig();
            _options.GrpcChannelOptions.ServiceProvider = _serviceProvider;

            _options.ServiceName = StrategyFactory.ResolveServiceName(_options.ConfigPath, _options.ServiceName);

            StrategyFactory.AddCache(_options);
            var exitus = StrategyFactory.Get(_options.ServiceName);
            if (exitus?.EndpointDiscovery == null)
                throw new ArgumentNullException("No Service Discovery Policy Obtained");

            return GrpcChannel.ForAddress($"consul:///{_options.ServiceName}", _options.GrpcChannelOptions);
        }

        /// <summary>
        /// 获取命名空间
        /// </summary>
        /// <returns></returns>
        private string GetConfigPath(string configPath)
        {
            if (string.IsNullOrWhiteSpace(configPath))
                configPath = $"dllconfigs/{typeof(T).Namespace}.dll.json";

            return configPath;
        }
        #endregion
    }
}