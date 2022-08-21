using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Grpc.Net.Client.Balancer;
using Microsoft.Extensions.Options;
using Grpc.Core;

namespace Overt.Core.Grpc.H2
{
    public class ConsulResolverFactory<T> : ResolverFactory where T : ClientBase
    {
        GrpcClientOptions<T> _options;
        public ConsulResolverFactory(IOptions<GrpcClientOptions<T>> options)
        {
            _options = options.Value;
        }

        public override string Name => typeof(T).Name;

        public override Resolver Create(ResolverOptions options)
        {
            return new ConsulResolver<T>(_options, options.LoggerFactory);
        }
    }
}
