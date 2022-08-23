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
        private readonly static ConcurrentDictionary<string, Exitus> _exitusMap = new ConcurrentDictionary<string, Exitus>();
        private readonly static ConcurrentDictionary<string, GrpcClientOptions> _options = new ConcurrentDictionary<string, GrpcClientOptions>();
        
        /// <summary>
        /// 加入缓存
        /// </summary>
        /// <param name="options"></param>
        public static void AddCache(GrpcClientOptions options)
        {
            if (options == null)
                return;

            _options.AddOrUpdate(options.ServiceName, options, (k, v) => options);
        }

        /// <summary>
        /// 获取EndpointStrategy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Exitus Get(string serviceName)
        {
            if (_exitusMap.TryGetValue(serviceName, out Exitus exitus) &&
                exitus?.EndpointDiscovery != null)
                return exitus;

            lock (_lockHelper)
            {
                if (_exitusMap.TryGetValue(serviceName, out exitus) &&
                    exitus?.EndpointDiscovery != null)
                    return exitus;

                if (!_options.TryGetValue(serviceName, out GrpcClientOptions options))
                    throw new Exception($"配置异常");

                exitus = ResolveConfiguration(options);
                _exitusMap.AddOrUpdate(serviceName, exitus, (k, v) => exitus);
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
            if (string.IsNullOrWhiteSpace(options.ServiceName))
                options.ServiceName = service?.Name ?? "";
            if (string.IsNullOrWhiteSpace(options.ServiceName))
                throw new ArgumentException("ServiceName is null");

            IEndpointDiscovery endpointDiscovery;
            if (EnableConsul(service?.Discovery ?? null, out string address))
                endpointDiscovery = ResolveConsulConfiguration(address, options);
            else
                endpointDiscovery = ResolveEndpointConfiguration(service, options);
            return new Exitus(options.ServiceName, endpointDiscovery);
        }

        /// <summary>
        /// 解析服务配置
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        public static GrpcServiceElement ResolveServiceConfiguration(string configFile)
        {
            var grpcSection = ConfigBuilder.BuildClient<GrpcClientSection>(Constants.GrpcClientSectionName, configFile);
            return grpcSection?.Service ?? default(GrpcServiceElement);
        }

        public static string ResolveServiceName(string configFile,string serviceName)
        {
            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                return serviceName;
            }
            var consulServiceConfig = ResolveServiceConfiguration(configFile);
            serviceName = consulServiceConfig?.Name ?? "";
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentNullException("service is not null");
            }
            return serviceName;
        }

        /// <summary>
        /// 解析Consul配置
        /// </summary>
        /// <param name="address"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static IEndpointDiscovery ResolveConsulConfiguration(string address, GrpcClientOptions options)
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
        
            var consulSection = ConfigBuilder.BuildClient<ConsulServerSection>(Constants.ConsulServerSectionName, configPath);
            if (string.IsNullOrWhiteSpace(consulSection?.Service?.Address) && (discovery?.EndPoints?.Count ?? 0) <= 0)
            {
                throw new Exception($"when resolve configpath, configpath file is not exist... [{configPath}] or No service discovery address configured [key ->ConsulServer:Service:Address]");
            }
            if (string.IsNullOrWhiteSpace(consulSection?.Service?.Address))
                return false;

            address = consulSection?.Service?.Address;
            return true;
        }
        #endregion
    }
}
