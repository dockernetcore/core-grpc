using Consul;
using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Microsoft.Extensions.Logging;
using System;
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
        private Timer _timer;
        ILoggerFactory _loggerFactory;
        readonly GrpcClientOptions<T> _options;


        public ConsulResolver(GrpcClientOptions<T> options, ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _options= options; 
            var serviceName = options?.ServiceName ?? "";
            if (serviceName == null || serviceName.Length <= 0)
            {
                throw new ArgumentNullException("设置服务地址解析器时,必须设置服务名!");
            }
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
                Refresh();
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

            //TODO 添加缓存,对比是否有差异,没有差异则不Listener
            var endPoints = await exitus.EndpointDiscovery.FindServiceEndpointsAsync();
            if (endPoints != null)
            {
                var addresses = endPoints.Select(s => s.Address).ToArray();
                Listener(ResolverResult.ForResult(addresses));
            }
        }
    }
}
