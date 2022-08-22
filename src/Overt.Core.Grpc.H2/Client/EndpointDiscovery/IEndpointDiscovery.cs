using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Overt.Core.Grpc.H2
{
    /// <summary>
    /// 重点服务发现接口
    /// </summary>
    public interface IEndpointDiscovery
    {
        /// <summary>
        /// 
        /// </summary>
        GrpcClientOptions Options { get; set; }

        /// <summary>
        /// 获取服务可连接终结点
        /// </summary>
        /// <returns></returns>
        Task<List<AddressWrapper>> FindServiceEndpointsAsync();
    }
}
