using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Overt.Core.Grpc.H2
{
    /// <summary>
    /// 客户端工厂
    /// </summary>
    public class GrpcClientFactory<T> : IGrpcClientFactory<T> where T : ClientBase
    {
        readonly GrpcClientOptions<T> _options;
        readonly GrpcChannel _channel;
        readonly T _client;

        public GrpcClientFactory(IOptions<GrpcClientOptions<T>> options = null)
        {
            _options = options?.Value ?? new GrpcClientOptions<T>();
            _options.ConfigPath = GetConfigPath(_options.ConfigPath);
            _channel = BuildChannel();
            _client= (T)Activator.CreateInstance(typeof(T), _channel);
        }

        /// <summary>
        /// 构造实例
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

        #region Private Method
        /// <summary>
        /// 构建可重用的GrpcChannel 通道
        /// </summary>
        /// <returns></returns>
        private GrpcChannel BuildChannel()
        {
            _options.GrpcChannelOptions ??= Constants.DefaultChannelOptions;
            _options.GrpcChannelOptions.ServiceConfig ??= Constants.DefaultServiceConfig;
            return GrpcChannel.ForAddress($"{typeof(T).Name}:///localhost", _options.GrpcChannelOptions);
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