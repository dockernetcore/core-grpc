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
        }
        #endregion

        #region Public Property
        public GrpcClientOptions Options { get; set; }

        #endregion

        #region Public Method
        public async Task<List<AddressWrapper>> FindServiceEndpointsAsync(bool filterBlack = true)
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
                           .Where(item => !ServiceBlackPolicy.In(Options.ServiceName, item.Target) || !filterBlack)
                           .ToList();
            }
            catch
            {
                return null;
            }
            return targets;
        }
        #endregion
    }
}