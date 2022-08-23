using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Grpc.Net.Client.Balancer;
using Microsoft.Extensions.Options;
using Grpc.Core;

namespace Overt.Core.Grpc.H2
{
    public class ConsulResolverFactory: ResolverFactory 
    {
        public ConsulResolverFactory()
        {
        }

        public override string Name => "consul";

        public override Resolver Create(ResolverOptions options)
        {
            return new ConsulResolver(options.Address, options.LoggerFactory);
        }
    }
}
