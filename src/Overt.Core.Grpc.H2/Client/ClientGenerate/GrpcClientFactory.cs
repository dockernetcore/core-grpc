using Grpc.Core;
using Grpc.Net.Client;
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

        public GrpcClientFactory(IOptions<GrpcClientOptions<T>> options = null)
        {
            _options = options?.Value ?? new GrpcClientOptions<T>();
            _options.ConfigPath = GetConfigPath(_options.ConfigPath);
            _channel = BuildChannel();
        }

        /// <summary>
        /// 构造实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get()
        {
            return (T)Activator.CreateInstance(typeof(T), _channel);
        }

        #region Private Method
        private GrpcChannel BuildChannel()
        {
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