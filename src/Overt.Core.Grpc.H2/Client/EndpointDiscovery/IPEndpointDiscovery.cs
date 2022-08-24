using Grpc.Net.Client.Balancer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Overt.Core.Grpc.H2
{
    /// <summary>
    /// IP服务发现
    /// </summary>
    internal class IPEndpointDiscovery : IEndpointDiscovery
    {
        #region Constructor
        private readonly List<Tuple<string, int>> _ipEndPoints;
        public IPEndpointDiscovery(GrpcClientOptions options, List<Tuple<string, int>> ipEndPoints)
        {
            if ((ipEndPoints?.Count ?? 0) <= 0)
                throw new ArgumentNullException("no ip endpoints availble");

            _ipEndPoints = ipEndPoints;

            Options = options;
        }
        #endregion

        #region Public Property
        public GrpcClientOptions Options { get; set; }

        public Action Watched { get; set; }

        #endregion

        #region Public Method
        public async Task<List<AddressWrapper>> FindServiceEndpointsAsync()
        {
            if ((_ipEndPoints?.Count ?? 0) <= 0)
                throw new ArgumentOutOfRangeException("endpoint not provide");

            var targets = _ipEndPoints.Select(x => new AddressWrapper("", new BalancerAddress(x.Item1, x.Item2)))
                                      .ToList();
            return await Task.FromResult(targets);
        }
        #endregion
    }
}
