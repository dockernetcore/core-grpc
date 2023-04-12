using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;

namespace Overt.Core.Grpc.H2
{
    public class Constants
    {
        /// <summary>
        /// Grpc Server 节点名称
        /// </summary>
        internal const string GrpcServerSectionName = "GrpcServer";
        /// <summary>
        /// Grpc Consul 节点名称
        /// </summary>
        internal const string ConsulServerSectionName = "ConsulServer";
        /// <summary>
        /// Grpc Client 节点名称
        /// </summary>
        internal const string GrpcClientSectionName = "GrpcClient";

        ///// <summary>
        ///// 默认的通道配置
        ///// </summary>
        //public static GrpcChannelOptions DefaultChannelOptions = new GrpcChannelOptions()
        //{
        //    Credentials = ChannelCredentials.Insecure,
        //    MaxReceiveMessageSize = int.MaxValue,
        //    MaxSendMessageSize = int.MaxValue,
        //    HttpHandler = new SocketsHttpHandler
        //    {
        //        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
        //        KeepAlivePingDelay = TimeSpan.FromSeconds(30),
        //        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
        //        KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
        //        EnableMultipleHttp2Connections = true
        //    },
        //    ServiceConfig = DefaultServiceConfig,
        //};

        //public static ServiceConfig DefaultServiceConfig = new ServiceConfig()
        //{
        //    LoadBalancingConfigs = { new LoadBalancingConfig(ClientBalancer.Random) }
        //};

        /// <summary>
        /// 默认的通道配置
        /// </summary>
        public static GrpcChannelOptions DefaultChannelOptions()
        {
            return new GrpcChannelOptions()
            {
                Credentials = ChannelCredentials.Insecure,
                MaxReceiveMessageSize = int.MaxValue,
                MaxSendMessageSize = int.MaxValue,
                HttpHandler = new SocketsHttpHandler
                {
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(30),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
                    EnableMultipleHttp2Connections = true,
#if NET6_0
                    ActivityHeadersPropagator = DistributedContextPropagator.CreateNoOutputPropagator()
#endif
                },
                ServiceConfig = DefaultServiceConfig(),
            };
        }

        public static ServiceConfig DefaultServiceConfig()
        {
            return new ServiceConfig()
            {
                LoadBalancingConfigs = { new LoadBalancingConfig(ClientBalancer.Random) }
            };
        }
    }
}
