﻿using Consul;
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
    public class ConsulResolver : PollingResolver 
    {
        public readonly static ConcurrentDictionary<string,List<AddressWrapper>> _currentAddressWrapper = new ConcurrentDictionary<string, List<AddressWrapper>>();

        Timer _timer;
        readonly ILoggerFactory _loggerFactory;
        readonly Uri _address;


        public ConsulResolver(Uri address, ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _address = address;

            #region 添加监听回调
            var serviceName = _address.LocalPath.Replace("/", "");
            var exitus = StrategyFactory.Get(serviceName);
            if (exitus == null)
                throw new Exception($"{serviceName} 配置异常");

            exitus.EndpointDiscovery.Watched = () =>
            {
                Refresh();
            };
            #endregion
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
                _loggerFactory.CreateLogger("resolver").LogError(ex, $"OnTimerCallback serviceName:{_address}");
            }
        }


        protected override async Task ResolveAsync(CancellationToken cancellationToken)
        {
            try
            {
                var serviceName = _address.LocalPath.Replace("/", "");
                var exitus = StrategyFactory.Get(serviceName);
                if (exitus?.EndpointDiscovery == null)
                { 
                    throw new ArgumentNullException("No Service Discovery Policy Obtained");
                }

                var endPoints = await exitus.EndpointDiscovery.FindServiceEndpointsAsync();
                if (endPoints == null)
                {
                    Listener(ResolverResult.ForResult(null));
                    return;
                }

                var addresses = endPoints.Select(s => s.Address).ToArray();
                Listener(ResolverResult.ForResult(addresses));
            }
            catch(Exception ex)
            {
                _loggerFactory.CreateLogger("resolver").LogError(ex, $"ResolveAsync serviceName:{_address}");
            }
        }
    }
}
