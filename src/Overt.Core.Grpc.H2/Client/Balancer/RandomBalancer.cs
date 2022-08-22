using Grpc.Net.Client.Balancer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Overt.Core.Grpc.H2
{
    public class RandomBalancer : SubchannelsLoadBalancer
    {
        static private long _times = 0;

        public RandomBalancer(IChannelControlHelper controller, ILoggerFactory loggerFactory)
            : base(controller, loggerFactory)
        {
        }
        protected override SubchannelPicker CreatePicker(IReadOnlyList<Subchannel> readySubchannels)
        {
            return new RandomPicker(readySubchannels);
        }

        private class RandomPicker : SubchannelPicker
        {
            private readonly IReadOnlyList<Subchannel> _subchannels;

            public RandomPicker(IReadOnlyList<Subchannel> subchannels)
            {
                _subchannels = subchannels;
            }

            public override PickResult Pick(PickContext context)
            {
#if NET6_0

                // Pick a random subchannel.
                return PickResult.ForSubchannel(_subchannels[Random.Shared.Next(0, _subchannels.Count)]);
#endif
#if NET5_0
                return PickResult.ForSubchannel(_subchannels[(int)(Interlocked.Increment(ref _times) % _subchannels.Count)]);
#endif
            }
        }
    }

    public class RandomBalancerFactory : LoadBalancerFactory
    {
        // Create a RandomBalancer when the name is 'random'.
        public override string Name => ClientBalancer.Random;

        public override LoadBalancer Create(LoadBalancerOptions options)
        {
            return new RandomBalancer(options.Controller, options.LoggerFactory);
        }
    }
}
