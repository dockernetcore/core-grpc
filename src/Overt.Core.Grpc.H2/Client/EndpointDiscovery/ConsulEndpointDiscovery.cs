using Consul;
using Grpc.Net.Client.Balancer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Overt.Core.Grpc.H2
{
    /// <summary>
    /// Consul注册中心服务发现
    /// </summary>
    internal sealed class ConsulEndpointDiscovery : IEndpointDiscovery
    {
        #region 构造函数
        private readonly ConsulClient _client;
        public ConsulEndpointDiscovery(GrpcClientOptions options, string address, bool startWatch = true)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException("consul address");

            _client = new ConsulClient((cfg) =>
            {
                var uriBuilder = new UriBuilder(address);
                cfg.Address = uriBuilder.Uri;
            });

            Options = options;

            //开启监听
            StartWatchService();
        }
        #endregion

        #region Public Property
        public GrpcClientOptions Options { get; set; }

        public Action Watched { get; set; }

        #endregion

        #region Public Method
        public async Task<List<AddressWrapper>> FindServiceEndpointsAsync()
        {
            if (_client == null)
                throw new ArgumentNullException("consul client");

            var targets = new List<AddressWrapper>();
            try
            {
                var r = await _client.Health.Service(Options.ServiceName, "", true);
                if (r.StatusCode != HttpStatusCode.OK)
                    throw new ApplicationException($"failed to query consul server");

                targets = r.Response
                           .Select(x => new AddressWrapper(x.Service.ID, new BalancerAddress(x.Service.Address, x.Service.Port)))
                           .ToList();
            }
            catch
            {
                throw;
            }
            return targets;
        }
        #endregion

        /// <summary>
        /// 开始监听服务变动
        /// </summary>
        private void StartWatchService()
        {
            Task.Factory.StartNew(async () =>
            {
                ulong lastWaitIndex = 0;
                do
                {
                    try
                    {
                        var serviceQueryResult = await _client.Catalog.Service(Options.ServiceName, "", new QueryOptions()
                        {
                            WaitTime = TimeSpan.FromSeconds(30),
                            WaitIndex = lastWaitIndex
                        });
                        var waitIndex = serviceQueryResult.LastIndex;
                        if (lastWaitIndex <= 0)
                        {
                            lastWaitIndex = waitIndex;
                            continue;
                        }
                        if (waitIndex == lastWaitIndex)
                            continue;

                        // 重置服务
                        lastWaitIndex = waitIndex;
                        if (Watched != null)
                            Watched.Invoke();
                    }
                    catch { }

                } while (true);
            });
        }
    }
}