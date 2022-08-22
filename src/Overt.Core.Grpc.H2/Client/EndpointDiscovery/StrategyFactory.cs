using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace Overt.Core.Grpc.H2
{
    /// <summary>
    /// Endpoint 策略工厂
    /// </summary>
    internal class StrategyFactory 
    {
        private readonly static object _lockHelper = new object();
        private readonly static ConcurrentDictionary<Type, Exitus> _exitusMap = new ConcurrentDictionary<Type, Exitus>();

        /// <summary>
        /// 获取EndpointStrategy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Exitus Get<T>(GrpcClientOptions options) where T : ClientBase
        {
            if (_exitusMap.TryGetValue(typeof(T), out Exitus exitus) &&
                exitus?.EndpointDiscovery != null)
                return exitus;

            lock (_lockHelper)
            {
                if (_exitusMap.TryGetValue(typeof(T), out exitus) &&
                    exitus?.EndpointDiscovery != null)
                    return exitus;

                exitus = ResolveConfiguration(options);
                _exitusMap.AddOrUpdate(typeof(T), exitus, (k, v) => exitus);
                return exitus;
            }
        }

        #region Private Method
        /// <summary>
        /// 解析配置文件
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        private static Exitus ResolveConfiguration(GrpcClientOptions options)
        {
            var service = ResolveServiceConfiguration(options.ConfigPath);
            if(string.IsNullOrWhiteSpace(options.ServiceName))
                options.ServiceName = service.Name;

            IEndpointDiscovery endpointDiscovery;
            if (EnableConsul(service.Discovery, out string address))
                endpointDiscovery = ResolveStickyConfiguration(address, options);
            else
                endpointDiscovery = ResolveEndpointConfiguration(service, options);
            return new Exitus(options.ServiceName, endpointDiscovery);
        }

        /// <summary>
        /// 解析服务配置
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        private static GrpcServiceElement ResolveServiceConfiguration(string configFile)
        {
            var grpcSection = ConfigBuilder.Build<GrpcClientSection>(Constants.GrpcClientSectionName, configFile);
            if (grpcSection == null || grpcSection.Service == null)
                throw new ArgumentNullException($"service config error");

            return grpcSection.Service;
        }

        /// <summary>
        /// 解析Consul配置
        /// </summary>
        /// <param name="address"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static IEndpointDiscovery ResolveStickyConfiguration(string address, GrpcClientOptions options)
        {
            var consulEndpointDiscovery = new ConsulEndpointDiscovery(options, address);
            return consulEndpointDiscovery;
        }

        /// <summary>
        /// 解析Endpoint配置
        /// </summary>
        /// <param name="service"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static IEndpointDiscovery ResolveEndpointConfiguration(GrpcServiceElement service, GrpcClientOptions options)
        {
            var ipEndPoints = service.Discovery.EndPoints.Select(oo => Tuple.Create(oo.Host, oo.Port)).ToList();
            return new IPEndpointDiscovery(options, ipEndPoints);
        }

        /// <summary>
        /// 是否是使用Consul
        /// </summary>
        /// <param name="discovery"></param>
        /// <returns></returns>
        private static bool EnableConsul(GrpcDiscoveryElement discovery, out string address)
        {
            address = string.Empty;
            var configPath = discovery?.Consul?.Path;
            if (string.IsNullOrWhiteSpace(configPath))
                return false;

            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, discovery.Consul.Path)))
                throw new Exception($"[{discovery.Consul.Path}] not exist at [{AppDomain.CurrentDomain.BaseDirectory}]");

            var consulSection = ConfigBuilder.Build<ConsulServerSection>(Constants.ConsulServerSectionName, configPath);
            if (string.IsNullOrWhiteSpace(consulSection?.Service?.Address))
                return false;

            address = consulSection?.Service?.Address;
            return true;
        }
        #endregion
    }
}
