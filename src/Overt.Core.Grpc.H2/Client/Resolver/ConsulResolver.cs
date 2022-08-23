using Consul;
using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Overt.Core.Grpc.H2
{
    /// <summary>
    /// consul 地址解析器
    /// </summary>
    public class ConsulResolver<T> : PollingResolver where T : ClientBase
    {
        public readonly static ConcurrentDictionary<string,List<AddressWrapper>> _currentAddressWrapper = new ConcurrentDictionary<string, List<AddressWrapper>>();

        Timer _timer;
        readonly ILoggerFactory _loggerFactory;
        readonly GrpcClientOptions<T> _options;


        public ConsulResolver(GrpcClientOptions<T> options, ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _options= options; 
        }

        protected override void OnStarted()
        {
            base.OnStarted();

            _timer = new Timer(OnTimerCallback, null, ClientTimespan.ResetInterval, ClientTimespan.ResetInterval);
        }

        private void OnTimerCallback(object state)
        {
            try
            {
                //Refresh();
            }
            catch (Exception ex)
            {
                _loggerFactory.CreateLogger("resolver").LogError(ex, $"OnTimerCallback serviceName:{_options?.ServiceName ?? ""}");
            }
        }


        protected override async Task ResolveAsync(CancellationToken cancellationToken)
        {
            var exitus = StrategyFactory.Get<T>(_options);
            if (exitus?.EndpointDiscovery == null)
                throw new ArgumentNullException("No Service Discovery Policy Obtained");

            var endPoints = await exitus.EndpointDiscovery.FindServiceEndpointsAsync();
            if (IsDifference(endPoints))
            {
                var addresses = endPoints.Select(s => s.Address).ToArray();
                Listener(ResolverResult.ForResult(addresses));

                Reset(endPoints);//重置缓存
            }
        }

        #region 私有方法
        /// <summary>
        /// 获得当前缓存的节点信息
        /// </summary>
        /// <returns></returns>
        private List<AddressWrapper> Get()
        {
            try
            {
                _currentAddressWrapper.TryGetValue(_options.ServiceName, out List<AddressWrapper> value);
                return value;
            }
            catch
            {
                return default(List<AddressWrapper>);
            }
        }

        private void Reset(List<AddressWrapper> list)
        {
            try
            {
                _currentAddressWrapper.TryRemove(_options.ServiceName, out _);
                _currentAddressWrapper.TryAdd(_options.ServiceName, list);
            }
            catch
            {

            }
        }

        private bool IsDifference(List<AddressWrapper> wrappers)
        {
            try
            {
                var currentValues = Get();
                var count = currentValues?.Count ?? 0;
                var wrappersCount = wrappers?.Count ?? 0;
                if (count <= 0)
                {
                    return true;
                }
                if (count != wrappersCount)
                {
                    return true;
                }
                return !currentValues.All(b => wrappers.Any(a => a.Target.Equals(b.Target)));
            }
            catch
            {
                return true;
            }
        }
        #endregion
    }
}
